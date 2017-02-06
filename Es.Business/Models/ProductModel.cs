using System;
using System.Windows.Media;
using ES.Business.Managers;
using ES.Data.Models;

namespace ES.Business.Models
{
    public class ProductModel : EsProductModel
    {
        /// <summary>
        /// Initialize a new instance of the Product class.
        /// </summary>

        #region Properties

        #endregion
        #region private properties

        #endregion
        #region Public properties
        public string EditProductStage
        {
            get
            {
                return new ProductsManager().GetProduct(Id, EsMemberId) != null ? "Խմբագրել" : "Ավելացնել";
            }
        }
        public string State { get { return IsEnabled ? "Ակտիվ" : "Պասիվ"; } }
        public Brush ProductStateHighlighthing { get { return IsEnabled ? Brushes.Green : Brushes.Red; } }
        public Brush ProductCountHighlighthing { get { return MinQuantity == null ? Brushes.BlueViolet : ExistingQuantity > MinQuantity ? Brushes.Green : Brushes.Red; } }
        #endregion
        #region Constructors
        public ProductModel()
        {
            
        }
        public ProductModel(long memberId, long lastModifierId, bool isEnable)
        {
            EsMemberId = memberId;
            LastModifierId = lastModifierId;
            IsEnabled = isEnable;
        }
        public ProductModel(string code, long memberId, long lastModifierId, bool isEnable)
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
    }

    public class ProductProvider
    {
        public Guid ProviderId { get; set; }
        public Guid ProductId { get; set; }
    }
}
