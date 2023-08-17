using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using Binding = System.Windows.Data.Binding;
using DataGrid = System.Windows.Controls.DataGrid;
using Timer = System.Threading.Timer;
using UserControl = System.Windows.Controls.UserControl;

namespace UserControls.Views.View
{
    public class ProductItemsViewModel : DocumentViewModel
    {
        #region Internal Properties
        private string _filterText = string.Empty;
        Timer _timer = null;
        private List<ProductItemModel> _productItems;
        //private List<ProductOrderModel> _items;
        private List<StockModel> _stocks;
        #endregion

        #region External properties
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        public decimal Count { get { return GetViewItems().GroupBy(s => s.Code).Count(); } }
        public decimal Quantity { get { return GetViewItems().Sum(s => s.ExistingQuantity); } }
        public decimal CostPrice { get { return GetViewItems().Sum(s => s.ExistingQuantity * (s.Product.CostPrice ?? 0)); } }
        public decimal Price { get { return GetViewItems().Sum(s => s.ExistingQuantity * s.Product.Price ?? 0); } }

        public CollectionViewSource ItemsView { get; set; }
        private ObservableCollection<ProductOrderModel> Items { get; set; }
        public List<StockModel> Stocks { get { return _stocks ?? new List<StockModel>(); } }
        public ObservableCollection<Controls.ProductDataGridColumnHeaderMetadata> ColumnHeaderMetadatas { get; protected set; }
        #endregion

        #region Constructors

        public ProductItemsViewModel()
        {
            Initialize();
        }
        public ProductItemsViewModel(List<StockModel> stocks)
            : this()
        {
            _stocks = stocks;
            OnUpdate(null);
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            Items = new ObservableCollection<ProductOrderModel>();
            Items.CollectionChanged += OnItemsChanged;
            ItemsView = new CollectionViewSource { Source = Items };
            ColumnHeaderMetadatas = new ObservableCollection<Controls.ProductDataGridColumnHeaderMetadata>();
            
            ItemsView.View.Filter = Filter;
            Title = "Ապրանքների դիտում ըստ պահեստների";
            var tooltip = string.Empty;
            foreach (var stockModel in Stocks)
            {
                tooltip += string.IsNullOrEmpty(tooltip) ? "" : ", " + stockModel.FullName;
            }
            Tooltip = string.Format("Պահեստներ ({0})", tooltip);
        }

