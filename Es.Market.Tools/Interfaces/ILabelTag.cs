using System;
using Es.Market.Tools.Helpers;
using Es.Market.Tools.Models;
using ES.Data.Models;
using System.ComponentModel;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace Es.Market.Tools.Interfaces
{
    public interface ILabelTag: INotifyPropertyChanged, ICloneable
    {
        LabelType Type { get; set; }
        LabelSize Size { get; } 
        ProductModel Product { get; set; }
    }
}