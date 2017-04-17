using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ES.Business.Managers;
using ES.Business.Models;
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

        #region Filters
        Timer _timer = null;
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
        public List<AccountingRecordsModel> AccountingRecords { get { return _accountingRecords; } }

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
            Initialize(startDate ?? DateTime.Today, endDate ?? DateTime.Now);
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize(DateTime startDate, DateTime endDate)
        {
            Title = "Հվային պլանի վերծանում ";
            _accountingRecords = AccountingRecordsManager.GetAccountingRecords(startDate, endDate);
        }
        #endregion Internal methods

        #region External methods
        #endregion External methods

        #region Commands
        #endregion Commands
    }
    
}
