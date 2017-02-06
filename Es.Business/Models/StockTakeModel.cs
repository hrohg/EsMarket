using System;
using System.ComponentModel;

namespace ES.Business.Models
{
    /// <summary>
    /// Stock taking model
    /// </summary>
    public class StockTakeModel
    {
        #region Properties
        #endregion
        #region Private properties
        private Guid _id = Guid.NewGuid();
        private long _memberId;
        private long? _stockId;
        private DateTime _createDate;
        private DateTime? _closedDate;
        private long? _stockTakeNumber;
        private long _creatorId;
        private long? _modifierId;
        private long? _closerId;
        private string _description;
        #endregion
        #region Public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public long MemberId { get { return _memberId; } set { _memberId = value; } }
        public long? StockId { get { return _stockId; } set { _stockId = value; } }
        public DateTime CreateDate { get { return _createDate; } set { _createDate = value; } }
        public DateTime? ClosedDate { get { return _closedDate; } set { _closedDate = value; } }
        public long? StockTakeNumber { get { return _stockTakeNumber; } set { _stockTakeNumber = value; } }
        public string StockTakeName { get { return _stockTakeNumber != null ? string.Format("ST-{0}{1}", ((DateTime)CreateDate).ToString("yy"), ((long)_stockTakeNumber).ToString("D3")) : null; } }
        public long CreatorId { get { return _creatorId; } set { _creatorId = value; } }
        public long? ModifierId { get { return _modifierId; } set { _modifierId = value; } }
        public long? CloserId { get { return _closerId; } set { _closerId = value; } }
        public string Description { get { return _description; } set { _description = value; } }
        #endregion
    }
    /// <summary>
    /// Stock taking item model
    /// </summary>
    public class StockTakeItemsModel : INotifyPropertyChanged
    {
        #region Properties
        private const string ProductDescriptionProperty = "ProductDescription";
        private const string CodeOrBarcodeProperty = "CodeOrBarcode";
        private const string MuProperty = "Mu";
        private const string PriceProperty = "Price";
        private const string DescriptionProperty = "Description";
        private const string QuantityProperty = "Quantity";
        private const string StockTakeQuantityProperty = "StockTakeQuantity";
        private const string BalanceProperty = "Balance"; 
        private const string AmountProperty = "Amount";
        private const string StockTakeDateProperty = "StockTakeDate";
        #endregion
        #region Private properties
        private Guid _id = Guid.NewGuid();
        private Guid _stockTakeId;
        private Guid? _productId;
        private string _productDescription;
        private string _codeOrBarcode;
        private string _mu;
        private decimal? _price;
        private string _description;
        private decimal _quantity;
        private decimal? _stockTakeQuantity;
        private DateTime _stockTakeDate;
        #endregion
        #region Public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public Guid StockTakeId { get { return _stockTakeId; } set { _stockTakeId = value; } }
        public Guid? ProductId { get { return _productId; } set { _productId = value; } }
        public string ProductDescription { get { return _productDescription; } set { _productDescription = value; OnPropertyChanged(ProductDescriptionProperty); } }
        public string Mu { get { return _mu; } set { _mu = value; OnPropertyChanged(MuProperty); } }
        public decimal? Price { get { return _price; } set { _price = value; OnPropertyChanged(PriceProperty); } }
        public string CodeOrBarcode { get { return _codeOrBarcode; } set { _codeOrBarcode = value; OnPropertyChanged(CodeOrBarcodeProperty); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }

        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
                OnPropertyChanged(QuantityProperty);
                OnPropertyChanged(BalanceProperty); OnPropertyChanged(AmountProperty);
            }
        }

        public decimal StockTakeQuantity
        {
            get { return _stockTakeQuantity??0; }
            set { _stockTakeQuantity = value; OnPropertyChanged(StockTakeQuantityProperty); OnPropertyChanged(BalanceProperty); OnPropertyChanged(AmountProperty); }
        }
        public decimal? Balance { get { return StockTakeQuantity - Quantity; } }
        public decimal Amount { get { return (StockTakeQuantity - Quantity) * (Price??0); } }
        public DateTime StockTakeDate { get { return _stockTakeDate; } set { _stockTakeDate = value; OnPropertyChanged(StockTakeDateProperty); } }
        #endregion
        public StockTakeItemsModel(Guid stockTakeId)
        {
            StockTakeId = stockTakeId;
            Quantity = 0;
        }
        /// <summary>
        /// Default INotifyPropertyChanged
        /// </summary>
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
