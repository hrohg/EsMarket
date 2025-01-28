using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using EsMarket.SharedData.Interfaces;
using static EsMarket.SharedData.Models.Taxpayer;

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

    [XmlInclude(typeof(AccountingDocument))]
    [Serializable]
    public class AccountingDocument
    {
        public int Type { get; set; }
        public bool Traceable { get; set; }
        public GeneralInfo GeneralInfo { get; set; }
        public SupplierInfo SupplierInfo { get; set; }
        public BuyerInfo BuyerInfo { get; set; }
        public List<Good> GoodsInfo { get; set; }
    }

    public class GeneralInfo
    {
        public string DeliveryDate { get; set; }
        public int Procedure { get; set; }
        public GeneralInfo()
        {
            DeliveryDate = DateTime.Now.ToString("yyyy-MM-ddK");
        }
    }

    [XmlInclude(typeof(BankAccount))]
    [Serializable]
    public class BankAccount
    {
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
    }
    public class Taxpayer
    {
        public string TIN { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public BankAccount BankAccount { get; set; }
    }
    public class SupplierInfo
    {
        public Taxpayer Taxpayer { get; set; }
        public string SupplyLocation { get; set; }
    }
    public class BuyerInfo
    {
        public Taxpayer Taxpayer { get; set; }
        public string DeliveryMethod { get; set; }
        public string DeliveryLocation { get; set; }

    }

    public class Good
    {
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Amount { get; set; }
        public string PricePerUnit { get; set; }
        public string Price { get; set; }
        public string TotalPrice { get; set; }
    }
}
