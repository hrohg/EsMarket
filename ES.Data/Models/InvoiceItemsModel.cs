using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ES.Data.Models
{
    public class InvoiceItemsModel : INotifyPropertyChanged
    {
        public InvoiceItemsModel(InvoiceModel invoice)
        {
            Invoice = invoice;
        }
        public InvoiceItemsModel()
        {
        }
        public InvoiceItemsModel(InvoiceModel invoice, ProductModel product)
            : this(invoice)
        {
            if (product == null) return;
            Product = product;
            ProductId = product.Id;
            ExpiryDate = product.ExpiryDays != null ? DateTime.Today.AddDays((int)product.ExpiryDays) : (DateTime?)null;
            Code = product.Code;
            Description = product.Description;
            Mu = product.Mu;
            Discount = product.Discount;
            Note = product.Note;
        }

        #region Invoice items model properties
        private const string IndexProperty = "Index";
        private const string ProductProperty = "Product";
        private const string CodeProperties = "Code";
        private const string DescriptionProperties = "Description";
        private const string MuProperties = "Mu";
        private const string QuantityProperties = "Quantity";
        private const string PriceProerties = "Price";
        private const string CostPriceProperty = "CostPrice";
        private const string PercentageProperty = "Percentage";
        private const string DiscountProperties = "Discount";
        private const string NoteProperties = "Note";
        private const string AmountProperty = "Amount";
        private const string ExpiryDateProperty = "ExpiryDate";
        #endregion

        #region Invoice items model private properties
        private Guid _id = Guid.NewGuid();
        private int _index;
        private Guid _productId;
        private ProductModel _product;
        private ProductItemModel _productItem;
        private Guid? _productItemId;
        private string _code;
        private string _description;
        private string _mu;
        private string _note;
        private decimal? _quantity;
        private decimal? _price;
        private decimal? _costPrice;
        private DateTime? _expiryDate;
        private decimal? _discount;
        #endregion

        #region Invoice items public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public int Index
        {
            get { return _index; }
            set
            {
                if (_index == value) { return; }
                _index = value;
                OnPropertyChanged(IndexProperty);
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
        public ProductModel Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
                if (_product == null) return;
                _productId = _product.Id;
                _code = _product != null ? _product.Code : null;
                _description = _product != null ? _product.Description : null;
                _mu = _product != null ? _product.Mu : null;
                _note = _product != null ? _product.Note : null;
                OnPropertyChanged(ProductProperty);
            }
        }
        [XmlIgnore]
        public ProductItemModel ProductItem
        {
            get
            {
                return _productItem;
            }
            set
            {
                _productItem = value;
                if (value == null) return;
                _productItemId = _productItem.Product != null ? _productItem.Id : (Guid?)null;
                _productId = _productItem != null ? _productItem.ProductId : Guid.Empty;
                _code = _productItem.Product != null ? _productItem.Product.Code : null;
                _description = _productItem.Product != null ? _productItem.Product.Description : null;
                _mu = _productItem.Product != null ? _productItem.Product.Mu : null;
                _note = _productItem.Product != null ? _productItem.Product.Note : null;
            }
        }
        public Guid ProductId { get { return _productId; } set { _productId = value; } }
        public Guid? ProductItemId { get { return _productItemId; } set { _productItemId = value; } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(CodeProperties); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperties); } }
        public string Mu { get { return _mu; } set { _mu = value; OnPropertyChanged(MuProperties); } }
        public decimal? Quantity { get { return _quantity; } set { _quantity = value; 
            OnPropertyChanged(QuantityProperties); 
            OnPropertyChanged(AmountProperty);
            OnPropertyChanged("CostAmount");
        } }
        public decimal? Price { get { return _price; } set { _price = value; OnPropertyChanged(PriceProerties); OnPropertyChanged(AmountProperty); OnPropertyChanged(PercentageProperty); } }
        public decimal? CostPrice { get { return _costPrice; } set { _costPrice = value; 
            OnPropertyChanged(CostPriceProperty); 
            OnPropertyChanged(PercentageProperty);
            OnPropertyChanged("CostAmount");
        } }
        public DateTime? ExpiryDate
        {
            get { return _expiryDate; }
            set
            {
                if (value == _expiryDate) { return; }
                _expiryDate = value; OnPropertyChanged(ExpiryDateProperty);
            }
        }
        public decimal Percentage { get { return Price.HasValue && Price != 0 ? Product != null ? ((Product.Price ?? 0) - (Price ?? 0)) * 100 / Price.Value : 0 : 100; } }
        public decimal Amount { get { return (Quantity != null && Price != null) ? (decimal)Price * (decimal)Quantity : 0; } }
        public decimal CostAmount { get { return (CostPrice ?? 0) * (Quantity ?? 0); } }
        public decimal? Discount { get { return _discount; } set { _discount = value; OnPropertyChanged(DiscountProperties); OnPropertyChanged(AmountProperty); } }
        public string Note { get { return _note; } set { _note = value; OnPropertyChanged(NoteProperties); } }

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
