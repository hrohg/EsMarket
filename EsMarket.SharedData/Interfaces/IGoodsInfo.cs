namespace EsMarket.SharedData.Interfaces
{
    public interface IGoods
    {
        string Code { get; set; }
        string HcdCs { get; set; }
        string Description { get; set; }
        string Unit { get; set; }
        decimal Price { get; set; }
    }
    public interface IGoodsInfo : IGoods
    {
        decimal Quantity { get; set; }
        decimal Total { get; set; }
    }
}