﻿using System;

namespace ES.Data.Models
{
    public class ProductOrderModel
    {
        #region External properties
        public int Index { get; set; }
        public Guid ProductId { get; set; }
        public EsProductModel Product { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal CostAmount { get { return ExistingQuantity*(CostPrice??0); } }
        public decimal Amount { get { return ExistingQuantity*(Price??0); } }
        public decimal? MinQuantity { get; set; }
        public decimal ExistingQuantity { get; set; }
        public decimal SaleQuantity { get; set; }
        public decimal DemandQuantity { get { return (MinQuantity ?? 0) + SaleQuantity - ExistingQuantity; } }
        public string Provider { get; set; }
        public string Notes { get; set; }
        #endregion
    }
}
