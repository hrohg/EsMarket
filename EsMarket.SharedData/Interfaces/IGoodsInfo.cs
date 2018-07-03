namespace EsMarket.SharedData.Interfaces
{
    public interface IGoodsInfo
    {
        string Description { get; set; }
        string Unit { get; set; }
        string Quantity { get; set; }
        string Price { get; set; }
        string Total { get; }
    }
}