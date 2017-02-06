using System;
using System.ComponentModel;
using ES.Business.Managers;

namespace ES.Business.Models
{
    public class AccountingRecordsModel : INotifyPropertyChanged
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

        private readonly string AmountProperty = "Amount";
        private readonly string DescriptionProperty = "Description";
        #endregion
        #region Private properties
        private Guid _id = Guid.NewGuid();
        private DateTime _registerDate = DateTime.Now;
        private string _description;
        private decimal _amount;
        private long _debit;
        private long _credit;
        private long _memberId;
        private long _registerId;
        private Guid? _debitGuidId;
        private Guid? _creditGuidId;
        private long? _debitLongId;
        private long? _creditLongId;
        #endregion
        #region public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public DateTime RegisterDate { get { return _registerDate; } set { _registerDate = value; }}
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public decimal Amount { get { return _amount; } 
            set { _amount=value; OnPropertyChanged(AmountProperty);} }
        public long Debit { get { return _debit; } 
            set { _debit = value; } }
        public long Credit { get { return _credit; } set { _credit = value; } }
        public long MemberId { get { return _memberId; } set { _memberId = value; } }
        public long RegisterId { get { return _registerId; } set { _registerId = value; } }
        public Guid? DebitGuidId { get { return _debitGuidId; } set { _debitGuidId = value; } }
        public Guid? CreditGuidId { get { return _creditGuidId; } set { _creditGuidId = value; } }
        public long? DebitLongId { get { return _debitLongId; } set { _debitLongId = value; } }
        public long? CreditLongId { get { return _creditLongId; } set { _creditLongId = value; } }
        #endregion
        public AccountingRecordsModel(DateTime date, long memberId, long registerId)
        {
            RegisterDate = date;
            MemberId = memberId;
            RegisterId = registerId;
        }
        public AccountingRecordsModel()
        {
            RegisterDate = DateTime.Now;
            MemberId = ApplicationManager.GetEsMember.Id;
            RegisterId = ApplicationManager.GetEsUser.UserId;
        }
    }
}
