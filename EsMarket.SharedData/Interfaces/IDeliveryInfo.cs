using System;
using EsMarket.SharedData.Enums;
using EsMarket.SharedData.Models;

namespace EsMarket.SharedData.Interfaces
{
    public interface IDeliveryInfo
    {
        
        DateTime DeliveryDate { get; set; }
        DeliveryTypeEnum DeliveryMethod { get; set; }
        AddressModel DeliveryLocation { get; set; } 
    }
}