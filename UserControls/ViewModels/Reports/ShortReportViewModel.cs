using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;

namespace UserControls.ViewModels.Reports
{
    public class ShortReportViewModel : DocumentViewModel
    {
        #region Internal fields
        private Thread _tdUpdate;
        private List<EsUserModel> _users;
        private List<PartnerModel> _partners;

        private string GetUserName(long? id)
        {
            var user = _users != null && id != null ? _users.SingleOrDefault(s => s.UserId == id) : null;
            return user == null ? string.Empty : user.UserName;
        }
        private string GetPartner(Guid? id)
        {
            var partner = _partners != null ? _partners.SingleOrDefault(s => s.Id == id) : null;
            return partner == null ? string.Empty : partner.FullName;
        }
        #endregion

        #region Internal properties

        //private FinanceReportModel _report;
        private InvoiceModel _invoice;
        private List<InvoiceModel> _invoices;
        private List<InvoiceItemsModel> _invoiceItems;
        #endregion

        #region External properties

        #region Start date
        private DateTime? _startDate;
        public DateTime StartDate
        {
            get
            {
                return _startDate != null ? new DateTime(_startDate.Value.Year, _startDate.Value.Month, _startDate.Value.Day,
                                 StartTime != null ? StartTime.Hour : 0, StartTime != null ? StartTime.Minute : 0, StartTime != null ? StartTime.Second : 0) : DateTime.Today;
            }
            set
            {
                if (value.Equals(_startDate)) return;
                if (_startDate == null) StartTime.SetTime(value.TimeOfDay);
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }
        #endregion Start date

        #region End date
        private DateTime? _endDate;
        public DateTime EndDate
        {
            get
            {
                return _endDate != null ? new DateTime(_endDate.Value.Year, _endDate.Value.Month, _endDate.Value.Day,
                                 EndTime != null ? EndTime.Hour : 0, EndTime != null ? EndTime.Minute : 0, EndTime != null ? EndTime.Second : 0) : DateTime.Now;
            }
            set
            {
                if (value.Equals(_endDate)) return;
                if(_endDate==null) EndTime.SetTime(value.TimeOfDay);
                _endDate = value;
                RaisePropertyChanged("EndDate");
            }
        }
        #endregion End date

        #region Start time

        private MyTime _startTime;
        public MyTime StartTime
        {
            get
            {
                return _startTime ?? (_startTime = new MyTime());
            }
            set
            {
                if (value.Equals(_startTime)) return;
                _startTime = value;
                RaisePropertyChanged("StartTime");
            }
        }
        #endregion Start time

        #region End time
        private MyTime _endTime;
        public MyTime EndTime
        {
            get
            {
                return _endTime ?? (_endTime = new MyTime());
            }
            set
            {
                if (value.Equals(_endTime)) return;
                _endTime = value;
                RaisePropertyChanged("EndTime");
            }
        }
        #endregion End time

        #region Invoice types
        private bool _isSale;

        public bool IsSale
        {
            get
            {
                return _isSale;
            }
            set
            {
                if (value == _isSale) return;
                _isSale = value; RaisePropertyChanged("IsSale");
                RaisePropertyChanged("Invoices");
                RaisePropertyChanged("SaleItems");
            }
        }

        private bool _isPurchase;
        public bool IsPurchase
        {
            get
            {
                return _isPurchase;
            }
            set
            {
                if (value == _isPurchase) return;
                _isPurchase = value;
                RaisePropertyChanged("IsPurchase");
                RaisePropertyChanged("Invoices");
                RaisePropertyChanged("SaleItems");
            }
        }

        private bool _isMove;

        public bool IsMove
        {
            get
            {
                return _isMove;
            }
            set
            {
                if (value == _isMove) return;
                _isMove = value;
                RaisePropertyChanged("IsMove");
                RaisePropertyChanged("Invoices");
                RaisePropertyChanged("SaleItems");
            }
        }

        #endregion Invoice types

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

        #region Is invoice items loading
        private bool _isInvocieItemsLoading;
        public virtual bool IsInvoiceItemsLoading
        {
            get
            {
                return _isInvocieItemsLoading && !IsLoading;
            }
            set
            {
                if (value == _isInvocieItemsLoading) return;
                _isInvocieItemsLoading = value;
                RaisePropertyChanged("IsInvoiceItemsLoading");
            }
        }
        #endregion Is invoice items loading

        #region Short invoice report
        public List<InvoiceModel> Invoices
        {
            get
            {
                return _invoices != null ? _invoices.Where(s =>
                    (IsSale && (InvoiceType)s.InvoiceTypeId == InvoiceType.SaleInvoice) ||
                    (IsPurchase && (InvoiceType)s.InvoiceTypeId == InvoiceType.PurchaseInvoice) ||
                    (IsMove && (InvoiceType)s.InvoiceTypeId == InvoiceType.MoveInvoice)).ToList() : new List<InvoiceModel>();
            }
        }
        #endregion Short invoice report

        #region Short invoice report from sale
        private List<InvoiceModel> _shortInvoiceReportFromSale;
        public List<InvoiceModel> ShortInvoiceReportFromSale
        {
            get { return _shortInvoiceReportFromSale; }
            set
            {
                _shortInvoiceReportFromSale = value;
                CostFromSale = _shortInvoiceReportFromSale.Sum(s => s.CostPrice);
                TotalSale = _shortInvoiceReportFromSale.Sum(s => s.Total);
                RaisePropertyChanged("ShortInvocieReportFromSale");
            }
        }

        #region Total Sale
        private decimal _totalSale;
        public decimal TotalSale
        {
            get
            {
                return _totalSale;
            }
            set
            {
                if (value == _totalSale) return;
                _totalSale = value;
                RaisePropertyChanged("TotalSale");
                RaisePropertyChanged("ProfitFromSale");
                RaisePropertyChanged("ProfitPrcentFromSale");
            }
        }
        #endregion Total Sale

        #region Cost From Sale
        private decimal _costFromSale;
        private List<ProductModel> _products;
        private List<ProductItemModel> _productItems;

        public decimal CostFromSale
        {
            get
            {
                return _costFromSale;
            }
            set
            {
                if (value == _costFromSale) return;
                _costFromSale = value;
                RaisePropertyChanged("CostFromSale");
                RaisePropertyChanged("ProfitFromSale");
                RaisePropertyChanged("ProfitPrcentFromSale");
            }
        }

        #endregion Cost From Sale

        public decimal ProfitFromSale { get { return TotalSale - CostFromSale; } }
        public decimal ProfitPrcentFromSale { get { return TotalSale != 0 ? ProfitFromSale * 100 / (CostFromSale != 0 ? CostFromSale : TotalSale) : 0; } }

        #endregion Short invoice report from sale


        public InvoiceModel Invoice
        {
            get { return _invoice; }
            set
            {
                if (value == _invoice) return;
                _invoice = value;
                RaisePropertyChanged("Invoice");
                RaisePropertyChanged("SaleItems");
                RaisePropertyChanged("IsShowInvocieItems");
                RaisePropertyChanged("InvoiceItems");
            }
        }
        public List<ProductOrderItemsModel> SaleItems
        {
            get
            {
                return _invoiceItems == null
                    ? null
                    : _invoiceItems
                    .Where(s => 
                        (IsSale && Invoices.Any(i=>i.Id == s.InvoiceId && i.InvoiceTypeId==(int)InvoiceType.SaleInvoice)) ||
                        (IsPurchase && Invoices.Any(i=>i.Id == s.InvoiceId && i.InvoiceTypeId==(int)InvoiceType.PurchaseInvoice)) ||
                        (IsMove && Invoices.Any(i=>i.Id == s.InvoiceId && i.InvoiceTypeId==(int)InvoiceType.MoveInvoice))).OrderBy(s => s.Code)
                        .GroupBy(s => s.Code + s.Price)
                        .Select(s => new ProductOrderItemsModel
                        {
                            ProductId = s.Select(t => t.ProductId).First(),
                            Code = s.Select(t => t.Code).First(),
                            Description = s.Select(t => t.Description).First(),
                            Mu = s.Select(t => t.Mu).First(),
                            Quantity = s.Sum(t => t.Quantity),
                            ExistingQuantity = _productItems != null ? _productItems.Where(pr => pr.ProductId == s.First().ProductId).Sum(pr => pr.Quantity) : (decimal?)null,
                            Price = s.Select(t => t.Price).First(),
                            Note = s.Select(t => t.Product != null ? t.Product.Note : string.Empty).First(),
                        }).ToList();
            }
        }

        #region Short report

        public List<ShortReport> ShortReport
        {
            get
            {
                return _invoices != null
                    ? new List<ShortReport>
                    {
                        new ShortReport
                        {
                            InvoiceType = InvoiceType.PurchaseInvoice,
                            ShortInvoiceReports = _invoices
                        },
                        new ShortReport
                        {
                            InvoiceType = InvoiceType.SaleInvoice,
                            ShortInvoiceReports = _invoices
                        },
                        new ShortReport
                        {
                            InvoiceType = InvoiceType.MoveInvoice,
                            ShortInvoiceReports = _invoices
                        },
                        new ShortReport
                        {
                            InvoiceType = InvoiceType.InventoryWriteOff,
                            ShortInvoiceReports = _invoices
                        }
                    }
                    : new List<ShortReport>();
            }
        }
        #endregion Short report

        public List<SaleBy> Sellers
        {
            get
            {
                return _invoices != null ? _invoices.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType.SaleInvoice)
                    .GroupBy(s => s.ApproverId).Select(s => new SaleBy
                {
                    Description = GetUserName(s.First().ApproverId),
                    Count = s.Count(),
                    Total = s.Sum(t => t.Total)
                }).ToList() : new List<SaleBy>();
            }
        }

