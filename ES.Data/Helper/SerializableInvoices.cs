using System;
using System.Collections.Generic;
using System.Configuration;
using ES.Data.Models;

namespace ES.Data.Helper
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class SerializableInvoice
    {
        #region External properties
        public InvoiceModel Invoice { get; set; }
        public List<InvoiceItemsModel> InvoiceItems { get; set; }
        #endregion External properties

        #region Constructors

        public SerializableInvoice()
        {

        }
        public SerializableInvoice(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems):this()
        {
            Invoice = invoice;
            InvoiceItems = invoiceItems;
        }
        #endregion Constructors
    }
}

