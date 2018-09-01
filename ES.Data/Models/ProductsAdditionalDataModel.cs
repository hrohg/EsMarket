using System.Xml.Serialization;
using CashReg.Helper;

namespace ES.Data.Models
{
    public partial class ProductModel
    {
        [XmlIgnore]
        public TypeOfTaxes TypeOfTaxes { get; set; }
    }
}
