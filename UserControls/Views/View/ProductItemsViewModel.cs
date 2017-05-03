using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using UserControls.Helpers;

namespace UserControls.Views.View
{
    public class ProductItemsViewModel : DocumentViewModel
    {
        #region Internal Properties
        private string _filterText=string.Empty;
        Timer _timer = null;
        private List<ProductItemModel> _productItems;
        private List<ProductOrderModel> _items;
        private List<StockModel> _stocks;
        private bool _isLoading;
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
                _filterText = value.ToLower();
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        public decimal Count { get { return _items != null ? _items.GroupBy(s => s.Code).Count() : 0; } }
        public decimal Quantity { get { return _items != null ? _items.Sum(s => s.ExistingQuantity) : 0; } }
        public decimal CostPrice { get { return _items != null ? _items.Sum(s => s.ExistingQuantity * (s.Product.CostPrice ?? 0)) : 0; } }
        public decimal Price { get { return _items != null ? _items.Sum(s => s.ExistingQuantity * s.Product.Price ?? 0) : 0; } }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (_isLoading == value)
                {
                    return;
                }
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        public ObservableCollection<ProductOrderModel> Items
        {
            get
            {
                return _items == null ? new ObservableCollection<ProductOrderModel>() 
                    : new ObservableCollection<ProductOrderModel>(_items.Where(s=>s.Code.ToLower().Contains(FilterText) || s.Description.ToLower().Contains(FilterText)));
            }
        }

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
        }
        #endregion
        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքների դիտում";
            OnUpdate(null);
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
        private void Update(object o)
        {
            IsLoading = true;
            _productItems = new ProductsManager().GetProductItems(ApplicationManager.Instance.GetEsMember.Id);
            if (_stocks != null)
                _productItems = _productItems.Where(pi => pi.StockId != null && _stocks.Select(s => s.Id).ToList().Contains((long)pi.StockId)).ToList();
            _items = (from item in _productItems
                      group item by item.Product.Code + item.Product.Price into product
                      where product != null
                      orderby product.First().Product.Code
                      select
                          new ProductOrderModel
                          {
                              Product = product.First().Product,
                              Code = product.First().Product.Code,
                              Description = product.First().Product.Description,
                              Mu = product.First().Product.Mu,
                              ExistingQuantity = product.Sum(s => s.Quantity),
                              //Պահեստ = _stocks.First(t=>t.Id==product.Select(s=>s.StockId).FullName
                          }).ToList();
            RaisePropertyChanged("Items");
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("CostPrice");
            RaisePropertyChanged("Price");
            IsLoading = false;
        }

        private void OnUpdate(object o)
        {
            var td = new Thread(() => Update(o));
            td.Start();
        }
        private bool CanExport(object o)
        {
            return Items != null && Items.Count > 0;
        }
        private bool CanPrint(object o)
        {
            return Items != null && Items.Count > 0;
        }
        private void OnExport(object o)
        {
            if (!CanExport(o)) { return;}
            ExcelExportManager.ExportProducts(Items.ToList());
        }

        private void OnPrint(object o)
        {
            var model = (from item in Items.ToList()
                select
                    new
                    {
                        Կոդ = item.Code,
                        Անվանում = item.Description,
                        Չմ = item.Mu,
                        Քանակ = item.ExistingQuantity.ToString("# ##0.##"),
                        Գին = item.Product != null && item.Product.Price!=null? ((decimal)item.Product.Price).ToString("# ##0.##") : "0",
                        Գումար = item.Amount.ToString("# ##0.##")
                        //Պահեստ = _stocks.FirstOrDefault(t=>t.Id==product.Select(s=>s.StockId).FirstOrDefault()).FullName
                    }).ToList();
            var columns = model.Count>0 ? model.First().GetType().GetProperties() : new PropertyInfo[0];
            var dgCtrl = new DataGrid{AutoGenerateColumns = false};
            foreach (var column in columns.Select(item => new DataGridTextColumn
            {
                Header = item.Name,
                Binding = new Binding(item.Name) { Mode = BindingMode.OneWay}
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
