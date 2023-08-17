using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ES.Data.Models.Products;

namespace ES.Data.Models
{
    public interface IProductOrderModel : INotifyPropertyChanged
    { ObservableCollection<StockProducts> StockProducts { get; set; } }
    public class ProductOrderModel : IProductOrderModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        #region External properties
        public EsProductModel Product { get; set; }
        public ObservableCollection<StockProducts> StockProducts { get; set; }

        public int Index { get; set; }
        public Guid ProductId { get; set; }

        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal CostAmount { get { return ExistingQuantity * (CostPrice ?? 0); } }
        public decimal Amount { get { return ExistingQuantity * (Price ?? 0); } }
        public decimal? MinQuantity { get; set; }
        public decimal ExistingQuantity { get; set; }
        public decimal SaleQuantity { get; set; }
        public decimal DemandQuantity { get { var demandQuantity = (MinQuantity ?? 0) + SaleQuantity - ExistingQuantity;  return demandQuantity > 0 ? demandQuantity : 0; ; } }
        public PartnerModel Provider { get; set; }
        public string ProviderDescription { get { return Provider != null ? Provider.FullName : ""; } }
        public string Notes { get; set; }

        public ProductOrderModel()
        {
            StockProducts = new ObservableCollection<StockProducts>();
        }

        public bool HasKey(string key)
        {
            return string.IsNullOrEmpty(key) || Description.ToLower().Contains(key.ToLower()) || Code.Contains(key) || Product == null || Product.HasKey(key);
        }
        #endregion
    }
}
