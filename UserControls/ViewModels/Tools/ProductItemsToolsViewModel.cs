using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.Enumerations;
using Xceed.Wpf.AvalonDock.Layout;
using ProductModel = ES.Data.Models.Products.ProductModel;

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

        private readonly object _sync = new object();
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
            foreach (var nodes in Items)
            {
                nodes.SetFilter(_filter);
            }
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
                lock (_sync)
                {
                    return _items != null ?
                    _items.Where(s => string.IsNullOrEmpty(Filter) || s.HasItems(Filter)).ToList() :
                    new List<ProductNodes>();
                }

            }
            private set
            {
                lock (_sync)
                {
                    _items = value;
                    RaisePropertyChanged("Items");
                }
            }
        }
        #endregion Items

        public ProductNodes SelectedItem { get; set; }
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
            _items = new List<ProductNodes>();
            SelectItemCommand = new RelayCommand<ProductModel>(OnSelectItem);
            EditProductCommand = new RelayCommand(OnManagingProduct, CanManageProduct);
            ApplicationManager.Instance.CashProvider.ProductsUpdateing += OnProductsUpdating;
            ApplicationManager.Instance.CashProvider.ProductUpdated += OnProductsUpdated;
            //OnUpdateProducts(null);
            UpdateProductsCommand = new RelayCommand(OnUpdateProducts);
            ProductsViewModes = new List<ItemsToSelect>
        {
            new ItemsToSelect("Ըստ ապրանքների", ProductsViewEnum.ByProducts),
           // new ItemsToSelect("By Product Items", ProductsViewEnum.ByProductItems),
            //new ItemsToSelect("By Categories", ProductsViewEnum.ByCategories),
            new ItemsToSelect("Ըստ բաժինների", ProductsViewEnum.ByStocks)
        };
            CurrentProductsViewMode = ProductsViewModes.FirstOrDefault();
        }

        private void OnProductsUpdating()
        {
            IsLoading = true;
        }

        private void OnProductsUpdated()
        {
         var items = new List<ProductNodes>();   
                var productNode = new ProductNodes("Products", "Products");
                switch ((ProductsViewEnum)CurrentProductsViewMode.SelectedValue)
                {
                    case ProductsViewEnum.ByStocks:
                        var productItems = ApplicationManager.CashManager.ProductItems;
                        foreach (var productItemModel in productItems.GroupBy(s => s.StockId))
                        {
                            var stock = ApplicationManager.CashManager.GetStocks.FirstOrDefault(s => s.Id == productItemModel.Key);
                            var nodes = new ProductNodes(stock != null ? stock.Name : "Անհայտ պահեստ", stock != null ? stock.FullName : "");
                            nodes.AddRange(productItemModel.Select(s => new ProductNodes(s.Product)).OrderBy(s => s.Name));
                            items.Add(nodes);
                        }

                        items = items.OrderBy(s => s.Name).ToList();
                        break;
                    case ProductsViewEnum.ByDetile:
                        break;
                    case ProductsViewEnum.ByProducts:
                        items = ApplicationManager.CashManager.GetProducts().OrderBy(s => s.Description).Select(p => new ProductNodes(p)).OrderBy(s => s.Name).ToList();
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
            lock (_sync)
            {
                _items.Clear();
                _items.AddRange(items);

            }
            IsLoading = false;
            RaisePropertyChanged("Items");
        }

        public void OnUpdateProducts(object o)
        {
            ApplicationManager.Instance.CashProvider.UpdateProductsAsync();
        }
        private void OnSelectItem(ProductModel product)
        {
            //var product = _products.FirstOrDefault(s => s.Id == SelectedItem.Value);
            if (product == null) return;
            var handle = OnProductItemSelected;
            if (handle != null)
            {
                handle(new ProductItemModel { Id = Guid.Empty, Product = product, ProductId = product.Id });
            }
        }

        private bool CanManageProduct(object item)
        {
            return SelectedItem != null && SelectedItem.Product != null && ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        private void OnManagingProduct(object item)
        {
            if (!CanManageProduct(item)) return;
            ManageProduct(CashManager.Instance.GetProducts().SingleOrDefault(s => s.Id == SelectedItem.Product.Id));
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
        public List<string> ProductKeyss { get; set; }
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
        private ProductNodes _parent;
        private string _description;
        private string _name;
        private string _filter;
        private List<ProductNodes> _childNodes;

        public string Name
        {
            get { return _name ?? string.Format("{0} ({1}) {2}", Product.Description, Product.Code, Product.Price); }
            set { _name = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public List<ProductNodes> ChildNodes
        {
            get { return _childNodes.Where(s => s.HasItems(_filter)).ToList(); }
        }

        public string Metadata { get; set; }

        public ProductModel Product { get; set; }
        public ProductItemModel ProductItem { get; set; }

        public ProductNodes(ProductModel product, ProductNodes parent = null)
            : this()
        {
            Product = product;
            Metadata = string.Format("{0} {1} {2} {3}",
                Name,
                product != null ? product.Barcode : string.Empty,
                product != null ? product.Price.ToString() : string.Empty,
                product != null ? product.ProductKeys.Aggregate("", (current, ProductKeysModel) => current + ProductKeysModel.ProductKey + " ") : string.Empty);
        }
        public ProductNodes(string name, string description)
            : this()
        {
            Name = name;
            Description = description;
        }
        private ProductNodes()
        {
            _childNodes = new List<ProductNodes>();
        }
        public bool HasItems(string filter)
        {
            return string.IsNullOrEmpty(filter) || (!string.IsNullOrEmpty(Metadata) && Metadata.ToLower().Contains(filter.ToLower())) || ChildNodes.Any(s => s.HasItems(filter));
        }
        public void SetFilter(string filter)
        {
            _filter = filter;
            foreach (var nodes in ChildNodes)
            {
                nodes.SetFilter(_filter);
            }
        }

        public void Add(ProductNodes item)
        {
            _childNodes.Add(item);
        }
        public void AddRange(IEnumerable<ProductNodes> items)
        {
            _childNodes.AddRange(items);
        }
    }
}
