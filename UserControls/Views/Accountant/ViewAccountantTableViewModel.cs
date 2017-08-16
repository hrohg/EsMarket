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
            Title = string.Format("Հաշվային պլանի վերծանում {0} - {1}", StartDate, EndDate);
        }
        private void OnUpdateAccountingRecords(AccountingActionsEnum actionEnum)
        {
            List<Guid> guidIds;
            switch (actionEnum)
            {
                case AccountingActionsEnum.None:
                    new Thread(delegate()
                    {
                        _accountingRecords = AccountingRecordsManager.GetAccountingRecords(StartDate, EndDate);
                        OnUpdate();
                    }).Start();
                    break;
                case AccountingActionsEnum.PurchasePayables:
                case AccountingActionsEnum.ReceivedInAdvance:
                case AccountingActionsEnum.AccountingReceivable:
                case AccountingActionsEnum.Prepayments:
                    guidIds = SelectItemsManager.SelectPartners(true).Select(s => s.Id).ToList();
                    new Thread(delegate()
                    {
                        var list = AccountingRecordsManager.GetAccountingRecords(StartDate, EndDate, new List<int> { (int)actionEnum });
                        _accountingRecords = list.Where(s => (s.DebitGuidId != null && guidIds.Contains(s.DebitGuidId.Value)) ||
                                    (s.CreditGuidId != null && guidIds.Contains(s.CreditGuidId.Value))).ToList();
                        OnUpdate();
                    }).Start();
                    break;
                case AccountingActionsEnum.CashDesk:
                    var cashDesks = SelectItemsManager.SelectCashDesks(null, true).Select(s => s.Id).ToList();

                    new Thread(delegate(){
                        var list = AccountingRecordsManager.GetAccountingRecords(StartDate, EndDate, new List<int> { (int)actionEnum });
                        _accountingRecords = list.Where(s => (s.DebitGuidId != null && cashDesks.Contains(s.DebitGuidId.Value)) ||
                                    (s.CreditGuidId != null && cashDesks.Contains(s.CreditGuidId.Value))).ToList();
                                             OnUpdate();
                    }).Start();
                    break;
               
                case AccountingActionsEnum.Partner:
                    guidIds = SelectItemsManager.SelectPartners().Select(s => s.Id).ToList();
                    new Thread(delegate()
                    {
                        _accountingRecords = AccountingRecordsManager.GetAccountingRecordsByPartner(StartDate, EndDate, guidIds);
                        OnUpdate();
                    }).Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("actionEnum", actionEnum, null);
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

        public void UpdateAccountingRecords(AccountingActionsEnum actionEnum)
        {
            OnUpdateAccountingRecords(actionEnum);
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
