using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.Interfaces;

namespace UserControls.ViewModels.Reports
{
    public class ShortReportViewModel : DocumentViewModel
    {
        #region InvoiceView models properties
        private const string InvoicesProperty = "Invoices";
        private const string SaleItemsProperty = "SaleItems";
        #endregion

        #region Internal fields
        private Thread _tdUpdate;
        #endregion

        #region Internal properties
        private bool _isLoading;
        private FinanceReportModel _report;
        private InvoiceModel _invoice;
        private List<InvoiceModel> _invoices;
        private List<InvoiceItemsModel> _invoiceItems;
        #endregion

        #region External properties
        public string Title { get; set; }
        public string Description { get; set; }
        
        public List<InvoiceModel> Invoices
        {
            get { return _invoices; }
            set
            {
                _invoices = value;
                _invoiceItems = _report.InvocieItems.ToList();
                RaisePropertyChanged(InvoicesProperty); 
                RaisePropertyChanged(SaleItemsProperty);
            }
        }
        public InvoiceModel Invoice
        {
            get { return _invoice; }
            set
            {
                if(value == _invoice) return;
                _invoice = value;
                RaisePropertyChanged("Invoice");
                RaisePropertyChanged(SaleItemsProperty);
                RaisePropertyChanged("IsShowInvocieItems");
                RaisePropertyChanged("InvoiceItems");
            }
        }
        public List<ProductOrderItemsModel> SaleItems
        {
            get
            {
                return _report == null
                    ? null
                    : _report.InvocieItems.OrderBy(s => s.Code)
                        .GroupBy(s => s.Code + s.Price)
                        .Select(s => new ProductOrderItemsModel
                        {
                            ProductId = s.Select(t => t.ProductId).First(),
                            Code = s.Select(t => t.Code).First(),
                            Description = s.Select(t => t.Description).First(),
                            Mu = s.Select(t => t.Mu).First(),
                            Quantity = s.Sum(t => t.Quantity),
                            ExistingQuantity = _report.ProductResidues.Where(pr => pr.ProductId == s.First().ProductId).Select(pr => pr.Quantity).FirstOrDefault(),
                            Price = s.Select(t => t.Price).First(),
                            Note =
                                s.Select(t => t.Product != null ? t.Product.Note : string.Empty)
                                    .First()
                        }).ToList();
            }
        }
        public ObservableCollection<ShortReport> ShortReport { get; set; }
        public ObservableCollection<SaleBy> Sallers { get; set; }
        public ObservableCollection<SaleBy> Customers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        private MyTime _startTime;
        public MyTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; RaisePropertyChanged("StartTime"); }
        }
        public MyTime EndTime { get; set; }
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (_isLoading == value)
                {
                    return;
                }
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }
        public List<InvoiceItemsModel> InvoiceItems
        {
            get
            {
                return Invoice != null ? _invoiceItems.Where(ii => ii.InvoiceId == Invoice.Id).ToList() : new List<InvoiceItemsModel>();
            }
        }
        public bool IsShowInvocieItems
        {
            get
            {
                return Invoice != null;
            }
        }

        #endregion

        #region Constructors
        public ShortReportViewModel()
        {
            Initialize();
        }
        #endregion

        #region Internal Methods
        private void Initialize()
        {
            Title = "Համառոտ վերլուծություններ";
            StartDate = EndDate = DateTime.Today;
            StartTime = new MyTime();
            EndTime = new MyTime();
            IsInput = IsOutput = true;
            ResetInvoiseCommand = new RelayCommand(OnResetInvoice);

            OnRefresh(null);
        }

        private void OnResetInvoice(object obj)
        {
            Invoice = null;
        }
        private void OnRefresh(object o)
        {
            _tdUpdate = new Thread(Update);
            _tdUpdate.Start();
        }

        private void OnBreak(object o)
        {
            _tdUpdate.Abort();
            IsLoading = false;
        }
        private void Update()
        {
            IsLoading = true;

            var fromDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, StartTime.Hour, StartTime.Minute, StartTime.Second);
            var toDate = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, EndTime.Hour, EndTime.Minute, EndTime.Second);
            _report = InvoicesManager.GetInvoicesFinance(fromDate.Date, toDate.Date.AddDays(1), ApplicationManager.Instance.GetEsMember.Id);

            var fromTimeValue = StartTime.Hour * 3600 + StartTime.Minute * 60 + StartTime.Second;
            var toTimeValue = EndTime.Hour * 3600 + EndTime.Minute * 60 + EndTime.Second;
            var invoices = _report.InvocieItems.Select(s => s.Invoice).ToList();
            invoices = invoices.Where(s =>
                        (IsInput && s.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice) ||
                        (IsOutput && s.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).GroupBy(s => s.Id).Select(s => s.First()).ToList();
            if (StartTime.Hour != EndTime.Hour || StartTime.Minute != EndTime.Minute || StartTime.Second != EndTime.Second)
                invoices = invoices.Where(s =>
                            s.CreateDate.Hour * 3600 + s.CreateDate.Minute * 60 + s.CreateDate.Second > fromTimeValue &&
                            s.CreateDate.Hour * 3600 + s.CreateDate.Minute * 60 + s.CreateDate.Second < toTimeValue).ToList();
            //var invoiceItems = invoicesReports.Select(s=>s.InvocieItem);
            Invoices = invoices;
            var totalPurchase = invoices.Where(s => s.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).Sum(s => s.Total);
            var profitPurchase = _invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (long)InvoiceType.PurchaseInvoice).Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0)) - totalPurchase;
            var totalSale = invoices.Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).Sum(s => s.Total);
            var profitSale = _invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (long)InvoiceType.SaleInvoice).Sum(s => ((s.Price ?? 0) - HgConvert.ToDecimal(s.CostPrice ?? 0)) * (s.Quantity ?? 0));
            var totalCost = _invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (long)InvoiceType.SaleInvoice).Sum(ii => (ii.CostPrice ?? 0) * (ii.Quantity ?? 0));
            ShortReport = new ObservableCollection<ShortReport>()
            {
                new ShortReport
                {
                    Description = "Մուտքեր", 
                    Count = invoices.Count(s => s.InvoiceTypeId==(long)InvoiceType.PurchaseInvoice).ToString("N2"),
                    Amount = totalPurchase.ToString("N2"),
                    Detile = totalPurchase!=0? (profitPurchase/totalPurchase).ToString("P"): "0%"
                },
                new ShortReport
                {
                    Description = "Ելքեր", 
                    Count = invoices.Count(s => s.InvoiceTypeId==(long)InvoiceType.SaleInvoice).ToString("N2"),
                    Amount = totalSale.ToString("N2"),
                    Detile = totalSale!=0? (profitSale/totalSale).ToString("P"): "0%"
                },
                new ShortReport
                {
                    Description = "Հաշվեկշիռ",
                    Count = "",
                    Amount =(totalPurchase-totalSale).ToString("N"),
                    Detile = ""
                },
                new ShortReport
                {
                    Description = "Ինքնարժեք",
                    Count = profitSale.ToString("N2"),
                    Amount = totalCost.ToString("N2"),
                    Detile = totalSale!=0? (profitSale/totalSale).ToString("P"): "0%"
                }
            };
            var invoicesByApprover = invoices.Where(s => s.InvoiceTypeId == (int)InvoiceType.SaleInvoice).GroupBy(s => s.ApproverId).ToList();
            var invoicesByPartner = invoices.Where(s => s.InvoiceTypeId == (int)InvoiceType.SaleInvoice).GroupBy(s => s.PartnerId).ToList();
            Sallers = new ObservableCollection<SaleBy>(invoicesByApprover.Select(s => new SaleBy { Description = s.First().Approver, Total = s.Sum(t => t.Total).ToString("N") }));
            Customers = new ObservableCollection<SaleBy>(invoicesByPartner.Where(s => s.FirstOrDefault() != null).Select(s =>
                new SaleBy { Description = s.First().Partner != null ? s.First().Partner.FullName : string.Empty, Total = s.Sum(t => t.Total).ToString("N") }));

            Description = string.Format("{0} {1} - {2}", "Համառոտ վերլուծություններ", fromDate, toDate);
            RaisePropertyChanged("Description");
            RaisePropertyChanged("Customers");
            RaisePropertyChanged("Sallers");
            RaisePropertyChanged("ShortReport");
            IsLoading = false;
        }
        #endregion

        #region Commands
        public ICommand RefreshCommand { get { return new RelayCommand(OnRefresh); } }
        public ICommand BreakCommand { get { return new RelayCommand(OnBreak); } }
        public ICommand ResetInvoiseCommand { get; private set; }
        #endregion
    }
    public class ShortReport
    {
        public string Description { get; set; }
        public string Count { get; set; }
        public string Amount { get; set; }
        public string Detile { get; set; }
    }

    public class SaleBy
    {
        public string Description { get; set; }
        public string Total { get; set; }
    }

    public class MyTime : INotifyPropertyChanged
    {
        private bool IsTimeFree = false;
        private const string HourProperty = "Hour";
        private const string MinuteProperty = "Minute";
        private const string SecondProperty = "Second";
        private int _hour = 0;
        private int _minute = 0;
        private int _second = 0;
        public int Hour { get { return _hour; } set { _hour = value; OnPropertyChanged(HourProperty); } }
        public int Minute
        {
            get { return _minute; }
            set
            {
                _minute = value % 60;
                Hour += value / 60; OnPropertyChanged(MinuteProperty);
            }
        }
        public int Second
        {
            get { return _second; }
            set
            {
                _second = value % 60;
                Minute += value / 60; OnPropertyChanged(SecondProperty);
            }
        }
        public MyTime()
        {
            Hour = 0;
            Minute = 0;
            Second = 0;
        }
        public MyTime(bool isTimeFree = false)
        {
            Hour = 0;
            Minute = 0;
            Second = 0;
            IsTimeFree = isTimeFree;
        }
        public MyTime(int hour = 0, int minute = 0, int second = 0, bool isTimeFree = false)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
            IsTimeFree = isTimeFree;
        }

        public int GetHour { get { return Hour; } }
        public int GetMinute { get { return Hour * 60 + Minute; } }
        public int GetSeccond { get { return Hour * 3600 + Minute * 60 + Second; } }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
