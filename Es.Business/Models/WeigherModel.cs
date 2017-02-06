using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ES.Business.Models
{
    //[Serializable()]
    //[XmlRoot(ElementName = "Scale", Namespace = "Exported Product for Scale")]
   public class ScaleModel
    {
        #region Internal properties
        #endregion
        #region External properties

        public string Connection { get; set; }
        [XmlArray("NewDataSet")]
        [XmlArrayItem("Report")]
        public List<WeigherProduct> WeigherProducts;
        #endregion
        public ScaleModel()
        {
            WeigherProducts = new List<WeigherProduct>();
        }
        public ScaleModel(string connection):this()
        {
            Connection = connection;
        }

        public void SetProducts(ProductModel product)
        {
            if (product == null) { return; }
            WeigherProducts.Add(new WeigherProduct
            {
                Code = product.Code,
                Barcode = product.Code,
                Description = product.Description,
                Price = product.Price ?? 0,
                IsPiece = product.IsWeight != null && product.IsWeight == true ? (short)0 : (short)1
            });
        }
        public void SetProducts(List<ProductModel> products)
        {
            foreach (var product in products)
            {
                SetProducts(product);
            }
        }

    }
    

    [Serializable()]
    [XmlType("Report")]
    [XmlRoot("NewDataSet")]
    public class WeigherProduct
    {
        #region Internal properties
        private string _barcode;
        private string _code;
        private string _description;
        private string _brand;
        private decimal _price;
        private bool _isPiece;
        #endregion

        #region External properties
        public string Code { get { return _code; } set { if (string.Equals(value, _code)) { return; } _code = value; } }
        public string Barcode { get { return _barcode; } set { if (string.Equals(value, _barcode)) { return; } _barcode = value; } }
        public string Description { get { return _description; } set { if (string.Equals(value, _description)) { return; } _description = value; } }
        public decimal Price { get { return _price; } set { if (value == _price) { return; } _price = value; } }
        public short IsPiece { get { return (short)(_isPiece ? 1 : 0); } set { _isPiece = value != 0; } }
        #endregion

        public WeigherProduct()
        {
            
        }
    }
}
