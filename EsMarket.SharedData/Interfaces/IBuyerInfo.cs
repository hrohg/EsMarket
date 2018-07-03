using EsMarket.SharedData.Enums;

namespace EsMarket.SharedData.Interfaces
{
    public interface IBuyerInfo
    {
        IEsMarketPartner Partner { get; set; }
        IDeliveryInfo DeliveryInfo { get; set; }
    }
}