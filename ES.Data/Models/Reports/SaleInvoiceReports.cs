using System;
using System.ComponentModel;
using CashReg.Managers;
using ES.Data.Annotations;
using ES.Data.Models.Products;

namespace ES.Data.Models.Reports
{
    public interface IInvoiceReport : INotifyPropertyChanged
    {
        DateTime Date { get; }
        decimal? Sale { get; set; }
        decimal Quantity { get; }
        decimal Cost { get; set; }
        decimal Price { get; }
        decimal Amount { get; }
    }
    public class InvoiceReport : IInvoiceReport
    {
        private decimal _cost;
        private decimal? _sale;
        private decimal _price;
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Quantity { get; set; }

        public decimal Price
        {
            get { return _price; }
            set { _price = value; OnPropertyChanged("Price"); }
        }

        public decimal Amount { get { return Quantity * Price; } }

        public decimal Cost
        {
            get { return _cost; }
            set { _cost = value; OnPropertyChanged("Cost"); }
        }

        public decimal? Sale
        {
            get { return _sale; }
            set { _sale = value; OnPropertyChanged("Sale"); }
        }

        public decimal Profit { get { return (decimal)(Sale != null ? Sale - Cost : 0); } }
        public string Pers { get { return (Sale != null && Sale != 0 ? (double)((Sale - Cost) * 100 / Sale.Value) : 0.0).ToString("N2"); } }
        public decimal ReturnAmount { get; set; }
        public decimal Total { get { return (Sale ?? 0) - ReturnAmount; } }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
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

        public decimal Cost { get; set; }
        public decimal? Sale { get; set; }
        public decimal? Profit { get { return (Price - Cost) * Quantity; } }
        public decimal Pers { get { return (Price - Cost) * 100 / (Cost > 0 ? Cost : 1); } }
        public string Approver { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SaleReportByPartnerDetiled : InvoiceReportByPartner
    {
        private PartnerModel _partner;
        private ProductModel _product;
        private short? _muId;
        public string Code { get; set; }
        public string Mu { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }

        public SaleReportByPartnerDetiled() { }
        public SaleReportByPartnerDetiled(PartnerModel partner, ProductModel product)
        {
            _partner = partner;
            _product = product;
            if (product != null) Mu = product.Mu;
        }
    }
}