        public List<SaleBy> Customers
        {
            get
            {
                return _invoices != null ? _invoices.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType.SaleInvoice)
                    .GroupBy(s => s.PartnerId).Select(s => new SaleBy
                {
                    Description = GetPartner(s.First().PartnerId),
                    Count = s.Count(),
                    Total = s.Sum(t => t.Total)
                }).ToList() : new List<SaleBy>();
            }
        }

        #endregion External properties

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
            StartDate = DateTime.Today;
            EndDate = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 23,59,59);
            IsSale = IsPurchase = true;
            ResetInvoiseCommand = new RelayCommand(OnResetInvoice);

            OnRefresh(null);
        }
        private void OnUpdateInvoices()
        {
            if (_invoiceItems != null && _invoiceItems.Any() && _invoices != null)
            {
                foreach (var invoiceModel in _invoices)
                {
                    invoiceModel.CostPrice = _invoiceItems.Where(s => s.InvoiceId == invoiceModel.Id).Sum(s => s.Quantity * s.CostPrice) ?? 0;
                    invoiceModel.Partner = _partners != null ? _partners.SingleOrDefault(s => s.Id == invoiceModel.PartnerId) : null;
                }
            }
            ShortInvoiceReportFromSale = _invoices != null ? _invoices.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType.SaleInvoice).ToList() : new List<InvoiceModel>();
            RaisePropertyChanged("Invoices");
            RaisePropertyChanged("Customers");
            RaisePropertyChanged("Sellers");
            RaisePropertyChanged("ShortReport");
            RaisePropertyChanged("SaleItems");
        }
        private void OnResetInvoice(object obj)
        {
            Invoice = null;
        }
        private void OnRefresh(object o)
        {
            _tdUpdate = new Thread(UpdateAsync);
            _tdUpdate.Start();
        }

        private void OnBreak(object o)
        {
            _tdUpdate.Abort();
            IsLoading = false;
        }
        private void UpdateAsync()
        {
            Description = string.Format("{0} {1} - {2}", "Համառոտ վերլուծություններ", StartDate, EndDate);
            RaisePropertyChanged("Description");

            new Thread(UpdateTools).Start();
            new Thread(UpdateShortReport).Start();
            new Thread(UpdateInvoiceItems).Start();
        }

        private void UpdateTools()
        {
            _partners = ApplicationManager.Instance.CashProvider.GetPartners;
            _users = ApplicationManager.Instance.CashProvider.GetUsers;
            _products = ApplicationManager.Instance.CashProvider.Products;
            _productItems = ApplicationManager.Instance.CashProvider.ProductItems;
            OnUpdateInvoices();
        }
        private void UpdateShortReport()
        {
            IsLoading = true;
            RaisePropertyChanged("IsInvoiceItemsLoading");
            _invoices = ReportManager.GetShortInvoiceReport(StartDate, EndDate);
            OnUpdateInvoices();
            IsLoading = false;
            RaisePropertyChanged("IsInvoiceItemsLoading");
        }

        private void UpdateInvoiceItems()
        {
            IsInvoiceItemsLoading = true;
            _invoiceItems = InvoicesManager.GetInvoiceItems(StartDate, EndDate);
            OnUpdateInvoices();
            //_report = InvoicesManager.GetInvoicesFinance(StartDate, EndDate);

            //var invoices = _report.InvocieItems.Select(s => s.Invoice).ToList();
            //invoices = invoices.Where(s => 
            //            (IsInput && s.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice) ||
            //            (IsOutput && s.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).GroupBy(s => s.Id).Select(s => s.First()).ToList();
            //if (StartTime.Hour != EndTime.Hour || StartTime.Minute != EndTime.Minute || StartTime.Second != EndTime.Second)
            //    invoices = invoices.Where(s => s.CreateDate >= StartDate && s.CreateDate <= EndDate).ToList();
            ////var invoiceItems = invoicesReports.Select(s=>s.InvocieItem);
            //Invoices = invoices;
            IsInvoiceItemsLoading = false;
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
        public InvoiceType InvoiceType;
        public List<InvoiceModel> ShortInvoiceReports { get; set; }
        public string Description
        {
            get
            {
                var descritpion = string.Empty;
                switch (InvoiceType)
                {
                    case InvoiceType.PurchaseInvoice:
                        descritpion = "Գնում";
                        break;
                    case InvoiceType.SaleInvoice:
                        descritpion = "Վաճառք";
                        break;
                    case InvoiceType.ProductOrder:

                        break;
                    case InvoiceType.MoveInvoice:
                        descritpion = "Տեղափոխություն";
                        break;
                    case InvoiceType.InventoryWriteOff:
                        descritpion = "Դուրսգրման ակտ";
                        break;
                    case InvoiceType.ReturnFrom:
                        descritpion = "Ետ վերադարձ";
                        break;
                    case InvoiceType.ReturnTo:
                        descritpion = "Վերադարձ մատակարարին";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return descritpion;
            }
        }

        public int Count
        {
            get
            {
                return ShortInvoiceReports.Count(s => (InvoiceType)s.InvoiceTypeId == InvoiceType);
            }
        }
        public decimal Total
        {
            get
            {
                return ShortInvoiceReports.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType).Sum(s => s.Total);
            }
        }
        public decimal CostPrice { get { return ShortInvoiceReports.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType).Sum(s => s.CostPrice); } }

        public decimal? Profit
        {
            get
            {
                decimal? profit = null;
                switch (InvoiceType)
                {
                    case InvoiceType.PurchaseInvoice:
                        //var salePrice = ShortInvoiceReports.Where(s => (InvoiceType)s.InvoiceTypeId == InvoiceType).Sum(s => s.);
                        break;
                    case InvoiceType.SaleInvoice:
                        profit = Total != 0 ? ((Total - CostPrice) * 100 / (CostPrice!=0?CostPrice:Total)) : (decimal?)null;
                        break;
                    case InvoiceType.ProductOrder:
                        break;
                    case InvoiceType.MoveInvoice:
                        break;
                    case InvoiceType.InventoryWriteOff:
                        break;
                    case InvoiceType.ReturnFrom:
                        break;
                    case InvoiceType.ReturnTo:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return profit;
            }
        }
    }

    public class SaleBy
    {
        public string Description { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    public class MyTime : INotifyPropertyChanged
    {
        public delegate void TimeChangedEvent();

        public event TimeChangedEvent HandleTimeChanged;
        private bool IsTimeFree = false;
        private const string HourProperty = "Hour";
        private const string MinuteProperty = "Minute";
        private const string SecondProperty = "Second";
        private int _hour = 0;
        private int _minute = 0;
        private int _second = 0;

        public int Hour
        {
            get { return _hour; }
            set
            {
                _hour = value < 24 ? value : 0;
                OnPropertyChanged(HourProperty);
                OnTimeChanged();
            }
        }

        public int Minute
        {
            get { return _minute; }
            set
            {
                _minute = value < 60 ? value : 0;
                OnPropertyChanged(MinuteProperty);
                OnTimeChanged();
            }
        }

        public int Second
        {
            get { return _second; }
            set
            {
                _second = value < 60 ? value : 0;
                OnPropertyChanged(SecondProperty);
                OnTimeChanged();
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

        public int GetHour
        {
            get { return Hour; }
        }

        public int GetMinute
        {
            get { return Hour * 60 + Minute; }
        }

        public int GetSeccond
        {
            get { return Hour * 3600 + Minute * 60 + Second; }
        }

        private void OnTimeChanged()
        {
            var handle = HandleTimeChanged;
            if (handle != null) handle();
        }

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

        public void SetTime(TimeSpan timeOfDay)
        {
            Hour = timeOfDay.Hours;
            Minute = timeOfDay.Minutes;
            Second = timeOfDay.Seconds;
        }
    }

    public class CustomDateTime : ViewModelBase
    {
        public delegate void DateTimeChangedEvent();

        public event DateTimeChangedEvent HandleDateTimeChanged;

        private const string HourProperty = "Hour";
        private const string MinuteProperty = "Minute";
        private const string SecondProperty = "Second";


        private int _hour = 0;

        public int Hour
        {
            get { return _hour; }
            set
            {
                _hour = value;
                OnPropertyChanged(HourProperty);
                OnTimeChanged();
            }
        }

        private int _minute = 0;

        public int Minute
        {
            get { return _minute; }
            set
            {
                _minute = value % 60;
                Hour += value / 60;
                OnPropertyChanged(MinuteProperty);
                OnTimeChanged();
            }
        }

        private int _second = 0;

        public int Second
        {
            get { return _second; }
            set
            {
                _second = value % 60;
                Minute += value / 60;
                OnPropertyChanged(SecondProperty);
                OnTimeChanged();
            }
        }

        public CustomDateTime()
        {
            Hour = 0;
            Minute = 0;
            Second = 0;
        }

        public CustomDateTime(int hour = 0, int minute = 0, int second = 0)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        public int GetHour
        {
            get { return Hour; }
        }

        public int GetMinute
        {
            get { return Hour * 60 + Minute; }
        }

        public int GetSeccond
        {
            get { return Hour * 3600 + Minute * 60 + Second; }
        }

        private void OnTimeChanged()
        {
            var handle = HandleDateTimeChanged;
            if (handle != null) handle();
        }

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
