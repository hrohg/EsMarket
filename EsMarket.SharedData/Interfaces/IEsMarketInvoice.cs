using System.Collections.Generic;
using EsMarket.SharedData.Models;

namespace EsMarket.SharedData.Interfaces
{

    public interface IEsMarketInvoice
    {
        InvoiceInfo InvoiceInfo { get; set; }
        DeliveryInfo DeliveryInfo { get; set; }
        EsMarketPartner SupplierInfo { get; set; }
        EsMarketPartner BuyerInfo { get; set; }
        string Notes { get; set; }
        List<EsGoodInfo> GoodsInfo { get; set; }
    }
}