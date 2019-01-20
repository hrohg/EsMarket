using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [XmlInclude(typeof(EsMarketInvoice))]
    [Serializable]
    public class EsMarketInvoice : IEsMarketInvoice
    {
        private InvoiceInfo _invoiceInfo;
        private DeliveryInfo _deliveryInfo;
        private EsMarketPartner _supplierInfo;
        private EsMarketPartner _buyerInfo;
        private string _notes;
        private List<EsGoodInfo> _goodsInfo;
        [XmlElement(typeof(InvoiceInfo))]
        public InvoiceInfo InvoiceInfo
        {
            get { return _invoiceInfo ?? new InvoiceInfo(); }
            set { _invoiceInfo = value; }
        }
        [XmlElement(typeof(DeliveryInfo))]
        public DeliveryInfo DeliveryInfo
        {
            get { return _deliveryInfo ?? new DeliveryInfo(); }
            set { _deliveryInfo = value; }
        }
        [XmlElement(typeof(EsMarketPartner))]
        public EsMarketPartner SupplierInfo
        {
            get { return _supplierInfo ?? new EsMarketPartner(); }
            set { _supplierInfo = value; }
        }
        [XmlElement(typeof(EsMarketPartner))]
        public EsMarketPartner BuyerInfo
        {
            get { return _buyerInfo ?? new EsMarketPartner(); }
            set { _buyerInfo = value; }
        }

        public string Notes
        {
            get { return _notes ?? string.Empty; }
            set { _notes = value; }
        }

        public List<EsGoodInfo> GoodsInfo
        {
            get { return _goodsInfo ?? new List<EsGoodInfo>(); }
            set { _goodsInfo = value; }
        }
    }
}
