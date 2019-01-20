using System;
using System.Xml.Serialization;
using EsMarket.SharedData.Enums;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [Serializable]
    public class DeliveryInfo : IDeliveryInfo
    {
        private DateTime? _deliveryDate;
        private DeliveryTypeEnum _deliveryMethod;
        private AddressModel _deliveryLocation;
        public DateTime DeliveryDate
        {
            get { return _deliveryDate ?? DateTime.Today; }
            set { _deliveryDate = value; }
        }
        public DeliveryTypeEnum DeliveryMethod
        {
            get { return _deliveryMethod; }
            set { _deliveryMethod = value; }
        }
        public AddressModel DeliveryLocation
        {
            get { return _deliveryLocation ?? (_deliveryLocation = new AddressModel()); }
            set { _deliveryLocation = value; }
        }
    }
}
