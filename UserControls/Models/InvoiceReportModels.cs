using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserControls.Models
{
    public class InvoiceReportModel
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get { return Quantity * Price; } }
    }

    public class ProductProviderReportModel
    {
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string Partner { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Mu { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get { return Quantity * Price; } }
    }
}
