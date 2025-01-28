using System;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [Serializable]
    public class EsMarketPartner : IEsMarketPartner
    {
        private string _tin;
        private string _name;
        private string _address;
        private BankAccount _bankAccount;

        public string Tin
        {
            get { return _tin ?? string.Empty; }
            set { _tin = value; }
        }

        public string Name
        {
            get { return _name ?? string.Empty; }
            set { _name = value; }
        }

        public string Address
        {
            get { return _address ?? string.Empty; }
            set { _address = value; }
        }
        
        public string SupplyAddress { get; set; }

        public BankAccount BankAccount
        {
            get { return _bankAccount??(_bankAccount = new BankAccount()); }
            set { _bankAccount = value; }
        }
    }
}
