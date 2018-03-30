using System;

namespace ES.Data.Models.Reports
{
    public interface IInvoiceReport
    {
        DateTime Date { get; }
        decimal? Sale { get; }
        decimal Quantity { get; }
        decimal Price { get; }
        decimal Amount { get; }
    }

    public class InvoiceReport : IInvoiceReport
    {
        public DateTime Date { get; set; }
        public int Code { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get { return Quantity * Price; } }
        public decimal Cost { get; set; }
        public decimal? Sale { get; set; }
        public decimal Profit { get { return (decimal)(Sale != null ? Sale - Cost : 0); } }
        public string Pers { get { return (Sale != null && Sale != 0 ? (double)((Sale - Cost) * 100 / Sale.Value) : 0.0).ToString("N2"); } }
        public decimal ReturnAmount { get; set; }
        public decimal Total { get { return (Sale ?? 0) - ReturnAmount; } }
    }
    public class InvoiceReportByPartner : IInvoiceReport
    {
        public string Invoice { get; set; }
        public DateTime Date { get; set; }
        public string Partner { get; set; }
        public int Count { get; set; }
        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public decimal? Cost { get; set; }
        public decimal? Sale { get; set; }
        public decimal? Profit { get { return Sale - Cost; } }
        public decimal? Pers { get { return (Profit ?? 0) * 100 / (Cost != 0 ? Cost : 1); } }
        public string Approver { get; set; }

    }

    public class SaleReportByPartnerDetiled : InvoiceReportByPartner
    {
        public string Code { get; set; }
        public string Mu { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }
}
