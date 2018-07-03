using System;
using EsMarket.SharedData.Enums;

namespace EsMarket.SharedData.Interfaces
{
    public interface IDeliveryInfo
    {
        DateTime DeliveryDate { get; set; }
        DeliveryTypeEnum DeliveryMethod { get; set; }
        IAddress DeliveryLocation { get; set; } 
    }
}