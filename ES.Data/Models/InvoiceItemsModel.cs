using ES.Common.ViewModels.Base;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ES.Data.Models
{
    public class InvoiceItemsModel : NotifyPropertyChanged
    {
        public delegate void InvoiceItemChangedEvent(InvoiceItemsModel invoiceItem);
        [XmlIgnore]
        public InvoiceItemChangedEvent InvoiceItemChanged;
        public InvoiceItemsModel(InvoiceModel invoice)
        {
            Invoice = invoice;
        }
        public InvoiceItemsModel()
        {
        }
        public InvoiceItemsModel(InvoiceModel invoice, Products.ProductModel product)
            : this(invoice)
        {
            if (product == null) return;
            Product = product;
        }

        #region Invoice items model private properties
        private Guid _id = Guid.NewGuid();
        private short _displayOrder;
        private Guid _productId;
        private Products.ProductModel _product;
        private Guid? _productItemId;
        private string _code;
        private string _description;
        private string _note;
        private decimal? _quantity;
        private decimal? _price;
        private decimal? _costPrice;
        private DateTime? _expiryDate;
        private decimal? _discount;
        #endregion

        #region Invoice items public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public short DisplayOrder
        {
            get { return _displayOrder; }
            set
            {
                if (_displayOrder == value) { return; }
                _displayOrder = value;
                RaisePropertyChanged(() => DisplayOrder);
            }
        }

        #region Invoice
        private Guid _invoiceId;
        public Guid InvoiceId
        {
            get { return _invoiceId; }
            set
            {
                if (value == _invoiceId) return;
                _invoiceId = value;
                OnPropertyChanged("InvoiceId");
            }
        }
        private InvoiceModel _invoice;
        [XmlIgnore]
        public InvoiceModel Invoice
        {
            get { return _invoice; }
            set
            {
                _invoice = value;
                InvoiceId = _invoice.Id;
                OnPropertyChanged("Invoice");
            }
        }
        #endregion Invoice
        [XmlIgnore]
        public Products.ProductModel Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                if (_product == null) return;

                ProductId = _product.Id;
                ExpiryDate = _product.ExpiryDays != null ? DateTime.Today.AddDays((int)_product.ExpiryDays) : (DateTime?)null;
                Code = _product.Code;
                Description = _product.Description;
                Note = _product.Note;

                RaisePropertyChanged(() => Product);
                RaisePropertyChanged(() => Mu);
                RaisePropertyChanged(() => ProductPrice);
            }
        }
        //[XmlIgnore]
        //public ProductItemModel ProductItem
        //{
        //    get
        //    {
        //        return _productItem;
        //    }
        //    set
        //    {
        //        _productItem = value;
        //        if (value == null) return;
        //        _productItemId = _productItem.Product != null ? _productItem.Id : (Guid?)null;
        //        _productId = _productItem != null ? _productItem.ProductId : Guid.Empty;
        //        _code = _productItem.Product != null ? _productItem.Product.Code : null;
        //        _description = _productItem.Product != null ? _productItem.Product.Description : null;
        //        _mu = _productItem.Product != null ? _productItem.Product.Mu : null;
        //        _note = _productItem.Product != null ? _productItem.Product.Note : null;
        //    }
        //}
        public Guid ProductId { get { return _productId; } set { _productId = value; } }
        public Guid? ProductItemId { get { return _productItemId; } set { _productItemId = value; } }
        public string Code { get { return _code; } set { _code = value; RaisePropertyChanged(() => Code); } }
        public string Description { get { return _description; } set { _description = value; RaisePropertyChanged(() => Description); } }
        public string Mu { get { return Product != null ? Product.Mu : null; } }
        public short? StockId { get; set; }
        public decimal? Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                RaisePropertyChanged(() => Quantity);
                RaisePropertyChanged(() => Amount);
                RaisePropertyChanged(() => CostAmount);
                OnInvoiceItemChanged();
            }
        }

        public decimal? ProductPrice { get { return Product != null ? Product.Price : null; } }
        public decimal? CostPrice
        {
            get { return _costPrice; }
            set
            {
                _costPrice = value;
                RaisePropertyChanged(() => CostPrice);
                RaisePropertyChanged(() => Percentage);
                RaisePropertyChanged(() => CostAmount);
                OnInvoiceItemChanged();
            }
        }
        public decimal? Price
        {
            get { return _price; }
            set
            {
                _price = value;
                _discount = null;
                RaisePropertyChanged(() => Price);
                RaisePropertyChanged(() => Discount);
                RaisePropertyChanged(() => Amount);
                RaisePropertyChanged(() => Percentage);
                OnInvoiceItemChanged();
            }
        }
        public decimal CostAmount { get { return (CostPrice ?? 0) * (Quantity ?? 0); } }
        public bool HasDiscount { get { return _discount.HasValue; } }
        public decimal? Discount
        {
            get { return _discount ?? decimal.Round((decimal)(Price > 0 ? (100 * ProductPrice / Price - 100) : 0), 2); }
            set
            {
                _discount = value;
                if (_discount != null && ProductPrice != null) _price = decimal.Round((decimal)(ProductPrice * (1 - _discount / 100)), 2);

                RaisePropertyChanged(() => Price);
                RaisePropertyChanged(() => Discount);
                RaisePropertyChanged(() => Amount);
                OnInvoiceItemChanged();
            }
        }
        public decimal Amount { get { return (Price ?? 0) * (Quantity ?? 0); } }
        public DateTime? ExpiryDate
        {
            get { return _expiryDate; }
            set
            {
                if (value == _expiryDate) { return; }
                _expiryDate = value; RaisePropertyChanged(() => ExpiryDate);
            }
        }
        public string Note
        {
            get { return _note; }
            set { _note = value; RaisePropertyChanged(() => Note); }
        }


        public decimal Percentage { get { return Price.HasValue && Price != 0 ? Product != null ? ((Product.Price ?? 0) - (Price ?? 0)) * 100 / Price.Value : 0 : 100; } }
        #endregion

        #region Internal methods
        private void OnInvoiceItemChanged()
        {
            OnPropertyChanged("InvoiceItemsModel");
            var handler = InvoiceItemChanged;
            if (handler != null) handler(this);
        }
        #endregion Internal methods

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
