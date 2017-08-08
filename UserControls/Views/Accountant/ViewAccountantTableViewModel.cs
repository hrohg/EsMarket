using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;

namespace UserControls.Views.Accountant
{
    public class ViewAccountantTableViewModel : DocumentViewModel
    {
        #region Internal properties

        private List<AccountingPlanRecordsModel> _accountingPlan;
        private List<AccountingRecordsModel> _accountingRecords;
        #endregion Internal properties

        #region External properties

        #region Dates

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        #endregion Dates

        #region Filters
        Timer _timer;
        private string _filterText;
        public string Filter
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
        private List<string> Filters
        {
            get
            {
                return string.IsNullOrEmpty(_filterText) ? new List<string>() : _filterText.Split(',').Select(s => s.Trim()).ToList();
            }
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged("Items");
            DisposeTimer();
        }
        #endregion Filters


        public List<AccountingPlanRecordsModel> AccountingPlan
        {
            get
            {
                return _accountingPlan ?? (_accountingPlan = new List<AccountingPlanRecordsModel>());
            }
        }
        public List<AccountingRecordsModel> AccountingRecords { get { return _accountingRecords ?? new List<AccountingRecordsModel>(); } }

        public List<AccountingRecordsModel> Items
        {
            get
            {
                return AccountingRecords.Where(s => !Filters.Any() || Filters.Contains(s.Credit.ToString()) || Filters.Contains(s.Debit.ToString())).ToList();
            }
        }
        #endregion External properties

        #region Consructors

        public ViewAccountantTableViewModel(DateTime? startDate, DateTime? endDate)
        {
            StartDate = startDate ?? DateTime.Today;
            EndDate = endDate ?? DateTime.Today.AddDays(1);
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Հվային պլանի վերծանում ";

        }
        private void OnUpdateAccountingRecords(AccountingPlanEnum accountingPlanEnum)
        {
            switch (accountingPlanEnum)
            {
                case AccountingPlanEnum.None:
                    new Thread(delegate()
                    {
                        _accountingRecords = AccountingRecordsManager.GetAccountingRecords(StartDate, EndDate);
                        OnUpdate();
                    }).Start();
                    break;
                case AccountingPlanEnum.Purchase:
                    break;
                case AccountingPlanEnum.AccountingReceivable:
                    break;
                case AccountingPlanEnum.Prepayments:
                    break;
                case AccountingPlanEnum.CashDesk:
                    var cashDesks = SelectItemsManager.SelectCashDesks(null, true).Select(s => s.Id).ToList();

                    new Thread(delegate(){
                        var list = AccountingRecordsManager.GetAccountingRecords(StartDate, EndDate, new List<int> { (int)accountingPlanEnum });
                        _accountingRecords = list.Where(s => (s.DebitGuidId != null && cashDesks.Contains(s.DebitGuidId.Value)) ||
                                    (s.CreditGuidId != null && cashDesks.Contains(s.CreditGuidId.Value))).ToList();
                                             OnUpdate();
                    }).Start();
                    break;
                case AccountingPlanEnum.Accounts:
                    break;
                case AccountingPlanEnum.EquityBase:
                    break;
                case AccountingPlanEnum.PurchasePayables:
                    break;
                case AccountingPlanEnum.ReceivedInAdvance:
                    break;
                case AccountingPlanEnum.Debit_For_Salary:
                    break;
                case AccountingPlanEnum.Proceeds:
                    break;
                case AccountingPlanEnum.CostPrice:
                    break;
                case AccountingPlanEnum.CostOfSales:
                    break;
                case AccountingPlanEnum.OtherOperationalExpenses:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("accountingPlanEnum", accountingPlanEnum, null);
            }
            
        }

        private void OnUpdate()
        {
            if (Application.Current != null)
            {
                RaisePropertyChanged("Items");
            }
        }
        #endregion Internal methods

        #region External methods

        public void UpdateAccountingRecords(AccountingPlanEnum accountingPlanEnum)
        {
            OnUpdateAccountingRecords(accountingPlanEnum);
        }

        #endregion External methods

        #region Commands

        private ICommand _exportToExcelCommand;

        public ICommand ExportToExcelCommand
        {
            get { return _exportToExcelCommand ?? (_exportToExcelCommand = new RelayCommand(OnExportToExcel)); }
        }

        private void OnExportToExcel(object obj)
        {
            ExcelExportManager.ExportList(AccountingRecords);
        }

        #endregion Commands
    }
}
