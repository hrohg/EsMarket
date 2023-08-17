using ES.Common.ViewModels.Base;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ES.Data.Models
{
    public class InvoiceModel : NotifyPropertyChanged
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
        public InvoiceModel(EsUserModel creator, EsMemberModel member, short invoiceTypeId)
            : this(creator, member)
        {
            _invoiceTypeId = invoiceTypeId;
        }


        #region Invoice model fields

        #endregion Invoice model fields

        #region ImvoiceModel private properties
        private Guid _id = Guid.NewGuid();
        private short _invoiceTypeId;
        private int _creatorId;
        private string _creator;
        private DateTime _createDate = DateTime.Now;
        private int _memberId;
        private Guid? _partnerId;
        private string _providerName;
        private string _recipientName;
        private decimal? _discount;
        private decimal _total;
        private decimal _amount;
        private decimal? _paid;
        private DateTime? _approveDate;
        private bool _useDiscountBond;
        #endregion

        #region Invoice model public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public int CreatorId { get { return _creatorId; } set { _creatorId = value; } }
        public string Creator { get { return _creator; } set { _creator = value; } }
        public short InvoiceTypeId { get { return _invoiceTypeId; } set { _invoiceTypeId = value; } }

        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set { _invoiceNumber = value; RaisePropertyChanged(() => InvoiceNumber); }
        }

        public string About
        {
            get
            {
                return string.Format("{0} {1} {2} {3} {4}", InvoiceNumber, CreateDate, ProviderName,
                    ApproveDate != null ? "=>" : "", RecipientName);
            }
        }
        public DateTime CreateDate { get { return _createDate; } set { _createDate = value; RaisePropertyChanged(() => CreateDate); } }
        public string RecipientName { get { return _recipientName; } set { _recipientName = value; } }
        public string ProviderName { get { return _providerName; } set { _providerName = value; } }
        public Guid? PartnerId { get { return _partnerId; } set { _partnerId = value; } }
        [XmlIgnore]
        public PartnerModel Partner { get; set; }
        public decimal? Discount
        {
            get { return _discount; }
            set
            {
                _discount = value;
                RaisePropertyChanged(() => Discount);
            }
        }
        
        #region Cost price
        private decimal _costPrice;
        public decimal CostPrice
        {
            get { return _costPrice; }
            set
            {
                if (value == _costPrice) return;
                _costPrice = value;
                RaisePropertyChanged(() => CostPrice);
            }
        }
        #endregion Cost price

        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                RaisePropertyChanged(() => Total);
                RaisePropertyChanged(() => TotalDiscount);
                RaisePropertyChanged(() => DiscountBond);
                RaisePropertyChanged(() => DiscountAmount);
            }
        }
        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                RaisePropertyChanged(() => Amount);
                RaisePropertyChanged(() => TotalDiscount);
                RaisePropertyChanged(() => DiscountBond);
                RaisePropertyChanged(() => DiscountAmount);
            }
        }
        public decimal DiscountAmount { get { return !UseDiscountBond ? Amount - Total: 0; } }
        public decimal? TotalDiscount { get { return Amount > 0 ? (Amount - Total) * 100 / Amount : 0; } }
        [XmlIgnore]
        public decimal? DiscountBond { get { return UseDiscountBond ? Amount > Total ? Amount - Total : 0 : 0; } }
        //public decimal? Paid
        //{
        //    get { return _paid; }
        //    set
        //    {
        //        _paid = value;
        //        OnPropertyChanged(PaidProperty);
        //        OnPropertyChanged(OddMoneyProperty);
        //    }
        //}

        //public decimal OddMoney { get { return (Paid != null ? (decimal)Paid : 0) - Amount; } }

        public int MemberId { get { return _memberId; } set { _memberId = value; } }
        public short? FromStockId { get; set; }
        public short? ToStockId { get; set; }
        public DateTime? ApproveDate { get { return _approveDate; } set { _approveDate = value; RaisePropertyChanged(() => ApproveDate); RaisePropertyChanged(() => IsApproved); } }
        public int? ApproverId { get; set; }
        public string Approver { get; set; }
        public DateTime? AcceptDate { get; set; }
        public int? AccepterId { get; set; }
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
            set { _note = value; RaisePropertyChanged(() => Notes); }
        }

        public bool IsApproved
        {
            get { return ApproveDate != null; }
        }
        public bool UseDiscountBond
        {
            get { return _useDiscountBond; }
            set
            {
                _useDiscountBond = value;
                RaisePropertyChanged(() => DiscountBond);
                RaisePropertyChanged(() => TotalDiscount);
                RaisePropertyChanged(() => DiscountAmount);
            }
        }

        public long InvoiceIndex { get; set; }

        public void Reload(InvoiceModel invoice, int memberId)
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
        //public void SetDiscountBond(decimal? value)
        //{
        //    DiscountBond = value > 0 ? value : 0;
        //    OnPropertyChanged("DiscountBond");
        //    OnPropertyChanged("TotalDiscount");
        //}
        #endregion
    }
}
