using ES.Common.ViewModels.Base;
using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace ES.Data.Models
{
    public class InvoicePaid : NotifyPropertyChanged
    {
        #region Properties
        private const string PaidProperty = "Paid";
        private const string ByCheckProperty = "ByCheck";
        private const string ReceivedPrepaymentProperty = "ReceivedPrepayment";
        private const string AccountsReceivableProperty = "AccountsReceivable";
        #endregion

        #region Private properties
        private decimal? _total;
        private decimal? _paid;
        private decimal? _byCheck;
        private decimal? _receivedPrepayment;
        private decimal? _accountsReceivable;
        private decimal? _prepaiment;
        private Guid? _cashDeskId;
        private Guid? _cashDeskForTicketId;
        private Guid? _partnerId;

        #endregion

        #region Public properties
        /// <summary>
        /// Total needed yo pay.
        /// </summary>
        public decimal? Total { get { return _total; } set { _total = value; RaisePropertyChanged(() => Change); } }

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
                RaisePropertyChanged(() => Paid);
                RaisePropertyChanged(() => Change);
            }
        }
        public decimal ByCash { get { return (Paid ?? 0) - (Change ?? 0); } }
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
                RaisePropertyChanged(() => Change);
            }
        }
        /// <summary>
        /// Կանխավճար, պատվիրատուից ստացված կանխավճար
        /// </summary>
        public decimal? ReceivedPrepayment
        {
            get { return _receivedPrepayment; }
            set
            {
                if (value < 0) { value = null; }
                if (_receivedPrepayment == value) { return; }
                _receivedPrepayment = value;
                RaisePropertyChanged(() => ReceivedPrepayment);
                RaisePropertyChanged(() => Change);
            }
        }
        /// <summary>
        /// Դեբիտորական պարտք, մատակարարին տրված կանխավճար
        /// </summary>
        public decimal? AccountsReceivable
        {
            get { return _accountsReceivable; }
            set
            {
                if (value < 0) value = null;
                if (_accountsReceivable == value) { return; }
                _accountsReceivable = value;
                RaisePropertyChanged(() => AccountsReceivable);
                RaisePropertyChanged(() => Change);
            }
        }

        /// <summary>
        /// Վերադարձվող մանր
        /// </summary>
        public decimal? Change
        {
            get
            {
                if (!Paid.HasValue) return null;
                var paid = Paid ?? 0;
                var paidCashless = (ByCheck ?? 0) + (ReceivedPrepayment ?? 0) + (AccountsReceivable ?? 0);
                var total = Total ?? 0;
                var prepayment = Prepayment ?? 0;
                bool isPaidValid = paidCashless - prepayment <= total && (prepayment == 0 || ReceivedPrepayment == 0) && (paid + paidCashless >= prepayment + total);
                return isPaidValid ? paid + paidCashless - prepayment - total : 0;
            }
        }

        /// <summary>
        /// Կանխավճար, մանրը թողնել որպես կանխավճար
        /// </summary>
        public decimal? Prepayment
        {
            get { return _prepaiment; }
            set
            {
                if (value < 0)
                {
                    value = null;
                }
                _prepaiment = value;
                RaisePropertyChanged(() => Prepayment);
                RaisePropertyChanged(() => Change);
            }
        }

        //public decimal? DiscountBond
        //{
        //    get { return _discountBond; }
        //    set { _discountBond = value; OnPropertyChanged("DiscountBond"); }
        //}

        //public bool UseDiscountBond { get { return DiscountBond > 0; } }

        #endregion

        #region Public methods
        public bool IsPaid
        {
            get
            {
                return (Paid ?? 0) + (ByCheck ?? 0) + (ReceivedPrepayment ?? 0) + (AccountsReceivable ?? 0) - (Change ?? 0) - (Prepayment ?? 0) == (Total ?? 0);
            }
        }

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
