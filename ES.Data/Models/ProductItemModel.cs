using System;
using System.ComponentModel;

namespace ES.Data.Models
{
    public class ProductItemModel:INotifyPropertyChanged
    {
        #region Product item model Properties
        private const string StockIdProperty = "StockId";
        private const string QuantityProperty = "Quantity";
        private const string CostPriceProperty = "CostPrice";
        private const string CoordinateXProperty = "CoordinateX";
        private const string CoordinateYProperty = "CoordinateY";
        private const string CoordinateZProperty = "CoordinateZ";
        private const string DescriptionProperty = "Description";
        #endregion

        #region Product item model private properties
        private Guid _id;
        private Guid _productId;
        private Guid _createdInvoiceId;
        private Guid? _deliveryInvoiceId;
        private short? _stockId;
        private decimal _quantity;
        private decimal _costPrice;
        private string _coordinateX;
        private string _coordinateY;
        private string _coordinateZ;
        private string _description;
        private int? _reservedById;
        private int _memberId;
        private DateTime _createdDate;
        private DateTime? _deliveryDate;

        #endregion

        #region Product item model public properties
        public Guid Id {get { return _id; } set { _id = value; }}
        public Guid ProductId { get { return _productId; } set { _productId = value;  }}
        public Guid CreateInvoiceId { get { return _createdInvoiceId; } set { _createdInvoiceId = value; } }
        public Guid? DeliveryInvoiceId { get { return _deliveryInvoiceId; } set { _deliveryInvoiceId = value; } }
        public short? StockId { get { return _stockId; } set { _stockId = value; OnPropertyChanged(StockIdProperty); } }
        public decimal Quantity { get { return _quantity; } set { _quantity = value;OnPropertyChanged(QuantityProperty); } }
        public decimal CostPrice { get { return _costPrice; } set { _costPrice = value; OnPropertyChanged(CostPriceProperty); } }
        public DateTime? ExpiryDate { get; set; }
        public string CoordinateX { get { return _coordinateX; } set { _coordinateX = value; OnPropertyChanged(CoordinateXProperty); } }
        public string CoordinateY { get { return _coordinateY; } set { _coordinateY = value; OnPropertyChanged(CoordinateYProperty); } }
        public string CoordinateZ { get { return _coordinateZ; } set { _coordinateZ = value; OnPropertyChanged(CoordinateZProperty);} }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public int? ReservedById { get { return _reservedById; } set { _reservedById = value; } }
        public int MemberId { get { return _memberId; } set { _memberId = value; } }
        public Products.ProductModel Product { get; set; }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        public DateTime? DeliveryDate
        {
            get { return _deliveryDate; }
            set { _deliveryDate = value; }
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
