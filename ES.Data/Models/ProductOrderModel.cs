using System;

namespace ES.Data.Models
{
    public class ProductOrderModel
    {
        #region External properties
        public int Index { get; set; }
        public Guid ProductId { get; set; }
        public string Provider { get; set; }
        public EsProductModel Product { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal CostAmount { get { return ExistingQuantity * (Product != null ? Product.CostPrice ?? 0 : 0); } }
        public decimal Amount { get { return ExistingQuantity*(Product!=null?Product.Price??0:0); } }
        public decimal? MinQuantity { get; set; }
        public decimal ExistingQuantity { get; set; }
        public decimal SaleQuantity { get; set; }
        public decimal NeededQuantity { get { return (MinQuantity ?? 0) + SaleQuantity - ExistingQuantity; } }
        public string Note { get; set; }
        #endregion
    }
}
