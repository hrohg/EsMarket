using System.Xml.Serialization;
using CashReg.Helper;

namespace ES.Data.Models.Products
{
    public partial class ProductModel
    {
        [XmlIgnore]
        public TypeOfTaxes TypeOfTaxes { get; set; }
        
    }
}
