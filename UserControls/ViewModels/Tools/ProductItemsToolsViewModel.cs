using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.Enumerations;
using Xceed.Wpf.AvalonDock.Layout;

namespace UserControls.ViewModels.Tools
{
    public class ProductItemsToolsViewModel : ToolsViewModel
    {
        #region Events

        public delegate void OnSelectProductItemDelegate(ProductItemModel productItem);
        public event OnSelectProductItemDelegate OnProductItemSelected;

        public delegate void OnManageProductDelegate(ProductModel product);
        public event OnManageProductDelegate OnManageProduct;
        #endregion Events

        #region Internal proeprties

        private List<ProductModel> _products;
        #endregion Internal properties

        #region External properties

        #region Filter

        private Timer _timer;
        private string _filter;
        public string Filter
        {
            get { return _filter ?? string.Empty; }
            set
            {
                _filter = value;
                RaisePropertyChanged("Filter");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged("Items");
            DisposeTimer();
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        #endregion Filter

        #region Items

        private List<ProductNodes> _items;

        public List<ProductNodes> Items
        {
            get
            {
                return _items != null ?
                    _items.Where(s => string.IsNullOrEmpty(Filter) || string.Equals(s.Metadata, Filter, StringComparison.InvariantCultureIgnoreCase)).ToList() :
                    new List<ProductNodes>();
            }
            private set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
        #endregion Items

        public CustomItem SelectedItem { get; set; }
        public List<ItemsToSelect> ProductsViewModes { get; set; }

        private ItemsToSelect _currentProductsViewMode;

        public ItemsToSelect CurrentProductsViewMode
        {
            get { return _currentProductsViewMode; }
            set
            {
                _currentProductsViewMode = value;
                UpdateProducts(false);
            }
        }

        #endregion External proeprties

        #region Constructors

        public ProductItemsToolsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքներ";
            IsFloating = true;
            CanFloat = true;
            AnchorSide = AnchorSide.Left;

            SelectItemCommand = new RelayCommand<ProductNodes>(OnSelectItem);
            EditProductCommand = new RelayCommand(OnManagingProduct, CanManageProduct);
            ApplicationManager.Instance.CashProvider.ProductsUpdateing += OnProductsUpdating;
            ApplicationManager.Instance.CashProvider.ProductUpdated += OnProductsUpdated;
            OnUpdateProducts(null);
            UpdateProductsCommand = new RelayCommand(OnUpdateProducts);
            _products = ApplicationManager.Instance.CashProvider.Products;


            ProductsViewModes = new List<ItemsToSelect>
        {
            new ItemsToSelect("By Products", ProductsViewEnum.ByProducts),
            new ItemsToSelect("By Product Items", ProductsViewEnum.ByProductItems),
            new ItemsToSelect("By Categories", ProductsViewEnum.ByCategories),
            new ItemsToSelect("By Stocks", ProductsViewEnum.ByStocks)
        };
            CurrentProductsViewMode = ProductsViewModes.FirstOrDefault();
        }

        private void OnProductsUpdating()
        {
            IsLoading = true;
        }

        private void OnProductsUpdated()
        {
            var productNode = new ProductNodes("Products", "Products");
            switch ((ProductsViewEnum)CurrentProductsViewMode.SelectedValue)
            {
                case ProductsViewEnum.ByStocks:
                    var productItems = ApplicationManager.CashManager.ProductItems;
                    foreach (var productItemModel in productItems.GroupBy(s => s.StockId))
                    {
                        var stock = ApplicationManager.CashManager.GetStocks.FirstOrDefault(s => s.Id == productItemModel.Key);
                        var nodes = new ProductNodes(stock != null ? stock.Name : "Անհայտ պահեստ", stock != null ? stock.FullName : "");
                        nodes.Nodes.AddRange(productItemModel.Select(s => new ProductNodes(s.Product)));
                        productNode.Nodes.Add(nodes);
                    }
                    Items.Add(productNode);
                    break;
                case ProductsViewEnum.ByDetile:
                    break;
                case ProductsViewEnum.ByProducts:
                    _products = ApplicationManager.CashManager.Products;
                    if (_products != null)
                    {
                        _products = _products.OrderBy(s => s.Description).ToList();
                        Items = _products.Select(p => new ProductNodes(p)).ToList();
                    }

                    //Items = _products.Select(p =>
                    //    new CustomItem(p.Id, string.Format("{0} {1} {2}", p.Code, p.Description, p.Price),
                    //        string.Format("{0} {1} {2} {3}", p.Code, p.Description, p.Price, p.Barcode))
                    //    {
                    //        ProductGroups = p.ProductGroups != null ? p.ProductGroups.Select(t => t.Barcode).ToList() : new List<string>()
                    //    }).ToList();
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                case ProductsViewEnum.ByPrice:
                    break;
                case ProductsViewEnum.ByProviders:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            IsLoading = false;
            RaisePropertyChanged("Items");
        }

        public void OnUpdateProducts(object o)
        {
            ApplicationManager.Instance.CashProvider.UpdateProductsAsync();
        }
        private void OnSelectItem(ProductNodes productNode)
        {
            var product = _products.FirstOrDefault(s => s.Id == SelectedItem.Value);
            if (product == null) return;
            var handle = OnProductItemSelected;
            if (handle != null)
            {
                handle(new ProductItemModel { Id = Guid.Empty, Product = product, ProductId = product.Id });
            }
        }

        private bool CanManageProduct(object item)
        {
            return SelectedItem != null && ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        private void OnManagingProduct(object item)
        {
            if (!CanManageProduct(item)) return;
            var product = _products.SingleOrDefault(s => s.Id == SelectedItem.Value);
            if (product == null) return;
            ManageProduct(product);

        }
        private void ManageProduct(ProductModel product)
        {
            var handler = OnManageProduct;
            if (handler != null) handler(product);
        }
        #endregion Internal methods

        #region External methods
        public void UpdateProducts(bool update)
        {
            if (update)
            {
                OnUpdateProducts(null);
            }
            else
            {
                OnProductsUpdated();
            }
        }
        #endregion External methods

        #region Commands
        public ICommand SelectItemCommand { get; private set; }
        public ICommand EditProductCommand { get; private set; }
        public ICommand UpdateProductsCommand { get; private set; }
        #endregion Commands
    }

    public class CustomItem
    {
        public Guid Value { get; private set; }
        public string Description { get; private set; }
        public string Metadata { get; private set; }
        public List<string> ProductGroups { get; set; }
        public CustomItem(Guid id, string description, string metadata)
        {
            Value = id;
            Description = description;
            Metadata = metadata;

        }
    }

    public class FilterComparer : IEqualityComparer<String>
    {
        public bool Equals(string x, string y)
        {
            if (x != null && y != null)
            {
                return x.ToLower().Contains(y.ToLower());
            }
            return x == y;
        }

        public int GetHashCode(string obj)
        {
            throw new NotImplementedException();
        }
    }

    public class ProductNodes
    {
        private string _description;
        private string _name;

        public string Name
        {
            get { return _name ?? string.Format("{0} ({1}) {2} {3}", Product.Code, Product.Barcode, Product.Description, Product.Price); }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public List<ProductNodes> Nodes { get; set; }
        public string Metadata { get; set; }
        public ProductModel Product { get; set; }
        public ProductItemModel ProductItem { get; set; }

        public ProductNodes(ProductModel product)
            : this()
        {
            Product = product;
        }
        public ProductNodes(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }

        private ProductNodes()
        {
            Nodes = new List<ProductNodes>();
        }
    }
}
