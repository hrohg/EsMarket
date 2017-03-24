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
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Interfaces;
using UserControls.ViewModels;
using UserControls.ViewModels.Invoices;
using UserControls.Views;

namespace ES.Shop.Views.Reports.ViewModels
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
            ViewSaleCommand = new RelayCommand<ViewInvoicesEnum>(OnViewSale);
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
            return InvoicesManager.GetSaleByPartners(dateIntermediate, ApplicationManager.Instance.GetEsMember.Id);
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
        private void AddTab(UserControl control, ITabItem vm)
        {
            var tab = _parentTabControl.FindChild<TabControl>();
            if (tab == null || vm == null) { return; }

            var nextTab = tab.Items.Add(new TabItem
            {
                Content = control,
                DataContext = vm,
                AllowDrop = true
            });
            tab.SelectedIndex = nextTab;
        }
        private bool CanViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            switch (o)
            {
                case ViewInvoicesEnum.None:
                case ViewInvoicesEnum.ByDetiles:
                    return true;
                    break;
                case ViewInvoicesEnum.ByStock:
                    return false;
                    break;

                default:
                    return false;
            }
        }
        private void OnViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            
            ITabItem viewModel = null;
            switch (o)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new InternalWayBillViewModel(o);
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    viewModel = new InternalWayBillDetileViewModel(o);
                    break;
                default:
                    break;
            }
            var tabControl = new UctrlViewTable{DataContext = viewModel};
            AddTab(tabControl, viewModel);
        }

        private void OnViewSale(ViewInvoicesEnum type)
        {
            ITabItem viewModel = null;
            switch (type)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new InvoiceReportViewModel(new List<InvoiceType> { InvoiceType.SaleInvoice });
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                case ViewInvoicesEnum.ByPartner:
                    viewModel = new SaleInvoiceReportByPartnerViewModel(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel};
            AddTab(tabControl, viewModel);
        }

        #endregion

        #region External methods

        #endregion

        #region Commands

        public ICommand ViewInternalWayBillCommands { get; private set; }
        public ICommand ViewSaleCommand { get; private set; }
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
