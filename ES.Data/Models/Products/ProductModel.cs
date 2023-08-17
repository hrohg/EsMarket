using System;
using System.Windows.Media;
using System.Xml.Serialization;
using EsMarket.SharedData.Models;

namespace ES.Data.Models.Products
{
    [Serializable]
    public partial class ProductModel : EsProductModel, ICloneable, IComparable
    {
        /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties

        #endregion

        #region private properties

        #endregion

        #region Public properties
        [XmlIgnore]
        public Brush ProductStateHighlighthing { get { return IsEnabled ? Brushes.Green : Brushes.Red; } }
        [XmlIgnore]
        public Brush ProductCountHighlighthing { get { return MinQuantity == null ? Brushes.BlueViolet : ExistingQuantity > MinQuantity ? Brushes.Green : Brushes.Red; } }
        public PartnerModel Provider { get; set; }
        #endregion

        #region Constructors
        public ProductModel()
        {

        }
        public ProductModel(int memberId, int lastModifierId, bool isEnable)
        {
            EsMemberId = memberId;
            LastModifierId = lastModifierId;
            IsEnabled = isEnable;
        }
        public ProductModel(string code, int memberId, int lastModifierId, bool isEnable)
        {
            Code = code;
            EsMemberId = memberId;
            LastModifierId = lastModifierId;
            IsEnabled = isEnable;
        }
        //public ProductModel() { }
        #endregion

        #region private methods
        public void SetProduct(ProductModel item)
        {
            if (item == null) return;
            Id = item.Id;
            Code = item.Code;
            Description = item.Description;
            //MeasureUnit = item.Mu;
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

        //#region INotifyPropertyChanged
        //public event PropertyChangedEventHandler PropertyChanged;
        //private void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}
        //#endregion

        public EsGood ToEsGoods()
        {
            return new EsGood
            {
                Code = Code,
                HcdCs = HcdCs,
                Barcode = Barcode,
                Description = Description,
                Unit = Mu,
                Price = Price ?? 0,
                CostPrice = CostPrice ?? 0,
                DealerPrice = DealerPrice
            };
        }
        private decimal GetProductDealerPrice()
        {
            return (DealerPrice ?? 0) * (1 - (DealerDiscount ?? 0) / 100);
        }
        private decimal GetProductPrice()
        {
            return (Price ?? 0) * (1 - (Discount ?? 0) / 100);
        }
        public decimal ProductDealerPrice
        {
            get
            {
                return DealerPrice.HasValue ? GetProductDealerPrice() : GetProductPrice();
            }
        }
        public decimal ProductPrice
        {
            get
            {
                return Math.Max(GetProductPrice(), HasDealerPrice ? GetProductDealerPrice() : 0);
            }
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public int CompareTo(object obj)
        {
            var value = obj as ProductModel;
            if (value == null) return -1;
            if (value.Id == Id) return 0;
            return StringComparer.OrdinalIgnoreCase.Compare(value.Code, Code);
        }        
    }

    public class ProductProvider
    {
        public Guid ProviderId { get; set; }
        public Guid ProductId { get; set; }
    }
}
