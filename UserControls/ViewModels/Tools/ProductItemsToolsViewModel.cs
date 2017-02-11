using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.Commands;
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
        private List<ProductItemModel> _productItems;
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
                _filter = value.ToLower();
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

        private List<CustomItem> _items;

        public List<CustomItem> Items
        {
            get { return _items.Where(s => s.Description.ToLower().Contains(Filter)).ToList(); }
            private set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }
        #endregion Items

        public CustomItem SelectedItem { get; set; }

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
            OnUpdateProducts(null);
            SelectItemCommand = new RelayCommand<Guid>(OnSelectItem);
            EditProductCommand = new RelayCommand(OnManagingProduct, CanManageProduct);
            UpdateProductsCommand = new RelayCommand(OnUpdateProducts);
        }
        public void OnUpdateProducts(object o)
        {
            _products = ApplicationManager.CashManager.Products.OrderBy(s => s.Description).ToList();
            Items = _products.Select(p => new CustomItem(p.Id, string.Format("{0} {1} {2}", p.Code, p.Description, p.Price))).ToList();
        }
        private void OnSelectItem(Guid id)
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
            return SelectedItem != null;
        }

        private void OnManagingProduct(object item)
        {
            if (!CanManageProduct(item)) return;
            var product = _products.SingleOrDefault(s => s.Id == SelectedItem.Value);
            if (product == null) return;
            ManageProduct((ProductModel)product);

        }
        private void ManageProduct(ProductModel product)
        {
            var handler = OnManageProduct;
            if (handler != null) handler(product);
        }
        #endregion Internal methods

        #region External methods
        public void UpdateProducts()
        {
            OnUpdateProducts(null);
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
        public Guid Value { get; set; }
        public string Description { get; set; }

        public CustomItem(Guid id, string description)
        {
            Value = id;
            Description = description;
        }
    }
}
