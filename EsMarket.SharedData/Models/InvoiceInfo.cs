using System;
using System.Xml.Serialization;
using EsMarket.SharedData.Enums;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [XmlInclude(typeof(InvoiceInfo))]
    [Serializable]
    public class InvoiceInfo:IInvoiceInfo
    {
        private InvoiceTypeEnum _type;
        private string _invoiceNumber;

        public InvoiceTypeEnum Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string InvoiceNumber
        {
            get { return _invoiceNumber ?? string.Empty; }
            set { _invoiceNumber = value; }
        }
    }
}
