using System.Collections.Generic;
using EsMarket.SharedData.Enums;

namespace EsMarket.SharedData.Interfaces
{
    public interface IEsMarketInvoice
    {
        IInvoiceInfo InvoiceInfo { get; set; }
        IDeliveryInfo DeliveryInfo { get; set; }
        IEsMarketPartner SupplierInfo { get; set; }
        IEsMarketPartner BuyerInfo { get; set; }
        string Notes { get; set; }
        IList<IGoodsInfo> GoodsInfo { get; set; }
    }
}