using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.DataAccess.Models;
using UserControls.Commands;

namespace UserControls.ViewModels
{
    public class AccountingRecordsViewModel : INotifyPropertyChanged
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

        private const string DescriptionProperty = "Description";
        private const string DebitDescriptionProperty = "DebitDescription";
        private const string CreditDescriptionProperty = "CreditDescription";
        private const string AccountingRecordsProperty = "AccountingRecords";
        private const string DebitsProperty = "Debits";
        private const string CreditsProperty = "Credits";
        private const string DebitProperty = "Debit";
        private const string DebitIdProperty = "DebitId";
        private const string CreditProperty = "Credit";
        private const string CreditIdProperty = "CreditId";
        private const string SubDebitsProperty = "SubDebits";
        private const string SubCreditsProperty = "SubCredits";
        private const string SubDebitProperty = "SubDebit";
        private const string SubCreditProperty = "SubCredit";
        private const string CanRenewProperty = "CanRenew";
        #endregion
        #region Private properties

        private List<AccountingPlan> _accountingPlan;
        private string _description;
        private string _debitDescription;
        private string _creditDescription;
        private AccountingRecordsModel _accountingRecord;
        //private ObservableCollection<AccountingAccounts> _debits;
        //private AccountingAccounts _debit;
        //private ObservableCollection<AccountingAccounts> _credits;
        //private AccountingAccounts _credit;
        private short? _debitId;
        private short? _creditId;
        private ObservableCollection<SubAccountingPlanModel> _subDebits;
        private ObservableCollection<SubAccountingPlanModel> _subCredits;
        private SubAccountingPlanModel _subDebit;
        private SubAccountingPlanModel _subCredit;
        private ObservableCollection<AccountingPlanRecordsModel> _accountingRecords = new ObservableCollection<AccountingPlanRecordsModel>() { new AccountingPlanRecordsModel() };
        #endregion
        #region Public properties

