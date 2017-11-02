using System;

namespace ES.Data.Models.Reports
{
    public interface IInvoiceReport
    {
        decimal? Sale { get; }
    }
    public class InvoiceReport : IInvoiceReport
    {
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public decimal? Sale { get; set; }
        public decimal Profit { get { return (decimal) (Sale!=null? Sale - Cost:0); } }
        public string Pers { get { return (Sale!=null && Sale != 0 ? (double)((Sale - Cost) * 100 / Sale.Value) : 0.0).ToString("N2"); } }
    }

    public class InvoiceReportByPartner : IInvoiceReport
    {
        public string Invoice { get; set; }
        public DateTime? Date { get; set; }
        public string Partner { get; set; }
        public int Count { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Sale { get; set; }
        public decimal? Profit { get { return Sale - Cost; } }
        public decimal? Pers { get { return ((Sale - Cost) * 100 / (Cost != 0 ? Cost : 1)); } }
        public string Approver { get; set; }

    }
}
