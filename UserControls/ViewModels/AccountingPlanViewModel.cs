using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Managers;
using ES.DataAccess.Models;
using UserControls.Commands;

namespace UserControls.ViewModels
{
    public class AccountingPlanViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
        #region Properties

        private const string AccountingPlanProperty = "AccountingPlan";
        private const string EditButtonImagePathProperty = "EditButtonImagePath";
        private const string SubAccountingPlansProperty = "SubAccountingPlans";
        private const string SubAccountingPlanProperty = "SubAccountingPlan";
        #endregion
        #region Private properties
        private List<AccountingPlan> _accountingPlans;
        private List<SubAccountingPlanModel> _subAccountingPlans;
        private int? _accountingPlan;
        private SubAccountingPlanModel _subAccountingPlan;
        #endregion
        #region Public properties
        public ObservableCollection<AccountingAccounts> Credits
        {
            get
            {
                return
                    new ObservableCollection<AccountingAccounts>(
                        _accountingPlans.Select(s => s.CreditId).Distinct().Select(s =>
                            new AccountingAccounts
                            {
                                Id = s,
                                Description = string.Format("{0} {1}", s,
                                    AccountingRecordsManager.GetAccountingRecordsDescription(s))
                            }).ToList());
            }
        }
        public ObservableCollection<AccountingAccounts> Debits
        {
            get
            {
                return
                    new ObservableCollection<AccountingAccounts>(
                        _accountingPlans.Select(s => s.CreditId).Distinct().Select(s =>
                            new AccountingAccounts
                            {
                                Id = s,
                                Description = string.Format("{0} {1}", s,
                                    AccountingRecordsManager.GetAccountingRecordsDescription(s))
                            }).ToList());
            }
        }
        public ObservableCollection<AccountingAccounts> AccountingPlans
        {
            get
            {
                return
                    new ObservableCollection<AccountingAccounts>(
                        _accountingPlans.Select(s => s.CreditId).Concat(_accountingPlans.Select(s => s.DebitId)).Distinct().OrderBy(s => s).Select(s =>
                            new AccountingAccounts
                            {
                                Id = s,
                                Description = string.Format("{0} {1}", s,
                                    AccountingRecordsManager.GetAccountingRecordsDescription(s))
                            }).ToList());
            }
        }
        public int? AccountingPlan { get { return _accountingPlan; } set { _accountingPlan = value; 
            OnPropertyChanged(AccountingPlanProperty); OnPropertyChanged(SubAccountingPlansProperty); OnPropertyChanged(EditButtonImagePathProperty); } }

        public ObservableCollection<SubAccountingPlanModel> SubAccountingPlans
        {
            get
            {
                return
                    new ObservableCollection<SubAccountingPlanModel>(_subAccountingPlans.Where(s => s.AccountingPlanId == _accountingPlan).ToList());
            }
        }
        public SubAccountingPlanModel SubAccountingPlan { get { return _subAccountingPlan; } 
            set { _subAccountingPlan=value; OnPropertyChanged(SubAccountingPlanProperty);} }
        public string EditButtonImagePath { get { return SubAccountingPlan != null && SubAccountingPlans.SingleOrDefault(s => s.Id==SubAccountingPlan.Id)!=null ? "pack://application:,,,/Shared;component/Images/plus.ico" : "pack://application:,,,/Shared;component/Images/edit.ico"; } }

        public Guid SubAccountingPlanId
        {
            get
            {
                if(SubAccountingPlan==null) {SubAccountingPlan=new SubAccountingPlanModel();}
                return SubAccountingPlan.Id;
            }
            set
            {
                SubAccountingPlan = _subAccountingPlans.SingleOrDefault(s => s.Id == value);
                OnPropertyChanged(SubAccountingPlanProperty);
            }
        }

        #endregion

        public AccountingPlanViewModel()
        {
            _accountingPlans = AccountingRecordsManager.GetAccountingPlan();
            _subAccountingPlans = SubAccountingPlanManager.GetSubAccountingPlanModels(ApplicationManager.Instance.GetMember.Id);
            SubAccountingPlan = new SubAccountingPlanModel();
            SetCommands();
        }

        private void SetCommands()
        {
            EditSubAccountingPlanCommand=new EditSubAccountingCommands(this);
        }
        #region EditSubAccountingPlanCommand

        public bool CanEditSubAccounting()
        {
            return SubAccountingPlan != null && !string.IsNullOrEmpty(SubAccountingPlan.Name);
        }

        public void EditSubAccountingPlan()
        {
            if (!CanEditSubAccounting()) { return;}
            var exItem = SubAccountingPlans.SingleOrDefault(s => s.Id == SubAccountingPlan.Id);
            if (exItem != null)
            {
                exItem.Name = SubAccountingPlan.Name;
            }
            else
            {
                SubAccountingPlans.Add(SubAccountingPlan);
            }
        }
        #endregion
        #region Commands

        public ICommand EditSubAccountingPlanCommand { get; private set; }

        #endregion

    }
}
