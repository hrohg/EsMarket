using ES.Common.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Data.Models.Products
{
    public class LogForProductsModel
    {
        public Guid Id { get; set; }
        public ModificationTypeEnum Action { get; set; }
        public string ActionType
        {
            get
            {
                switch (Action)
                {
                    case ModificationTypeEnum.Added:
                        return "Ավելացվել է:";
                    case ModificationTypeEnum.Modified:
                        return "Փոփոխվել է:";
                    case ModificationTypeEnum.Removed:
                        return "Հեռացվել է:";
                    default:
                        return "Անհայտ";

                };
            }
        }
        public string Availability { get { return IsEmpty ? "Առկա չէ": "Առկա"; } }
        public DateTime Date { get; set; }
        public Guid ProductId { get; set; }
        public ProductModel Product { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public Nullable<decimal> CostPrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        public decimal? OldPrice { get; set; }
        public double? PriceChangeProfit { get { return OldPrice > 0 ? (double)((Price - OldPrice) * 100 / OldPrice) : Price.HasValue ? 100 : (double?)null; } }
        public double? Profit { get { return CostPrice > 0 ? (double)((Price - CostPrice) * 100 / CostPrice) : Price.HasValue ? 100 : (double?)null; } }
        public bool IsEmpty { get; set; }
        public int ModifierId { get; set; }
        public EsUserModel Modifier { get; set; }
        public int MemberId { get; set; }
        public IEnumerable<LogForProductsModel> Log { get; set; }
        public LogForProductsModel()
        {
            Id = Guid.NewGuid();
        }
    }
}