        private List<ProductOrderModel> GetViewItems()
        {
            return ItemsView.View.OfType<ProductOrderModel>().ToList();
        }
        private void TimerElapsed(object obj)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                ItemsView.View.Refresh();
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Quantity");
                RaisePropertyChanged("CostPrice");
                RaisePropertyChanged("Price");
            });
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
        protected bool Filter(object item)
        {
            var product = item as ProductOrderModel;
            if (product == null) return false;
            return product.HasKey(FilterText);
        }
        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("CostPrice");
            RaisePropertyChanged("Price");
            RaisePropertyChanged("ItemsView");
        }
        private void Update(object o)
        {
            if (_stocks == null || !_stocks.Any() || IsLoading) return;
            IsLoading = true;
            string productKey = null;

            ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Կոդ", BindingText = "Code", CanSort = true, SortKey = "Կոդ" });
            ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Անվանում", BindingText = "Description", CanSort = true, SortKey = "Անվանում" });
            ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Չմ", BindingText = "Mu", CanSort = true, SortKey = "Չմ" });
            ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Գին", BindingText = "Price", CanSort = true, SortKey = "Գին" });
            ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Առկա", BindingText = "ExistingQuantity", CanSort = true, SortKey = "Առկա" });

            foreach (var stock in Stocks)
            {
                ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = stock.Name, BindingText = "StockProducts", CanSort = true, SortKey = stock.Name, ConverterParameter = stock.Name, Converter = new Controls.StockProductsConverter() });
            }


            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                Items.Clear();
                var inputBox = new InputBox("Input product key");
                var inputBoxResult = inputBox.ShowDialog();
                if (inputBoxResult != null && (bool)inputBoxResult)
                {
                    productKey = inputBox.InputValue;
                }
            });
            _productItems = ProductsManager.GetProductItemsByStocks(_stocks.Select(s => s.Id).ToList(), productKey);
            //_productItems = _productItems.Where(pi => pi.StockId != null && _stocks.Select(s => s.Id).ToList().Contains((short)pi.StockId)).ToList();
            var items = (from item in _productItems
                         group item by new { item.ProductId, item.CostPrice }
                             into product
                         where product != null
                         select product).ToList();

            foreach (var productItem in items)
            {
                var firstItem = productItem.First();
                if (firstItem == null || firstItem.Product == null) continue;
                var productOrderModel = new ProductOrderModel
                {
                    Product = productItem.First().Product,
                    Code = productItem.First().Product.Code,
                    Description = productItem.First().Product.Description,
                    Mu = productItem.First().Product.Mu,
                    CostPrice = productItem.First().CostPrice,
                    Price = productItem.First().Product.Price,
                    ExistingQuantity = productItem.Sum(s => s.Quantity)
                };
                foreach (var stockModel in _stocks)
                {
                    productOrderModel.StockProducts.Add(
                        new StockProducts
                        {
                            Product = productItem.First(),
                            Stock = stockModel,
                            Quantity = productItem.Where(s => s.StockId == stockModel.Id).Sum(s => s.Quantity)
                        });
                }
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    Items.Add(productOrderModel);
                });
            }
            IsLoading = false;

        }

        private void OnUpdate(object o)
        {
            var td = new Thread(() => Update(o));
            td.Start();
        }
        private bool CanExport(object o)
        {
            return GetViewItems().Any();
        }
        private bool CanPrint(object o)
        {
            return GetViewItems().Any();
        }
        private void OnExport(object o)
        {
            if (!CanExport(o)) { return; }
            ExcelExportManager.ExportProducts(GetViewItems());
        }

        private void OnPrint(object o)
        {
            var model = (from item in GetViewItems()
                         select
                             new
                             {
                                 Կոդ = item.Code,
                                 Անվանում = item.Description,
                                 Չմ = item.Mu,
                                 Քանակ = item.ExistingQuantity.ToString("# ##0.###"),
                                 Գին = item.Product != null && item.Product.Price != null ? ((decimal)item.Product.Price).ToString("# ##0.##") : "0",
                                 Գումար = item.Amount.ToString("# ##0.##")
                                 //Պահեստ = _stocks.FirstOrDefault(t=>t.Id==product.Select(s=>s.StockId).FirstOrDefault()).FullName
                             }).ToList();
            var columns = model.Count > 0 ? model.First().GetType().GetProperties() : new PropertyInfo[0];
            var dgCtrl = new DataGrid { AutoGenerateColumns = false };
            foreach (var column in columns.Select(item => new DataGridTextColumn
            {
                Header = item.Name,
                Binding = new Binding(item.Name) { Mode = BindingMode.OneWay }
            }))
            {
                dgCtrl.Columns.Add(column);
            }
            dgCtrl.ItemsSource = model;
            //new UiPrintPreview(dgCtrl).Show();
            var ctrl = new UserControl();
            ctrl.Content = dgCtrl;
            PrintManager.PrintPreview(ctrl, "Print product list", true);
        }

        #endregion

        #region External methods
        #endregion

        #region Commands
        public ICommand UpdateCommand { get { return new RelayCommand(OnUpdate); } }
        public ICommand ExportCommand { get { return new RelayCommand(OnExport, CanExport); } }
        public ICommand PrintCommand { get { return new RelayCommand(OnPrint, CanPrint); } }
        #endregion

    }

    public class ProductItemsViewByDetileViewModel : DocumentViewModel
    {
        #region Internal Properties
        private string _filterText = string.Empty;
        Timer _timer = null;
        private List<ProductItemModel> _productItems;
        private ObservableCollection<ProductOrderModel> Items;
        private List<StockModel> _stocks;

        #endregion

        #region External properties
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        public decimal Quantity { get { return GetViewItems().Sum(s => s.ExistingQuantity); } }
        public decimal CostPrice { get { return GetViewItems().Sum(s => s.ExistingQuantity * (s.Product.CostPrice ?? 0)); } }
        public decimal Price { get { return GetViewItems().Sum(s => s.ExistingQuantity * s.Product.Price ?? 0); } }
        public decimal Count { get { return GetViewItems().GroupBy(s => s.Code).Count(); } }

        public CollectionViewSource ItemsView { get; private set; }
        public ObservableCollection<StockModel> Stocks { get { return new ObservableCollection<StockModel>(_stocks ?? new List<StockModel>()); } }
        #endregion

        #region Constructors

        public ProductItemsViewByDetileViewModel()
        {
            Initialize();
        }
        public ProductItemsViewByDetileViewModel(List<StockModel> stocks)
            : this()
        {
            _stocks = stocks;
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            Items = new ObservableCollection<ProductOrderModel>();
            Items.CollectionChanged += OnItemsChanged;
            ItemsView = new CollectionViewSource { Source = Items };
            ItemsView.View.Filter = Filter;
            Title = "Ապրանքների դիտում մանրամասն";
            var tooltip = string.Empty;
            foreach (var stockModel in Stocks)
            {
                tooltip += string.IsNullOrEmpty(tooltip) ? "" : ", " + stockModel.FullName;
            }
            Tooltip = string.Format("Պահեստներ ({0})", tooltip);

            OnUpdate(null);
        }

        protected List<ProductOrderModel> GetViewItems()
        {
            return ItemsView.View.OfType<ProductOrderModel>().ToList();
        }
        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("CostPrice");
            RaisePropertyChanged("Price");
            RaisePropertyChanged("ItemsView");
        }
        private void TimerElapsed(object obj)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                ItemsView.View.Refresh();
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Quantity");
                RaisePropertyChanged("CostPrice");
                RaisePropertyChanged("Price");
            });
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

        protected bool Filter(object item)
        {
            var product = item as ProductOrderModel;
            if (product == null) return false;
            return product.HasKey(FilterText);
        }
        private void Update(object o)
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                Items.Clear();
                _stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(), true);
            });

            if (_stocks == null || !_stocks.Any() || IsLoading) return;
            IsLoading = true;
            _productItems = ProductsManager.GetProductItems();
            _productItems = _productItems.Where(pi => pi.StockId != null && _stocks.Select(s => s.Id).ToList().Contains((short)pi.StockId)).ToList();
            var items = (from item in _productItems
                         group item by item.ProductId
                             into product
                         where product != null
                         select product);

            foreach (var item in items)
            {
                var product = item.First().Product;
                var productOrderModel = product != null ? new ProductOrderModel
                {
                    Product = product,
                    Code = product.Code,
                    Description = product.Description,
                    Mu = product.Mu,
                    ExistingQuantity = item.Sum(s => s.Quantity)
                } :
                    new ProductOrderModel
                    {
                        Product = product,
                        Code = item.First().ProductId.ToString(),
                        ExistingQuantity = item.Sum(s => s.Quantity)
                    };

                foreach (var stockModel in _stocks)
                {
                    productOrderModel.StockProducts.Add(
                        new StockProducts
                        {
                            Product = item.First(),
                            Stock = stockModel,
                            Quantity = item.Where(s => s.StockId == stockModel.Id).Sum(s => s.Quantity)
                        });
                }
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { Items.Add(productOrderModel); });
            }
            IsLoading = false;
        }
        private void OnUpdate(object o)
        {
            var td = new Thread(() => Update(o));
            td.Start();
        }
        private bool CanExport(object o)
        {

            return GetViewItems().Any();
        }
        private bool CanPrint(object o)
        {
            return GetViewItems().Any();
        }
        private void OnExport(object o)
        {
            if (!CanExport(o)) { return; }
            ExcelExportManager.ExportProducts(GetViewItems());
        }

        private void OnPrint(object o)
        {
            var model = (from item in GetViewItems()
                         select
                             new
                             {
                                 Կոդ = item.Code,
                                 Անվանում = item.Description,
                                 Չմ = item.Mu,
                                 Քանակ = item.ExistingQuantity.ToString("# ##0.###"),
                                 Գին = item.Product != null && item.Product.Price != null ? ((decimal)item.Product.Price).ToString("# ##0.##") : "0",
                                 Գումար = item.Amount.ToString("# ##0.##")
                                 //Պահեստ = _stocks.FirstOrDefault(t=>t.Id==product.Select(s=>s.StockId).FirstOrDefault()).FullName
                             }).ToList();
            var columns = model.Count > 0 ? model.First().GetType().GetProperties() : new PropertyInfo[0];
            var dgCtrl = new DataGrid { AutoGenerateColumns = false };
            foreach (var column in columns.Select(item => new DataGridTextColumn
            {
                Header = item.Name,
                Binding = new Binding(item.Name) { Mode = BindingMode.OneWay }
            }))
            {
                dgCtrl.Columns.Add(column);
            }
            dgCtrl.ItemsSource = model;
            //new UiPrintPreview(dgCtrl).Show();
            var ctrl = new UserControl();
            ctrl.Content = dgCtrl;
            PrintManager.PrintPreview(ctrl, "Print product list", true);
        }

        #endregion

        #region External methods
        #endregion

        #region Commands
        public ICommand UpdateCommand { get { return new RelayCommand(OnUpdate); } }
        public ICommand ExportCommand { get { return new RelayCommand(OnExport, CanExport); } }
        public ICommand PrintCommand { get { return new RelayCommand(OnPrint, CanPrint); } }
        #endregion

    }
}
