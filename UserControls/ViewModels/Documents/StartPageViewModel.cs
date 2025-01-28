using System;
using System.Windows.Input;
using AccountingTools.Enums;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using UserControls.Enumerations;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl.Helper;
using UserControls.Views.Analyze.View;

namespace UserControls.ViewModels.Documents
{
    public class StartPageViewModel : DocumentViewModel
    {
        #region Internal properties

        private readonly IShellViewModel _parent;
        #endregion Internal properties

        #region External properties

        #endregion External properties

        #region Constructors

        public StartPageViewModel(IShellViewModel parent)
        {
            _parent = parent;
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            Title = "Գլխավոր";
            CanFloat = false;
            IsClosable = false;

            OpenInvocieCommand = new RelayCommand<Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>>(OnOpenInvoice, CanOpenInvoice);
            GetReportCommand = new RelayCommand<ReportTypes>(OnGetReport, CanGetReport);
            OpenCarculatorCommand = new RelayCommand(_parent.OnOpenCalc);
            ToolsCommand = new RelayCommand<ToolsEnum>(_parent.OnTools, _parent.CanOpenTools);
            PriceTagCommand = new RelayCommand(() => { PriceTicketManager.PrintPriceTicket(PrintPriceTicketEnum.PriceTag); });
            ManageProductsProcessingCommand = new RelayCommand(OnManageProductsProcessing, CanManageProductProcessing);
        }       

        public bool CanOpenInvoice(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> tuple)
        {
            return _parent.CanGetInvoices(tuple);
        }
        private void OnOpenInvoice(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> o)
        {
            _parent.OnGetInvoices(o);
        }

        private bool CanGetReport(ReportTypes reportTypesEnum)
        {
            switch (reportTypesEnum)
            {
                case ReportTypes.ShortReport:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);

                case ReportTypes.Report:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager);

                default:
                    throw new ArgumentOutOfRangeException("reportTypesEnum", reportTypesEnum, null);
            }
        }

        private void OnGetReport(ReportTypes type)
        {
            _parent.OnGetReport(type);
        }
        private bool CanExecuteAccountingAction(AccountingPlanEnum accountingPlan)
        {
            return _parent.CanExecuteAccountingAction(accountingPlan);
        }

        private void OnAccountingAction(AccountingPlanEnum accountingPlan)
        {
            _parent.OnAccountingAction(accountingPlan);
        }

        private bool CanManageProductProcessing()
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Admin);
        }
        private void OnManageProductsProcessing()
        {
            new AnalyzeProducts().Show();
        }
        #endregion Internal methods

        #region External methods

        #endregion External methods

        #region Commands

        #region Invoices

        public ICommand OpenInvocieCommand { get; private set; }

        #endregion Invoices

        #region ChashDesk

        private ICommand _accountingActionCommand;

        public ICommand AccountingActionCommand
        {
            get { return _accountingActionCommand ?? (_accountingActionCommand = new RelayCommand<AccountingPlanEnum>(OnAccountingAction, CanExecuteAccountingAction)); }
        }

        #endregion CashDesk

        #region Reports

        public ICommand GetReportCommand { get; private set; }
        public ICommand OpenCarculatorCommand { get; private set; }

        #endregion Reports

        public ICommand ToolsCommand { get; private set; }
        public bool IsPopupOpened { get; set; }
        public ICommand PriceTagCommand { get; private set; }
        public ICommand ManageProductsCommand
        {
            get { return _parent.ManageProductsCommand; }
        }
        public ICommand ManageProductsProcessingCommand { get; private set; }

        #endregion Commands
    }
}
