using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.ViewModels;
using UserControls.Views;

namespace ES.Market.Views.Reports.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        #region Constants
        private const string IsInProgressProperty = "IsInProgress";
        #endregion
        #region Internal properties
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
            ViewInternalWayBillCommands = new RelayCommand<ViewInvoicesEnum>(OnViewViewInternalWayBillCommands, CanViewViewInternalWayBillCommands);
            SallByCustomersCommand = new RelayCommand(OnSallByCustomers);
        }

        private void OnSallByCustomers(object o)
        {
            var vm = new ReportBaseViewModel();
            vm.OnUpdate += UpdateSallByCustomers;
            vm.Update();
            var tabControl = new UctrlViewTable { DataContext = vm };
            AddTab(tabControl, vm);
        }

        private List<InvoiceReport> UpdateSallByCustomers(Tuple<DateTime, DateTime> dateIntermediate)
        {
            return InvoicesManager.GetSaleByPartners(dateIntermediate, ApplicationManager.Instance.GetMember.Id);
        }
        private void AddTab<T>(TableViewModel<T> vm)
        {
            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null || vm == null) { return; }
            var content = new UctrlViewTable(vm) { DataContext = vm };
            var nextTab = tab.Items.Add(new TabItem
            {
                Header = vm.Title,
                Content = content,
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

        private bool CanViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            switch (o)
            {
                case ViewInvoicesEnum.None:
                case ViewInvoicesEnum.ByDetiles:
                    return true;
                case ViewInvoicesEnum.ByStock:
                    return false;


                default:
                    return false;
            }
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
                default:
                    break;
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTab(tabControl, viewModel);
        }

        private void OnViewSale(ViewInvoicesEnum type)
        {
            DocumentViewModel viewModel = null;
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
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTab(tabControl, viewModel);
        }
        private void OnViewPurchase(ViewInvoicesEnum type)
        {
            DocumentViewModel viewModel = null;
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
