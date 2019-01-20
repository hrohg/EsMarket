using System;
using System.Xml.Serialization;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [Serializable]
    public class EsGoodInfo : IGoodsInfo
    {
        private string _description;
        private string _unit;
        private decimal _quantity;
        private decimal _price;
        private decimal _total;
        private string _code;
        private string _hcdCs;

        public string Code
        {
            get { return _code ?? string.Empty; }
            set { _code = value; }
        }
        [XmlIgnore]
        public string HcdCs
        {
            get { return _hcdCs ?? string.Empty; }
            set { _hcdCs = value; }
        }
        
        public string Description
        {
            get { return _description ?? string.Empty; }
            set { _description = value; }
        }

        public string Unit
        {
            get { return _unit ?? string.Empty; }
            set { _unit = value; }
        }

        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal Total
        {
            get { return _total; }
            set { _total = value; }
        }
    }

    [Serializable]
    public class EsGood : IGoods
    {
        private string _description;
        private string _unit;
        private decimal _price;
        private string _code;
        private string _hcdCs;

        public string Code
        {
            get { return _code ?? string.Empty; }
            set { _code = value; }
        }
        public string Barcode { get; set; }

        public string HcdCs
        {
            get { return _hcdCs ?? string.Empty; }
            set { _hcdCs = value; }
        }

        public string Description
        {
            get { return _description ?? string.Empty; }
            set { _description = value; }
        }

        public string Unit
        {
            get { return _unit ?? string.Empty; }
            set { _unit = value; }
        }
        public decimal CostPrice { get; set; }
        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }
        public decimal? DealerPrice { get; set; }
        
    }
}
