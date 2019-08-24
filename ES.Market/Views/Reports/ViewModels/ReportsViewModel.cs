using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.Enumerations;
using UserControls.Interfaces;
using UserControls.ViewModels;
using UserControls.ViewModels.Reports;
using UserControls.Views;

namespace ES.Market.Views.Reports.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        #region Constants
        private const string IsInProgressProperty = "IsInProgress";
        #endregion

        #region Internal properties
        private readonly object _sync = new object();
        private DocumentViewModel _activeTab;
        private bool _isInProgress;
        private Window _parentTabControl;
        #endregion

        #region External properties
        public bool IsInProgress
        {
            get { return _isInProgress; }
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged(IsInProgressProperty);
            }
        }

        #region Avalon dock
        public ObservableCollection<DocumentViewModel> Documents { get; set; }
        public ObservableCollection<ToolsViewModel> Tools { get; set; }
        public DocumentViewModel ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                OnPropertyChanged("AddSingleVisibility");
            }
        }
        #endregion Avalon dock

        #endregion

        #region Constructors

        public ReportsViewModel(Window parentcontrol)
        {
            Initialize();
            _parentTabControl = parentcontrol;
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Documents = new ObservableCollection<DocumentViewModel>();
            ViewInternalWayBillCommands = new RelayCommand<ViewInvoicesEnum>(OnViewViewInternalWayBillCommands, CanViewViewInternalWayBillCommands);
            SallByCustomersCommand = new RelayCommand(OnSallByCustomers);
        }

        private void AddDocument(DocumentViewModel vm)
        {
            if (vm.IsClosable)
            {
                vm.OnClosed += OnRemoveDocument;
            }
            vm.ActiveTabChangedEvent += OnActiveTabChanged;
            vm.IsActive = true;
            vm.IsSelected = true;
            lock (_sync)
            {
                Documents.Add(vm);
            }

            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null) { return; }
            var content = new UctrlViewTable(vm) { DataContext = vm };
            vm.OnClosed += CloseTab;
            var nextTab = tab.Items.Add(new TabItem
            {
                Content = content,
                DataContext = vm,
                AllowDrop = true,
            });
            tab.SelectedIndex = nextTab;
        }
        private void AddTab<T>(TableViewModel<T> vm)
        {
            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null || vm == null) { return; }
            var content = new UctrlViewTable(vm) { DataContext = vm };
            vm.OnClosed += CloseTab;
            var nextTab = tab.Items.Add(new TabItem
            {
                Content = content,
                DataContext = vm,
                AllowDrop = true,
            });
            tab.SelectedIndex = nextTab;
        }
        private void AddTab(UserControl control, DocumentViewModel vm)
        {
            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null || vm == null) { return; }
            vm.OnClosed += CloseTab;
            var nextTab = tab.Items.Add(new TabItem
            {
                Content = control,
                DataContext = vm,
                AllowDrop = true
            });
            tab.SelectedIndex = nextTab;
        }
        private void CloseTab(PaneViewModel vm)
        {
            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null || vm == null) { return; }
            var tabItem = tab.Items.Cast<TabItem>().FirstOrDefault(s => s.DataContext == vm);
            if (tabItem == null) return;
            tab.Items.RemoveAt(tab.Items.IndexOf(tabItem));
        }

        private void OnActiveTabChanged(DocumentViewModel document, ActivityChangedEventArgs e)
        {
            foreach (var doc in Documents)
            {
                doc.IsActive = (doc == document && e.Value);
            }
            ActiveTab = document;
        }

        private void OnRemoveDocument(PaneViewModel vm)
        {
            if (vm == null) return;
            vm.OnClosed -= OnRemoveDocument;
            if (vm is DocumentViewModel)
            {
                ((DocumentViewModel)vm).ActiveTabChangedEvent -= OnActiveTabChanged;
            }
            Documents.Remove((DocumentViewModel)vm);
        }

        //Sale
        private void OnViewSale(ViewInvoicesEnum type)
        {
            TableViewModelBase viewModel = null;
            switch (type)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new InvoiceReportViewModel(new List<InvoiceType> { InvoiceType.SaleInvoice });
                    break;
                case ViewInvoicesEnum.ByDetiles:

                    break;
                case ViewInvoicesEnum.ByStock:
                    viewModel = new SaleInvoiceReportByStocksViewModel(type);
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                case ViewInvoicesEnum.ByPartner:
                    viewModel = new SaleInvoiceReportByPartnerViewModel(type);
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    viewModel = new SaleInvoiceReportByPartnersDetiledViewModel(type);
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    viewModel = new SaleInvoiceReportByStockDetiledViewModel(type);
                    break;
                case ViewInvoicesEnum.BySaleChart:
                    viewModel = new SaleInvoiceReportByChartViewModel(type);
                    break;
                case ViewInvoicesEnum.ByZeroAmunt:
                    viewModel = new SaleInvoiceReportTypeViewModel(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTab(tabControl, viewModel);
            if (viewModel != null) ((ITableViewModel)viewModel).Update();
        }
        //Purchase
        private void OnViewPurchase(ViewInvoicesEnum type)
        {
            TableViewModelBase viewModel = null;
            switch (type)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new InvoiceReportViewModel(new List<InvoiceType> { InvoiceType.PurchaseInvoice });
                    break;
                case ViewInvoicesEnum.ByDetiles:

                    break;
                case ViewInvoicesEnum.ByStock:
                    viewModel = new SaleInvoiceReportByStocksViewModel(type);
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                case ViewInvoicesEnum.ByPartner:
                    viewModel = new SaleInvoiceReportByPartnerViewModel(type);
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    viewModel = new SaleInvoiceReportByPartnersDetiledViewModel(type);
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    viewModel = new SaleInvoiceReportByStockDetiledViewModel(type);
                    break;
                case ViewInvoicesEnum.BySaleChart:
                    viewModel = new SaleInvoiceReportByChartViewModel(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTab(tabControl, viewModel);
            if (viewModel != null) ((ITableViewModel)viewModel).Update();
        }
        private void OnSallByCustomers(object o)
        {
            var vm = new ReportBaseViewModel();
            var tabControl = new UctrlViewTable { DataContext = vm };
            AddTab(tabControl, vm);
        }
        //WayBill
        private bool CanViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            switch (o)
            {
                case ViewInvoicesEnum.None:
                case ViewInvoicesEnum.ByDetiles:
                    return true;
                case ViewInvoicesEnum.ByStock:
                    return false;


                case ViewInvoicesEnum.ByPartnerType:
                    break;
                case ViewInvoicesEnum.ByPartner:
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    break;
                case ViewInvoicesEnum.BySaleChart:
                    break;
                case ViewInvoicesEnum.ByZeroAmunt:
                    break;
                default:
                    return false;
            }
            return false;
        }
        private void OnViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {

            DocumentViewModel viewModel = null;
            switch (o)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new ViewInternalWayBillViewModel(o);
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    viewModel = new InternalWayBillDetileViewModel(o);
                    break;
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTab(tabControl, viewModel);
        }
        #endregion

        #region External methods

        #endregion

        #region Commands

        public ICommand ViewInternalWayBillCommands { get; private set; }
        private ICommand _viewSaleCommand;
        public ICommand ViewSaleCommand { get { return _viewSaleCommand ?? (_viewSaleCommand = new RelayCommand<ViewInvoicesEnum>(OnViewSale)); } }
        private ICommand _viewPurchaseCommand;
        public ICommand ViewPurchaseCommand { get { return _viewPurchaseCommand ?? (_viewPurchaseCommand = new RelayCommand<ViewInvoicesEnum>(OnViewPurchase)); } }
        public ICommand SallByCustomersCommand { get; private set; }


        #region Products commands

        private ICommand _viewProductsCommand;
        public ICommand ViewProductsCommand { get { return _viewProductsCommand ?? (_viewProductsCommand = new RelayCommand<ProductsViewEnum>(OnViewProductsCommand)); } }

        private void OnViewProductsCommand(ProductsViewEnum viewEnum)
        {
            TableViewModel<IInvoiceReport> vm = null;
            DocumentViewModel vmDoc;
            switch (viewEnum)
            {
                case ProductsViewEnum.ByDetile:
                    vm = new ViewProductsBalanceViewModel(viewEnum);
                    break;
                case ProductsViewEnum.ByPrice:
                    vm = new ViewProductsBalanceViewModel(viewEnum);
                    break;
                case ProductsViewEnum.ByStocks:
                    vm = new ViewProductsResidualViewModel();
                    break;
                case ProductsViewEnum.ByProducts:
                    vmDoc = new ViewProductsBalanceViewModel(viewEnum);
                    AddDocument(vmDoc);
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                case ProductsViewEnum.ByProviders:
                    vmDoc = new ViewProductsViewModel();
                    AddDocument(vmDoc);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("viewEnum", viewEnum, null);
            }

            if (vm != null)
            {
                AddTab(vm);
                vm.Update();
            }
        }

        private ICommand _checkProductsRemainderForStockCommand;

        public ICommand CheckProductsRemainderForStockCommand
        {
            get { return _checkProductsRemainderForStockCommand ?? (_checkProductsRemainderForStockCommand = new RelayCommand(OnCheckProductsRemainderForStock, CanCheckProductsRemainderForStock)); }
        }

        private bool CanCheckProductsRemainderForStock(object obj)
        {
            return true;
        }

        private void OnCheckProductsRemainderForStock(object obj)
        {
            var vm = new CheckProductsRemainderByStockViewModel();
            vm.Update();
            AddTab(vm);
        }

        private ICommand _getFallowProductsCommand;

        public ICommand GetFallowProductsCommand
        {
            get { return _getFallowProductsCommand ?? (_getFallowProductsCommand = new RelayCommand(OnGetFalowProducts)); }
        }

        private void OnGetFalowProducts(object obj)
        {
            var vm = new FallowProductsViewModel();
            AddDocument(vm);
        }

        #endregion Products commands

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
