using System;
using System.ComponentModel;
using System.Xml.Serialization;
using ES.Data.Model;
using EsMarket.SharedData.Interfaces;

namespace ES.Data.Models
{
    public class InvoiceModel :INotifyPropertyChanged
    {
        public InvoiceModel()
        {
            CreateDate = DateTime.Now;
        }
        public InvoiceModel(EsUserModel creator, EsMemberModel member)
        {
            _creatorId = creator.UserId;
            _creator = creator.FullName;
            Creator = creator.FullName;
            _memberId = member.Id;
            CreateDate = DateTime.Now;
        }
        public InvoiceModel(EsUserModel creator, EsMemberModel member, long invoiceTypeId)
            : this(creator, member)
        {
            _invoiceTypeId = invoiceTypeId;
        }


        #region Invoice model properties
        private const string CreateDateProperty = "CreateDate";
        private const string DiscountProperty = "Discount";
        private const string TotalProperty = "Total";
        private const string AmountProperty = "Amount";
        private const string PaidProperty = "Paid";
        private const string OddMoneyProperty = "OddMoney";
        private const string ApproveDateProperty = "ApproveDate";
        #endregion

        #region ImvoiceModel private properties
        private Guid _id = Guid.NewGuid();
        private long _invoiceTypeId;
        private long _creatorId;
        private string _creator;
        private DateTime _createDate = DateTime.Now;
        private long _memberId;
        private Guid? _partnerId;
        private string _providerName;
        private string _recipientName;
        private decimal? _discount;
        private decimal _summ;
        private decimal _amount;
        private decimal? _paid;
        private DateTime? _approveDate;

        #endregion

        #region Invoice model public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public long CreatorId { get { return _creatorId; } set { _creatorId = value; } }
        public string Creator { get { return _creator; } set { _creator = value; } }
        public long InvoiceTypeId { get { return _invoiceTypeId; } set { _invoiceTypeId = value; } }

        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set { _invoiceNumber = value; OnPropertyChanged("InvoiceNumber"); }
        }

        public string About
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4}", InvoiceNumber, CreateDate, ProviderName,
                    ApproveDate != null ? "=>" : "", RecipientName);
            }
        }
        public DateTime CreateDate { get { return _createDate; } set { _createDate = value; OnPropertyChanged(CreateDateProperty); } }
        public string RecipientName { get { return _recipientName; } set { _recipientName = value; } }
        public string ProviderName { get { return _providerName; } set { _providerName = value; } }
        public Guid? PartnerId { get { return _partnerId; } set { _partnerId = value; } }
        [XmlIgnore]
        public PartnerModel Partner { get; set; }
        public decimal? Discount { get { return _discount; } set { _discount = value; } }

        #region Cost price
        private decimal _costPrice;
        public decimal CostPrice
        {
            get { return _costPrice; }
            set
            {
                if (value == _costPrice) return;
                _costPrice = value;
                OnPropertyChanged("CostPrice");
            }
        }
        #endregion Cost price

        public decimal Total
        {
            get { return _summ; }
            set
            {
                _summ = value;
                _discount = Amount != 0 ? (Amount - Total) * 100 / Amount : 0; OnPropertyChanged(TotalProperty); OnPropertyChanged(DiscountProperty);
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                _discount = Amount != 0 ? (Amount - Total) * 100 / Amount : 0; OnPropertyChanged(DiscountProperty);
                OnPropertyChanged(AmountProperty); OnPropertyChanged(OddMoneyProperty);
            }
        }
        public decimal? Paid { get { return _paid; } set { _paid = value; OnPropertyChanged(PaidProperty); OnPropertyChanged(OddMoneyProperty); } }
        public decimal OddMoney { get { return (Paid != null ? (decimal)Paid : 0) - Amount; } }
        public long MemberId { get { return _memberId; } set { _memberId = value; } }
        public long? FromStockId { get; set; }
        public long? ToStockId { get; set; }
        public DateTime? ApproveDate { get { return _approveDate; } set { _approveDate = value; OnPropertyChanged(ApproveDateProperty); OnPropertyChanged("IsApproved"); } }
        public long? ApproverId { get; set; }
        public string Approver { get; set; }
        public DateTime? AcceptDate { get; set; }
        public long? AccepterId { get; set; }
        public string Accepter { get; set; }
        public string ProviderJuridicalAddress { get; set; }
        public string ProviderAddress { get; set; }
        public string ProviderBank { get; set; }
        public string ProviderBankAccount { get; set; }
        public string ProviderTaxRegistration { get; set; }
        public string RecipientJuridicalAddress { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientBank { get; set; }
        public string RecipientBankAccount { get; set; }
        public string RecipientTaxRegistration { get; set; }
        private string _note;
        private string _invoiceNumber;

        public string Notes
        {
            get { return _note; }
            set { _note = value; OnPropertyChanged("Notes"); }
        }

        public bool IsApproved
        {
            get { return ApproveDate != null; }
        }
        public void Reload(InvoiceModel invoice, long memberId)
        {
            Id = invoice.Id;
            InvoiceTypeId = invoice.InvoiceTypeId;
            InvoiceNumber = invoice.InvoiceNumber;
            CreateDate = invoice.CreateDate;
            Creator = invoice.Creator;
            CreatorId = invoice.CreatorId;
            MemberId = memberId;
            Notes = invoice.Notes;
        }
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
}
