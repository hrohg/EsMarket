using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ES.Business.Managers;

namespace ES.Business.Models
{
    public class AccountingPlanRecordsModel : INotifyPropertyChanged, IEditableObject
    {
        private AccountingPlanRecordsModel newItem;
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
        #region IEditableObject Members
        public void BeginEdit()
        {
            if (this.newItem == null)
                this.newItem = new AccountingPlanRecordsModel();
        }

        public void CancelEdit()
        {
            
        }

        public void EndEdit()
        {
            
        }
        #endregion IEditableObject Members
        #region Properties

        private const string DebitsProperty = "Debits";
        private const string CreditsProperty = "Credits";
        #endregion
        #region Private properties

        private ObservableCollection<AccountingAccounts> debits;
        private AccountingAccounts debit;
        private ObservableCollection<AccountingAccounts> credits;
        private AccountingAccounts credit;
        #endregion
        #region Public properties

        public ObservableCollection<AccountingAccounts> Debits { get { return debits; } set { debits = value; } }
        public ObservableCollection<SubAccountingPlanModel> SubDebits;
        public AccountingAccounts Debit
        {
            get { return debit; }
            set
            {
                debit = value; SubDebits = 
                    new ObservableCollection<SubAccountingPlanModel>(SubAccountingPlanManager.GetSubAccountingPlanModels(Debit.Id, ApplicationManager.GetEsMember.Id, true));
            }
        }
        public ObservableCollection<AccountingAccounts> Credits { get { return credits; } set { credits = value; OnPropertyChanged(DebitsProperty); } }
        public ObservableCollection<SubAccountingPlanModel> SubCredits;
        public AccountingAccounts Credit
        {
            get { return credit; }
            set
            {
                credit = value; OnPropertyChanged(CreditsProperty);
                SubCredits =
                    new ObservableCollection<SubAccountingPlanModel>(
                        SubAccountingPlanManager.GetSubAccountingPlanModels(Credit.Id, ApplicationManager.GetEsMember.Id, true));
            }
        }
        #endregion

        public AccountingPlanRecordsModel()
        {
            Debits = new ObservableCollection<AccountingAccounts>(AccountingRecordsManager.GetDebits().Select(s =>
               new AccountingAccounts { Id = s, Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s)) }).ToList());
            Credits = new ObservableCollection<AccountingAccounts>(AccountingRecordsManager.GetDebits().Select(s =>
                new AccountingAccounts { Id = s, Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s)) }).ToList());
        }
        public AccountingPlanRecordsModel(int? debit, Guid? subDebit, int? credit, Guid? subcredit)
        {
            Debits = new ObservableCollection<AccountingAccounts>(AccountingRecordsManager.GetDebits().Select(s =>
                new AccountingAccounts { Id = s, Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s)) }).ToList());
            Credits = new ObservableCollection<AccountingAccounts>(AccountingRecordsManager.GetDebits().Select(s =>
                new AccountingAccounts { Id = s, Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s)) }).ToList());
        }
    }
}
