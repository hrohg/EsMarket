using System.ComponentModel;
using Es.Market.Tools.Helpers;
using Es.Market.Tools.Interfaces;
using ES.Data.Annotations;
using ES.Data.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace Es.Market.Tools.Models
{
    public class LabelSize
    {
        protected double WidthInCm;
        protected double HeightInCm;
        public double Width { get { return WidthInCm * 37.7952755906; } }
        public double Height { get { return HeightInCm * 37.7952755906; } }

        public void SetWidthInCm(double width) { WidthInCm = width; }
        public void SetHeightInCm(double height) { HeightInCm = height; }
    }
    public class LabelModelBase: ILabelTag
    {
        private ProductModel _product;
        public LabelType Type { get; set; }
        public LabelSize Size { get { return LabelHelpers.GetLableSize(Type);} }

        public ProductModel Product
        {
            get { return _product; }
            set { _product = value; OnPropertyChanged("Product");}
        }

        public LabelModelBase(LabelType type)
        {
            Type = type;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class PriceTag : LabelModelBase
    {
        public bool ShowOldPrice { get; set; }

        public PriceTag(LabelType type):base(type)
        {
            
        }
    }
    public class PriceDroppedTag : LabelModelBase
    {
        public PriceDroppedTag()
            : base(LabelType.PriceDropped)
        {

        }
    }

}
