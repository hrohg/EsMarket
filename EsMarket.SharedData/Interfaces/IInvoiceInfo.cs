using EsMarket.SharedData.Enums;

namespace EsMarket.SharedData.Interfaces
{
    public interface IInvoiceInfo
    {
        InvoiceTypeEnum Type { get; set; }
        string InvoiceNumber { get; set; }
    }
}
