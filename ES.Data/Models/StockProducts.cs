using System;
using System.Collections;

namespace ES.Data.Models
{
    public class StockProducts : IComparable
    {
        public StockModel Stock { get; set; }
        public ProductItemModel Product { get; set; }
        public decimal Quantity { get; set; }

        public int CompareTo(StockProducts other)
        {
            if (other == null) return 0;
            return Quantity > other.Quantity ? 1 : -1;
        }

        public int CompareTo(object obj)
        {
            var o = obj as StockProducts;
            if (o == null) return 1;

            return Quantity >= o.Quantity ? 1 : -1;
        }
    }
}
