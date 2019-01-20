using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EsMarket.SharedData.Interfaces;

namespace EsMarket.SharedData.Models
{
    [XmlInclude(typeof(BankAccount))]
    [Serializable]
    public class BankAccount:IBankAccount
    {
        private string _bankName;
        private string _bankAccountNumber;

        public string BankName
        {
            get { return _bankName ?? string.Empty; }
            set { _bankName = value; }
        }

        public string BankAccountNumber
        {
            get { return _bankAccountNumber ?? string.Empty; }
            set { _bankAccountNumber = value; }
        }
    }
}
