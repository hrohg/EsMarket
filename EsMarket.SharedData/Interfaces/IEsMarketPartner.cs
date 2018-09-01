using EsMarket.SharedData.Models;

namespace EsMarket.SharedData.Interfaces
{
    public interface IEsMarketPartner
    {
        string Tin { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        BankAccount BankAccount { get; set; }
    }
}