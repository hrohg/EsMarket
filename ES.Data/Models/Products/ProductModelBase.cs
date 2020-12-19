using System.Xml.Serialization;
using ES.Data.Models.EsModels;

namespace ES.Data.Models.Products
{
    public partial class ProductModel
    {
        private EsmMeasureUnitModel _measureUnit;
        public EsmMeasureUnitModel MeasureUnit { get { return _measureUnit; } set { _measureUnit = value; } }
        public int? MeasureUnitId { get { return MeasureUnit!=null? MeasureUnit.Id:(int?)null; } }

        [XmlIgnore]
        public string Mu { get { return MeasureUnit != null ? MeasureUnit.Name : string.Empty; } }
        [XmlIgnore]
        public bool IsWeight { get { return MeasureUnit != null && (MeasureUnit.IsWeight ?? false); } }


    }
}
