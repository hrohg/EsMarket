using System;
using System.ComponentModel;
using ES.Data.Models;

namespace ES.Business.Models
{
    public class ProductOrderItemsModel
    {
        /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties
        private const string IdProperty = "Id";
        private const string ProductIdProperty = "ProductId";
        private const string CodeProperty = "Code";
        private const string DescriptionProperty = "Description";
        private const string MuProperty = "Mu";
        private const string QuantityProperty = "Quantity";
        private const string ExistingQuantityProperty = "ExistingQuantity";
        private const string CostPriceProperty = "CostPrice";
        private const string PriceProperty = "Price";
        private const string AmountProperty = "Amount";
        private const string NoteProperty = "Note";
        #endregion
        #region private properties
        private Guid _id = Guid.NewGuid();
        private Guid _productOrderId;
        private Guid? _productId;
        private long _lastModifierId;
        private DateTime _modifyDate;
        private string _code;
        private string _description;
        private string _mu;
        private decimal? _quantity;
        private decimal? _existingQuantity;
        private decimal? _costPrice;
        private decimal? _price;
        private string _note;
        #endregion
        #region public properties
        public Guid Id { get { return _id; } set { _id = value; } }
        public Guid ProductOrderId { get { return _productOrderId; } set { _productOrderId = value; } }
        public Guid? ProductId { get { return _productId; } set { _productId = value; } }
        public PartnerModel Provider { get; set; }
        public Guid? ProviderId { get; set; }
        public long LastModifierId { get { return _lastModifierId; } set { _lastModifierId = value; } }
        public DateTime LastModifyDate { get { return _modifyDate; } set { _modifyDate = value; } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(CodeProperty); } }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string Mu { get { return _mu; } set { _mu = value; OnPropertyChanged(MuProperty); } }
        public decimal? Quantity { get { return _quantity; } set { _quantity = value; OnPropertyChanged(QuantityProperty); OnPropertyChanged(AmountProperty); } }
        public decimal? ExistingQuantity { get { return _existingQuantity; } set { _existingQuantity = value; OnPropertyChanged(ExistingQuantityProperty); } }
        public decimal? CostPrice { get { return _costPrice; } set { _costPrice = value; OnPropertyChanged(CostPriceProperty); } }
        public decimal? Price { get { return _price; } set { _price = value; OnPropertyChanged(PriceProperty); OnPropertyChanged(AmountProperty); } }
        public decimal Amount { get { return (Quantity ?? 0) * (Price ?? 0); } }
        public string Note { get { return _note; } set { _note = value; OnPropertyChanged(NoteProperty); } }
        #endregion
        #region Constructors

        public ProductOrderItemsModel()
        {

        }
        #endregion
        #region private methods

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
