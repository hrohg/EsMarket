using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;

namespace UserControls.ViewModels
{
    public class TableViewModel<T> : DocumentViewModel
    {
        #region Constants

        public const string IsInProgressProperty = "IsInProgress";

        #endregion

        #region Internal properties

        private string _title;
        private string _description;
        private bool _isLoading;
        private bool _isShowNulls;
        private int _totalRows;
        private double _totalCount;
        private double _total;
        #endregion

        #region External Properties
        
        public override int TotalRows { get { return _totalRows; } set { _totalRows = value; RaisePropertyChanged("TotalRows"); } }
        public override double TotalCount { get { return _totalCount; } set { _totalCount = value; RaisePropertyChanged("TotalCount"); } }
        public override double Total { get { return _total; } set { _total = value; RaisePropertyChanged("Total"); } }
        
        public bool IsShowNulls
        {
            get
            {
                return _isShowNulls;
            }
            set
            {
                if (value == _isShowNulls) { return; }
                _isShowNulls = value;
                RaisePropertyChanged("ViewList");
            }
        }
        public bool IsShowUpdateButton { get; set; }
        public bool IsShowCloseButton { get; set; }
        public virtual ObservableCollection<T> ViewList { get; protected set; }
        #endregion

        #region Constructors
        public TableViewModel(List<T> list):this()
        {
            Initialize();
            IsShowUpdateButton = true;
            IsShowCloseButton = true;
        }
        public TableViewModel()
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            ViewList = new ObservableCollection<T>();
        }

        protected virtual bool CanExportToExcel(object o) { return ViewList != null && ViewList.Any(); }
        protected virtual void OnExportToExcel(object o)
        {
            if (!CanExportToExcel(o)) { return; }
        }
        protected virtual void OnUpdate(object o)
        { }
        protected virtual bool CanPrint(object o) { return o != null && ViewList != null && ViewList.Any(); }
        protected virtual void OnPrint(object o)
        {
            if (!CanPrint(o)) { return; }
        }
        #endregion

        #region External methods

        #endregion

        #region Commands

        private ICommand _exportToExcelCommand;
        public ICommand ExportToExcelCommand { get { return _exportToExcelCommand?? (_exportToExcelCommand = new RelayCommand(OnExportToExcel, CanExportToExcel)); } }
        private ICommand _printCommand;
        public ICommand PrintCommand { get { return _printCommand ?? (_printCommand = new RelayCommand(OnPrint, CanPrint)); } }
        private ICommand _updateCommand;
        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(OnUpdate)); } }
        #endregion
    }

    public class InvoiceReportModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get { return Quantity * Price; } }
    }
    public class ProductProviderReportModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string Partner { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get { return Quantity * Price; } }
    }


    public class ProductOrderBySaleViewModel : TableViewModel<ProductOrderModel>
    {
        #region Internal properties
        private List<ProductOrderModel> _items = new List<ProductOrderModel>();
        #endregion
        #region External properties
        public override ObservableCollection<ProductOrderModel> ViewList
        {
            get
            {
                return new ObservableCollection<ProductOrderModel>(_items.Where(s => IsShowNulls || s.NeededQuantity > 0).ToList());
            }
        }

        #endregion
        public ProductOrderBySaleViewModel(object o)
            : base()
        {
            IsShowUpdateButton = true;
            Initialize(o);
        }

        #region Internal methods

        private void Initialize(object o)
        {
            OnUpdate(o);
        }
        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {

            var invoices = InvoicesManager.GetInvoices((DateTime)dateIntermediate.Item1, (DateTime)dateIntermediate.Item2);
            var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Where(s => s.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Select(s => s.Id));
            var productItems = new ProductsManager().GetProductItems(ApplicationManager.Instance.GetMember.Id);
            var productOrder = invoiceItems.GroupBy(s => s.ProductId).Where(s => s.FirstOrDefault() != null).Select(s =>
                new ProductOrderModel
                {
                    ProductId = s.First().ProductId,
                    Code = s.First().Product.Code,
                    Description = s.First().Product.Description,
                    Mu = s.First().Product.Mu,
                    MinPrice = s.First().Product.CostPrice,
                    MinQuantity = s.First().Product.MinQuantity,
                    ExistingQuantity = productItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity),
                    SaleQuantity = invoiceItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity ?? 0),
                    Note = s.First().Product.Note
                }).ToList();
            productOrder = productOrder.Where(s => s.NeededQuantity >= 0).ToList();
            var productProviders = ProductsManager.GetProductsProviders(productOrder.Select(s => s.ProductId).ToList());
            var providers = PartnersManager.GetPartners(productProviders.Select(s => s.ProviderId).ToList());
            foreach (var item in productOrder)
            {
                var productProvider = productProviders.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (productProvider == null) continue;
                var provider = providers.FirstOrDefault(s => s.Id == productProvider.ProviderId);
                if (provider == null) continue;
                item.Provider = provider.FullName;
            }
            _items = productOrder.OrderBy(s => s.Provider).ThenBy(s => s.Description).ThenBy(s => s.Code).ToList();
            TotalRows = _items.Count;
            TotalCount = (double)_items.Sum(s => s.ExistingQuantity);
            Total = (double)_items.Sum(i => i.Amount);
            RaisePropertyChanged("ViewList");
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
            
        }
        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate==null) return;
            Description = string.Format("Պատվեր ըստ վաճառքի և մնացորդի (մանրամասն) {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            var thread = new Thread(() => Update(dateIntermediate));
            thread.Start();
            IsLoading = true;
        }
        protected override void OnExportToExcel(object o)
        {
            base.OnExportToExcel(o);
            ExcelExportManager.ExportList(ViewList);
        }
        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null) { return; }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }

        #endregion
    }
}
