using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ES.Data.Model;

namespace ES.Data.Models
{
    public class EsProductModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties
        private const string IdProperty = "Id";
        private const string EditProductStageProperty = "EditProductStage";
        private const string CodeProperty = "Code";
        private const string BarcodeProperty = "Barcode";
        private const string HcdCsProperty = "HcdCs";
        private const string DescriptionProperty = "Description";
        private const string MuProperty = "Mu";
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
        private string _mu;
        private string _note;
        private decimal? _costPrice;
        private decimal? _oldPrice;
        private decimal? _price;
        private decimal? _discount;
        private decimal? _profitPercent;
        private decimal? _dealerPrice;
        private decimal? _dealerDiscount;
        private decimal? _dealerProfitPercent;
        private int? _expiryDays;
        private decimal? _minQuantity;
        private decimal? _existingQuantity;
        private string _imagePath;
        private bool _isEnable;
        private long? _brandId;
        private long _esMemberId;
        private BrandModel _brand;
        private EsMemberModel _esMember;
        private long _lastModifierId;
        private EsUserModel _esUser;
        #endregion
        #region Public properties
        public Guid Id { get { return _id; } set { _id = value; OnPropertyChanged(IdProperty); OnPropertyChanged(EditProductStageProperty); } }
        public string State { get { return IsEnabled ? "Ակտիվ" : "Պասիվ"; } }
        public string Code { get { return _code; } set { _code = value; OnPropertyChanged(CodeProperty); } }
        public string Barcode { get { return _barcode; } set { _barcode = value; OnPropertyChanged(BarcodeProperty); } }
        public string HcdCs { get { return _hcdCs; } set { _hcdCs = value; OnPropertyChanged(HcdCsProperty); } }
        public List<ProductGroupModel> ProductGroups { get; set; }
        public List<ProductCategoriesModel> ProductCategories { get; set; }
        public string Description { get { return _description; } set { _description = value; OnPropertyChanged(DescriptionProperty); } }
        public string Mu { get { return _mu; } set { _mu = value; OnPropertyChanged(MuProperty); } }
        public bool? IsWeight { get; set; }
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
                Price = ProfitPercent != null && CostPrice != null ? CostPrice * (1 + ProfitPercent / 100) : null;
                DealerPrice = DealerProfitPercent != null && CostPrice != null ? CostPrice * (1 + DealerProfitPercent / 100) : null;
                OnPropertyChanged(CostPriceProperty); OnPropertyChanged(ProfitPercentProperty); OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        public decimal? OldPrice { get { return _oldPrice; } set { _oldPrice = value; } }
        public decimal? Price
        {
            get { return _price; }
            set
            {
                if (_price == value) { return; }
                _price = value;
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
        public decimal? ProfitPercent
        {
            get { return _price != null && CostPrice != null && CostPrice != 0 ? _price * 100 / CostPrice - 100 : null; ; }
            set
            {
                //    if (_profitPercent == value) { return;}
                //    _profitPercent = value;
                Price = value != null && CostPrice != null ? CostPrice * (100 + value) / 100 : null;
                //   OnPropertyChanged(ProfitPercentProperty);
                //    OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        public bool HasDealerPrice { get { return _dealerPrice != null; } }
        public decimal? DealerPrice
        {
            get { return _dealerPrice ?? Price; }
            set
            {
                if (_dealerPrice == value)
                {
                    return;
                }
                _dealerPrice = value;
                //DealerProfitPercent =_dealerPrice!=null && CostPrice != null && CostPrice != 0 && _dealerPrice!=null ? _dealerPrice * 100 / CostPrice - 100 : null;
                OnPropertyChanged(DealerPriceProperty);
                OnPropertyChanged(DealerProfitPercentProperty);
            }
        }

        public decimal? DealerDiscount
        {
            get { return _dealerDiscount; }
            set
            {
                if (_dealerDiscount == value) { return; } _dealerDiscount = value;
                OnPropertyChanged(DealerDiscountProperty);
            }
        }

        public decimal? DealerProfitPercent
        {
            get { return _dealerPrice != null && CostPrice != null && CostPrice != 0 && _dealerPrice != null ? _dealerPrice * 100 / CostPrice - 100 : null; }
            set
            {

                //if (_dealerProfitPercent == value) { return;}
                //_dealerProfitPercent = value;
                DealerPrice = CostPrice != null && value != null ? _costPrice * (100 + value) / 100 : null;
                OnPropertyChanged(DealerProfitPercentProperty);
            }
        }
        public int? ExpiryDays { get { return _expiryDays; } set { _expiryDays = value; OnPropertyChanged(ExpiryDaysProperty); } }
        public decimal? MinQuantity { get { return _minQuantity; } set { _minQuantity = value; OnPropertyChanged(MinQuantityProperty); } }
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
        public long? BrandId { get { return _brandId; } set { _brandId = value; OnPropertyChanged(BrandIdProperty); } }
        public long EsMemberId { get { return _esMemberId; } set { _esMemberId = value; } }
        public BrandModel Brand { get { return _brand; } set { _brand = value; } }
        public EsMemberModel EsMember { get { return _esMember; } set { _esMember = value; } }
        public long LastModifierId { get { return _lastModifierId; } set { _lastModifierId = value; } }
        public EsUserModel EsUser { get { return _esUser; } set { _esUser = value; } }
        #endregion
        #region Constructors
        public EsProductModel()
        {
            IsEnabled = true;
        }
        public EsProductModel(long memberId, long lastModifierId, bool isEnable)
            : this()
        {
            EsMemberId = memberId;
            LastModifierId = lastModifierId;
            IsEnabled = isEnable;
        }
        public EsProductModel(string code, long memberId, long lastModifierId, bool isEnable)
            : this(memberId, lastModifierId, isEnable)
        {
            Code = code;
        }
        //public ProductModel() { }
        #endregion
        #region private methods
        public void SetProduct(EsProductModel item)
        {
            if (item == null) return;
            Id = item.Id;
            Code = item.Code;
            Description = item.Description;
            Mu = item.Mu;
            Note = item.Note;
            CostPrice = item.CostPrice;
            OldPrice = item.OldPrice;
            Price = item.Price;
            Discount = item.Discount;
            DealerPrice = item.DealerPrice;
            DealerDiscount = item.DealerDiscount;
            ImagePath = item.ImagePath;
            IsEnabled = item.IsEnabled;
            BrandId = item.BrandId;
            Brand = item.Brand;
            EsMemberId = item.EsMemberId;
            EsMember = item.EsMember;
            LastModifierId = item.LastModifierId;
            EsUser = item.EsUser;
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

    public class ProductGroupModel
    {
        public Guid ProductId { get; set; }
        public string Barcode { get; set; }
        public int MemberId { get; set; }
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