        public AccountingRecordsModel AccountingRecord
        {
            get
            {
                return _accountingRecord;
            }
            set
            {
                _accountingRecord = value;
                OnPropertyChanged(AccountingRecordsProperty);
            }
        }
        public ObservableCollection<AccountingPlanRecordsModel> AccountingRecords
        {
            get { return _accountingRecords; }
            set
            {
                _accountingRecords = value;
            }
        }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string DebitDescription { get { return _debitDescription; } set { _debitDescription = value; OnPropertyChanged(DebitDescriptionProperty); } }
        public string CreditDescription { get { return _creditDescription; } set { _creditDescription = value; OnPropertyChanged(CreditDescriptionProperty); } }
        public ObservableCollection<AccountingAccounts> Debits
        {
            get
            {
                return CreditId == null ? new ObservableCollection<AccountingAccounts>(_accountingPlan.Select(s => s.DebitId).Distinct().Select(s =>
                new AccountingAccounts
                {
                    Id = s,
                    Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s))
                }).ToList()) :
                    new ObservableCollection<AccountingAccounts>(_accountingPlan.Where(s => s.CreditId == CreditId).Select(s => s.DebitId).Distinct().Select(s =>
                new AccountingAccounts
                {
                    Id = s,
                    Description = string.Format("{0} {1}", s, AccountingRecordsManager.GetAccountingRecordsDescription(s))
                }).ToList());
            }
        }

        public AccountingAccounts Debit
        {
            get
            {
                return DebitId == null ? null : new AccountingAccounts { Id = (short)DebitId, Description = string.Format("{0} {1}", DebitId, AccountingRecordsManager.GetAccountingRecordsDescription((int)DebitId)) };
            }
            set
            {
                _debitId = value != null ? value.Id : (short?)null;
            }
        }

        public short? DebitId
        {
            get { return _debitId; }
            set
            {
                _debitId = value;
                AccountingRecord.Debit = DebitId ?? 0;
                SubDebits = DebitId != null ? new ObservableCollection<SubAccountingPlanModel>(SubAccountingPlanManager.GetSubAccountingPlanModels((short)DebitId, 0, ApplicationManager.Instance.GetMember.Id, true)) : new ObservableCollection<SubAccountingPlanModel>();
                OnPropertyChanged(DebitIdProperty);
                if (CreditId == null) OnPropertyChanged(CreditsProperty);
                OnPropertyChanged("CanSelectDebit");
            }
        }
        public short? CreditId
        {
            get { return _creditId; }
            set
            {
                _creditId = value;
                AccountingRecord.Credit = CreditId ?? 0;
                SubCredits = CreditId != null ? new ObservableCollection<SubAccountingPlanModel>(SubAccountingPlanManager.GetSubAccountingPlanModels(0, (short)CreditId, ApplicationManager.Instance.GetMember.Id, true)) : new ObservableCollection<SubAccountingPlanModel>();
                OnPropertyChanged(CreditIdProperty);
                if (DebitId == null) OnPropertyChanged(DebitsProperty);
                OnPropertyChanged("CanSelectCredit");
            }
        }

        public ObservableCollection<SubAccountingPlanModel> SubDebits
        {
            get { return _subDebits; }
            set
            {
                _subDebits = value; OnPropertyChanged(SubDebitsProperty);
                SubDebit = SubDebits != null ? SubDebits.FirstOrDefault() : null;
            }
        }
        public ObservableCollection<AccountingAccounts> Credits
        {
            get
            {
                //return _credits;
                return DebitId == null ? new ObservableCollection<AccountingAccounts>(_accountingPlan.Select(s => s.CreditId).Distinct().Select(s =>
                new AccountingAccounts
                {
                    Id = s,
                    Description = string.Format("{0} {1}", s,
                        AccountingRecordsManager.GetAccountingRecordsDescription(s))
                }).ToList()) :
                    new ObservableCollection<AccountingAccounts>(_accountingPlan.Where(s => s.DebitId == DebitId).Select(s => s.CreditId).Distinct().Select(s =>
                new AccountingAccounts
                {
                    Id = s,
                    Description = string.Format("{0} {1}", s,
                        AccountingRecordsManager.GetAccountingRecordsDescription(s))
                }).ToList());
            }
        }

        public AccountingAccounts Credit
        {
            get
            {
                return CreditId == null ? null : new AccountingAccounts { Id = (short)CreditId, Description = string.Format("{0} {1}", CreditId, AccountingRecordsManager.GetAccountingRecordsDescription((int)CreditId)) };
            }
            set
            {
                _creditId = value != null ? value.Id : (short?)null;
            }
        }

        public ObservableCollection<SubAccountingPlanModel> SubCredits
        {
            get
            {
                return _subCredits;
            }
            set
            {
                _subCredits = value;
                SubCredit = SubCredits != null ? SubCredits.FirstOrDefault() : null;
                OnPropertyChanged(SubCreditsProperty);

            }
        }

        public SubAccountingPlanModel SubDebit
        {
            get { return _subDebit; }
            set
            {
                if (value == _subDebit) { return;}
                _subDebit = value;
                AccountingRecord.DebitGuidId = SubDebit != null ? SubDebit.Id : (Guid?)null;
                OnPropertyChanged(SubDebitProperty);
            }
        }
        public SubAccountingPlanModel SubCredit
        {
            get { return _subCredit; }
            set
            {
                if(value==_subCredit){return;}
                _subCredit = value;
                AccountingRecord.CreditGuidId = SubCredit != null ? SubCredit.Id : (Guid?)null;
                OnPropertyChanged(SubCreditProperty);
            }
        }
        public bool CanSelectDebit { get { return DebitId == null; } }
        public bool CanSelectCredit { get { return CreditId == null; } }
        #endregion
        public AccountingRecordsViewModel(AccountingRecordsModel accountingRecord, string description)
        {
            _accountingPlan = AccountingRecordsManager.GetAccountingPlan();
            OnPropertyChanged(DebitsProperty);
            OnPropertyChanged(CreditsProperty);
            Description = description;
            if (accountingRecord != null)
            {
                AccountingRecord = accountingRecord;
                DebitId = accountingRecord.Debit;
                CreditId = accountingRecord.Credit;
            }
            else { AccountingRecord = new AccountingRecordsModel(); }
            
            DebitDescription = AccountingRecord.Debit + AccountingRecordsManager.GetAccountingRecordsDescription(AccountingRecord.Debit);
            CreditDescription = AccountingRecord.Credit + AccountingRecordsManager.GetAccountingRecordsDescription(AccountingRecord.Credit);
            SetCommands();
        }
        public AccountingRecordsViewModel()
        {
            _accountingPlan = AccountingRecordsManager.GetAccountingPlan();
            AccountingRecord = new AccountingRecordsModel();
            OnPropertyChanged(DebitsProperty);
            OnPropertyChanged(CreditsProperty);
            SetCommands();
        }
        public AccountingRecordsViewModel(int? debit, int? credit)
        {
            _accountingPlan = AccountingRecordsManager.GetAccountingPlan();
            AccountingRecord = new AccountingRecordsModel();
            OnPropertyChanged(DebitsProperty);
            OnPropertyChanged(CreditsProperty);
            DebitId = Debits.Where(s => s.Id == debit).Select(s => s.Id).FirstOrDefault();
            CreditId = Credits.Where(s => s.Id == credit).Select(s => s.Id).FirstOrDefault();
            SetCommands();
        }
        #region Privatemethods
        private void SetCommands()
        {
            AccountingRecordsCommand = new AccountingRecordsCommands(this);
            RecordRenewCommand = new RecordRenewCommands(this);
        }
        #endregion
        #region Public methods

        public bool CanExecute()
        {
            return AccountingRecord.Amount > 0 && AccountingRecord.Debit > 0 && AccountingRecord.Credit > 0;
        }
        public void Execute()
        {

        }

        public bool CanRenewRecord()
        {
            return CreditId != null || DebitId != null;
        }

        public void RecordRenew()
        {
            DebitId = CreditId = null;
            OnPropertyChanged(DebitsProperty);
            OnPropertyChanged(CreditsProperty);
        }

        #endregion
        #region ICommands
        public ICommand AccountingRecordsCommand { get; private set; }
        public ICommand RecordRenewCommand { get; private set; }
        #endregion
    }
}
