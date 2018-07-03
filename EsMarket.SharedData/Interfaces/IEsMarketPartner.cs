namespace EsMarket.SharedData.Interfaces
{
    public interface IEsMarketPartner
    {
        string Tin { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        IBankAccount BankAccount { get; set; }
    }
}