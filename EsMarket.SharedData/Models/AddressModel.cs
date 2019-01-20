using System;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [Serializable]
    public class AddressModel:IAddress
    {
        private string _address;

        public string Address
        {
            get { return _address ?? string.Empty; }
            set { _address = value; }
        }
    }
}
