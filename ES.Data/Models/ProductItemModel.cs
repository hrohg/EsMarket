using System;
using System.ComponentModel;
using ES.Data.Model;

namespace ES.Data.Models
{
    public class ProductItemModel:INotifyPropertyChanged
    {
        #region Product item model Properties
        private const string IdProperty = "Id";
        private const string ProductIdProperty = "ProductId";
        private const string DeliveryInvoiceIdProperty = "DeliveryInvoiceId";
        private const string StockIdProperty = "StockId";
        private const string QuantityProperty = "Quantity";
        private const string CostPriceProperty = "CostPrice";
        private const string CoordinateXProperty = "CoordinateX";
        private const string CoordinateYProperty = "CoordinateY";
        private const string CoordinateZProperty = "CoordinateZ";
        private const string DescriptionProperty = "Description";
        private const string ReservedByIdProperty = "ReservedById";
        private const string MemberIdProperty = "MemberId";
        #endregion
        #region Product item model private properties
        private Guid _id;
        private Guid _productId;
        private Guid _deliveryInvoiceId;
        private long? _stockId;
        private decimal _quantity;
        private decimal _costPrice;
        private string _coordinateX;
        private string _coordinateY;
        private string _coordinateZ;
        private string _description;
        private long? _reservedById;
        private long _memberId;
        #endregion
        #region Product item model public properties
        public Guid Id {get { return _id; } set { _id = value; OnPropertyChanged(IdProperty); }}
        public Guid ProductId { get { return _productId; } set { _productId = value; OnPropertyChanged(ProductIdProperty); }}
        public Guid DeliveryInvoiceId {get { return _deliveryInvoiceId; } set { _deliveryInvoiceId = value; OnPropertyChanged(DeliveryInvoiceIdProperty); }}
        public long? StockId { get { return _stockId; } set { _stockId = value; OnPropertyChanged(StockIdProperty); } }
        public decimal Quantity { get { return _quantity; } set { _quantity = value;OnPropertyChanged(QuantityProperty); } }
        public decimal CostPrice { get { return _costPrice; } set { _costPrice = value; OnPropertyChanged(CostPriceProperty); } }
        public DateTime? ExpiryDate { get; set; }
        public string CoordinateX { get { return _coordinateX; } set { _coordinateX = value; OnPropertyChanged(CoordinateXProperty); } }
        public string CoordinateY { get { return _coordinateY; } set { _coordinateY = value; OnPropertyChanged(CoordinateYProperty); } }
        public string CoordinateZ { get { return _coordinateZ; } set { _coordinateZ = value; OnPropertyChanged(CoordinateZProperty);} }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public long? ReservedById { get { return _reservedById; } set { _reservedById = value; OnPropertyChanged(ReservedByIdProperty); } }
        public long MemberId { get { return _memberId; } set { _memberId = value; OnPropertyChanged(MemberIdProperty); } }
        public EsProductModel Product { get; set; }
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
