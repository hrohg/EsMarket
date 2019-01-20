using EsMarket.SharedData.Interfaces;
using EsMarket.SharedData.Models;
using ES.Data.Models;

namespace ES.Data.Converters
{
    public class EsMarketInvoiceConverter
    {
        public static EsMarketInvoice ConvertToEsMarketInvoice(InvoiceModel invoice, InvoiceItemsModel invoiceItem)
        {
            EsMarketInvoice newItem = null;
            return newItem;
        }
        public static InvoiceModel ConvertToInvoiceModel(EsMarketInvoice esMarketInvoice)
        {
            var newItem = new InvoiceModel()
            {
                
            };
            return newItem;
        }
    }
    
}
