namespace EsMarket.SharedData.Interfaces
{
    public interface IGoodsInfo
    {
        string Code { get; set; }
        string Description { get; set; }
        string Unit { get; set; }
        decimal Quantity { get; set; }
        decimal Price { get; set; }
        decimal Total { get; set; }
    }
}