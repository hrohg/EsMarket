using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace ES.Data.Models.Products
{
    [Serializable]
    public class EsProductModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties
        private const string IdProperty = "Id";
        private const string CodeProperty = "Code";
        private const string BarcodeProperty = "Barcode";
        private const string HcdCsProperty = "HcdCs";
        private const string DescriptionProperty = "Description";

        private const string NoteProperty = "Note";
        private const string CostPriceProperty = "CostPrice";
        private const string ProfitPercentProperty = "ProfitPercent";
        private const string OldPriceProperty = "OldPrice";
        private const string PriceProperty = "Price";
        private const string DiscountProperty = "Discount";
        private const string DealerPriceProperty = "DealerPrice";
        private const string DealerProfitPercentProperty = "DealerProfitPercent";
        private const string DealerDiscountProperty = "DealerDiscount";
        private const string ImagePathProperty = "ImagePath";
        private const string IsEnableProperty = "IsEnable";
        private const string BrandIdProperty = "BrandId";
        private const string EsMemberIdProperty = "EsMemberId";
        private const string LastModifierIdProperty = "LastModifierId";
        private const string MinQuantityProperty = "MinQuantity";
        private const string ExistingQuantityProperty = "ExistingQuantity";
        private const string ExpiryDaysProperty = "ExpiryDays";
        private const string StateProperty = "State";
        private const string IsEnabledProperty = "IsEnabled";
        private const string ProductStateHighlighthingProperty = "ProductStateHighlighthing";
        #endregion

        #region private properties
        private Guid _id = Guid.NewGuid();
        private string _code;
        private string _barcode;
        private string _hcdCs;
        private string _description;
        private string _note;
        private decimal? _costPrice;
        private decimal? _oldPrice;
        private decimal? _price;
        private decimal? _discount;
        private decimal? _profitPercent;
        private decimal? _dealerPrice;
        private decimal? _dealerDiscount;
        private decimal? _dealerProfitPercent;
        private short? _expiryDays;
        private decimal? _minQuantity;
        private decimal? _existingQuantity;
        private string _imagePath;
        private bool _isEnable;
        private int? _brandId;
        private int _esMemberId;
        private BrandModel _brand;
        private EsMemberModel _esMember;
        private int _lastModifierId;
        private DateTime _lastModifiedDate;
        private EsUserModel _esUser;
        #endregion

        #region Public properties
        [XmlIgnore]
        public Guid Id { get { return _id; } set { _id = value; OnPropertyChanged(IdProperty); } }
        public string State { get { return IsEnabled ? "Ակտիվ" : "Պասիվ"; } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(CodeProperty); } }
        public string Barcode { get { return _barcode; } set { _barcode = value; OnPropertyChanged(BarcodeProperty); } }
        public string HcdCs { get { return _hcdCs; } set { _hcdCs = value; OnPropertyChanged(HcdCsProperty); } }
        [XmlIgnore]
        public List<ProductKeysModel> ProductKeys { get; set; }
        [XmlIgnore]
        public List<ProductCategoriesModel> ProductCategories { get; set; }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string Note { get { return _note; } set { _note = value; OnPropertyChanged(NoteProperty); } }
        public decimal? CostPrice
        {
            get
            {
                return _costPrice;
            }
            set
            {
                if (_costPrice == value) { return; }
                _costPrice = value;
                OnPropertyChanged(CostPriceProperty);
                if (CostPrice != null)
                {
                    Price = ProfitPercent != null && CostPrice != null ? CostPrice * (1 + ProfitPercent / 100) : null;
                    DealerPrice = DealerProfitPercent != null && CostPrice != null ? CostPrice * (1 + DealerProfitPercent / 100) : null;
                    OnPropertyChanged(ProfitPercentProperty); OnPropertyChanged(DealerProfitPercentProperty);
                }
            }
        }
        public decimal? OldPrice { get { return _oldPrice; } set { _oldPrice = value; } }
        public decimal? Price
        {
            get { return _price; }
            set
            {
                if (_price == value) { return; }
                _price = value != null ? decimal.Round((decimal)value, 2) : (decimal?)null;
                //ProfitPercent = _price!=null && CostPrice!=null && CostPrice != 0 ? _price * 100 / CostPrice - 100 :null;
                OnPropertyChanged(PriceProperty);
                OnPropertyChanged(ProfitPercentProperty);
                OnPropertyChanged(DealerPriceProperty);
            }
        }
        public decimal? Discount
        {
            get { return _discount; }
            set
            {
                if (_discount == value) { return; }
                _discount = value;
                OnPropertyChanged(DiscountProperty);
            }
        }

        [XmlIgnore]
        public decimal? ProfitPercent
        {
            get { return _price > 0 && CostPrice > 0 ? (100 * _price / CostPrice - 100) : null; }
            set
            {
                if (value == null) { return; }
                //    _profitPercent = value;
                Price = value != null && CostPrice != null ? CostPrice * (100 + value) / 100 : null;
                //   OnPropertyChanged(ProfitPercentProperty);
                //    OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        [XmlIgnore]
        public bool HasDiscount { get { return Discount != null; } }
        [XmlIgnore]
        public bool HasDealerPrice { get { return _dealerPrice.HasValue; } }
        public decimal? DealerPrice
        {
            get { return _dealerPrice ?? Price; }
            set
            {
                if (_dealerPrice == value)
                {
                    return;
                }
                _dealerPrice = value != null ? decimal.Round((decimal)value, 2) : (decimal?)null;
                //DealerProfitPercent hasnt value setter
                //DealerProfitPercent = CostPrice > 0 && _dealerPrice != null ? (_dealerPrice - CostPrice) * 100 / CostPrice : null;
                OnPropertyChanged(DealerPriceProperty);
                OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        [XmlIgnore]
        public bool HasDealerDiscount { get { return DealerDiscount != null; } }
        public decimal? DealerDiscount
        {
            get { return _dealerDiscount; }
            set
            {
                if (_dealerDiscount == value) { return; }
                _dealerDiscount = value;
                OnPropertyChanged(DealerDiscountProperty);
            }
        }
        [XmlIgnore]
        public decimal? DealerProfitPercent
        {
            get { return _dealerPrice > 0 && CostPrice > 0 ? 100 * _dealerPrice / CostPrice - 100 : null; }
            set
            {
                if (value == null) { return; }
                //_dealerProfitPercent = value;
                DealerPrice = CostPrice != null && value != null ? _costPrice * (100 + value) / 100 : null;
                OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        public short? ExpiryDays { get { return _expiryDays; } set { _expiryDays = value; OnPropertyChanged(ExpiryDaysProperty); } }
        public decimal? MinQuantity { get { return _minQuantity; } set { _minQuantity = value; OnPropertyChanged(MinQuantityProperty); } }
        [XmlIgnore]
        public decimal? ExistingQuantity { get { return _existingQuantity; } set { _existingQuantity = value; OnPropertyChanged(ExistingQuantityProperty); } }
        public string ImagePath { get { return _imagePath; } set { _imagePath = value; OnPropertyChanged(ImagePathProperty); } }
        public bool IsEnabled
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value; OnPropertyChanged(IsEnableProperty);
                OnPropertyChanged(StateProperty); OnPropertyChanged(ProductStateHighlighthingProperty);
            }
        }
        public int? BrandId { get { return _brandId; } set { _brandId = value; OnPropertyChanged(BrandIdProperty); } }
        [XmlIgnore]
        public int EsMemberId { get { return _esMemberId; } set { _esMemberId = value; } }
        [XmlIgnore]
        public BrandModel Brand { get { return _brand; } set { _brand = value; } }
        [XmlIgnore]
        public EsMemberModel EsMember { get { return _esMember; } set { _esMember = value; } }
        [XmlIgnore]
        public int LastModifierId { get { return _lastModifierId; } set { _lastModifierId = value; } }
        [XmlIgnore]
        public EsUserModel LastModifier { get; set; }
        //[XmlIgnore]
        public DateTime LastModifiedDate { get { return _lastModifiedDate; } set { _lastModifiedDate = value; } }
        [XmlIgnore]
        public EsUserModel EsUser { get { return _esUser; } set { _esUser = value; } }
        #endregion

        #region Constructors
        public EsProductModel()
        {
            IsEnabled = true;
        }
        public EsProductModel(int memberId, int lastModifierId, bool isEnable)
            : this()
        {
            EsMemberId = memberId;
            LastModifierId = lastModifierId;
            IsEnabled = isEnable;
            ProductKeys = new List<ProductKeysModel>();
        }
        public EsProductModel(string code, int memberId, int lastModifierId, bool isEnable)
            : this(memberId, lastModifierId, isEnable)
        {
            Code = code;
        }
        //public ProductModel() { }
        #endregion        

        #region private methods
        #endregion

        #region External methods
        public void SetCostPrice(decimal? value)
        {
            _costPrice = value;
            OnPropertyChanged("CostPrice");
        }
        public void SetProfitPercent(decimal? value)
        {
            _profitPercent = value;
            OnPropertyChanged("ProfitPercent");
        }
        public void SetPrice(decimal? value)
        {
            _price = value;
            OnPropertyChanged("Price");
        }
        public void SetDiscount(decimal? value)
        {
            _discount = value;
            OnPropertyChanged("Discount");
        }
        public void SetDealerProfitPercent(decimal? value)
        {
            _dealerProfitPercent = value;
            OnPropertyChanged("DealerProfitPercent");
        }
        public void SetDealerPrice(decimal? value)
        {
            _dealerPrice = value;
            OnPropertyChanged("DealerPrice");
        }
        public void SetDealerDiscount(decimal? value)
        {
            _dealerDiscount = value;
            OnPropertyChanged("DealerDiscount");
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

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return true;
            key = key.ToLower().Trim();

            return Description.ToLower().Contains(key) ||
                   Code.ToLower().Contains(key) ||
                   Barcode != null && Barcode.ToLower().Contains(key) ||
                   ProductKeys.Any(t => t.ProductKey.ToLower().Contains(key));
        }
    }

    public class ProductKeysModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductKey { get; set; }
        public int MemberId { get; set; }

        public ProductKeysModel(string productKey, Guid productId, int memberId)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            ProductKey = productKey;
            MemberId = memberId;
        }
    }

    public class ProductCategoriesModel
    {
        #region External properties
        public Guid Id { get; set; }
        public int CategoriesId { get; set; }
        public Guid ProductId { get; set; }
        #endregion
    }
}
