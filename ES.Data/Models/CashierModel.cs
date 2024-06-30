using System;
using System.ComponentModel;

namespace ES.Data.Models
{
    public class InvoicePaid : INotifyPropertyChanged
    {
        #region Properties
        private const string PaidProperty = "Paid";
        private const string ByCheckProperty = "ByCheck";
        private const string ReceivedPrepaymentProperty = "ReceivedPrepayment";
        private const string AccountsReceivableProperty = "AccountsReceivable";
        private const string ChangeProperty = "Change";
        private const string PrepaymentProperty = "Prepayment";
        #endregion

        #region Private properties
        private decimal? _total;
        private decimal? _paid;
        private decimal? _byCheck;
        private decimal? _receivedPrepayment;
        private decimal? _accountsReceivable;
        private decimal _prepaiment;
        private Guid? _cashDeskId;
        private Guid? _cashDeskForTicketId;
        private Guid? _partnerId;

        #endregion

        #region Public properties
        /// <summary>
        /// Total needed yo pay.
        /// </summary>
        public decimal? Total { get { return _total; } set { _total = value; OnPropertyChanged(ChangeProperty); } }

        public decimal? Paid
        {
            get
            {
                return _paid;
            }
            set
            {
                if (value < 0) value = null;
                if (_paid == value) { return; }
                _paid = value;
                OnPropertyChanged(PaidProperty);
                OnPropertyChanged(ChangeProperty);
            }
        }
        public decimal ByCash { get { return (Paid ?? 0) - (Change ?? 0) - (ReceivedPrepayment ?? 0); } }
        public decimal? ByCheck
        {
            get
            {
                return _byCheck;
            }
            set
            {
                if (value < 0)
                {
                    value = null;
                }
                if (_byCheck == value) { return; }
                _byCheck = value;
                OnPropertyChanged(ByCheckProperty);
                OnPropertyChanged(ChangeProperty);
            }
        }
        /// <summary>
        /// Կանխավճար
        /// </summary>
        public decimal? ReceivedPrepayment
        {
            get { return _receivedPrepayment; }
            set
            {
                if (value < 0) { value = null; }
                if (_receivedPrepayment == value) { return; }
                _receivedPrepayment = value; OnPropertyChanged(ReceivedPrepaymentProperty); OnPropertyChanged(ChangeProperty);
            }
        }
        /// <summary>
        /// Դեբետ
        /// </summary>
        public decimal? AccountsReceivable
        {
            get { return _accountsReceivable; }
            set
            {
                if (value < 0) value = null;
                if (_accountsReceivable == value) { return; }
                _accountsReceivable = value; OnPropertyChanged(AccountsReceivableProperty); OnPropertyChanged(ChangeProperty);
            }
        }
        public decimal? Change { get { return IsPaid ? ((Paid ?? 0) + (ByCheck ?? 0) + (ReceivedPrepayment ?? 0) + (AccountsReceivable ?? 0) - ReceivedPrepayment - (Total ?? 0)) : 0; } }
        //public decimal Prepayment { get { return _prepaiment; } set { _prepaiment = value; OnPropertyChanged(PrepaymentProperty); OnPropertyChanged(ChangeProperty); } }

        //public decimal? DiscountBond
        //{
        //    get { return _discountBond; }
        //    set { _discountBond = value; OnPropertyChanged("DiscountBond"); }
        //}

        //public bool UseDiscountBond { get { return DiscountBond > 0; } }

        #endregion

        #region Public methods
        public bool IsPaid { get { return ((Paid ?? 0) + (ByCheck ?? 0) + (ReceivedPrepayment ?? 0) + (AccountsReceivable ?? 0) - (ReceivedPrepayment ?? 0) - (Total ?? 0)) >= 0; } }
        public Guid? CashDeskId { get { return _cashDeskId; } set { _cashDeskId = value; } }
        public Guid? CashDeskForTicketId { get { return _cashDeskForTicketId; } set { _cashDeskForTicketId = value; } }
        public Guid? PartnerId { get { return _partnerId; } set { _partnerId = value; } }
        #endregion

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
    }
    public class AccountsReceivableModel : INotifyPropertyChanged
    {
        #region Properties
        private const string DateProperty = "Date";
        private const string ExpiryDateProperty = "ExpiryDate";
        private const string TotalProperty = "Total";
        private const string NotesProperty = "Notes";
        #endregion
        #region Private properties
        private Guid _id = Guid.NewGuid();
        private Guid _partnerId;
        private Guid _invoiceId;
        private long _cashierId;
        private long _memberid;
        private DateTime _date;
        private DateTime? _expiryDate;
        private decimal? _total;
        private string _notes;
        private bool? _isDebit;
        #endregion
        #region Public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public Guid PartnerId { get { return _partnerId; } set { _partnerId = value; } }
        public Guid InvoiceId { get { return _invoiceId; } set { _invoiceId = value; } }
        public long CashierId { get { return _cashierId; } set { _cashierId = value; } }
        public long MemberId { get { return _memberid; } set { _memberid = value; } }
        public DateTime Date { get { return _date; } set { _date = value; OnPropertyChanged(DateProperty); } }
        public DateTime? ExpiryDate { get { return _expiryDate; } set { _expiryDate = value; OnPropertyChanged(ExpiryDateProperty); } }
        public decimal? Total { get { return _total; } set { _total = value > 0 ? value : null; OnPropertyChanged(TotalProperty); } }
        public string Notes { get { return _notes; } set { _notes = value; OnPropertyChanged(NotesProperty); } }
        public bool? IsDebit { get { return _isDebit; } set { _isDebit = value; } }
        #endregion
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
        public AccountsReceivableModel(Guid invoiceId, Guid partnerId, long cashierId, long memberId, bool? isDebit)
        {
            InvoiceId = invoiceId;
            PartnerId = partnerId;
            CashierId = cashierId;
            MemberId = memberId;
            IsDebit = isDebit;
        }
    }


}
