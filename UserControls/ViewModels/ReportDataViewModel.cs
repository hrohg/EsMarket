using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using ProductOrderModel = ES.Data.Models.ProductOrderModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels
{
    public class ReportDataViewModel : INotifyPropertyChanged
    {
        #region Constants

        private const string IsInProgressProperty = "IsInProgress";
        #endregion

        #region Internal properties
        private bool _isInProgress;
        #endregion

        #region External properties
        public bool IsInProgress
        {
            get { return _isInProgress; }
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                OnPropertyChanged(IsInProgressProperty);
            }
        }
        public ObservableCollection<object> Items { get; set; }
        #endregion

        #region Constructors

        #endregion

        #region Internal methods
        private void GetProductOrder(object o)
        {
            IsInProgress = true;
            var dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;

            var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2);
            var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Select(s => s.Id));
            var productItems = new ProductsManager().GetProductItems(ApplicationManager.Instance.GetMember.Id);
            var productOrder = new List<object>(productItems.GroupBy(s => s.Product).Select(s =>
                new ProductOrderModel
                {
                    Code = s.First().Product.Code,
                    Description = s.First().Product.Description,
                    Mu = s.First().Product.Mu,
                    MinPrice = s.First().Product.CostPrice,
                    MinQuantity = s.First().Product.MinQuantity,
                    ExistingQuantity = s.Sum(t => t.Quantity),
                    SaleQuantity = invoiceItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity ?? 0),
                    Notes = s.First().Product.Note
                }).ToList());

            OnPropertyChanged("Items");
            IsInProgress = false;
        }
        private void OnGetProductOrder(object o)
        {
            ThreadStart ts = delegate { GetProductOrder(o); };
            Thread myThread = new Thread(ts);
            myThread.Start();
        }
        #endregion

        #region Commands
        public ICommand ProductOrderCommand { get { return new RelayCommand(OnGetProductOrder); } }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
    public class ReportBaseViewModel : TableViewModel<IInvoiceReport>
    {
        #region Event
        public delegate List<InvoiceReport> UpdateDelegate(Tuple<DateTime, DateTime> dateIntermediate);
        public event UpdateDelegate OnUpdate;
        #endregion Event
        #region Internal properties

        #endregion

        #region External properties
        private List<IInvoiceReport> _items = new List<IInvoiceReport>();
        public override ObservableCollection<IInvoiceReport> ViewList
        {
            get
            {
                return new ObservableCollection<IInvoiceReport>(_items);
            }
            protected set
            {
                _items = value.ToList();

                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Total");
            }
        }
        #endregion

        #region Constructors
        public ReportBaseViewModel()
            : base()
        {
            Title = Description = "Հաշվետվություն";
            IsShowUpdateButton = true;
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
        }

        private void OnUpdateAsync(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            RaisePropertyChanged(IsInProgressProperty);
            var handle = OnUpdate;
            if (handle == null) return;
            var reports = handle(dateIntermediate);
            if (reports == null || reports.Count == 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    MessageManager.OnMessage
                    (new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information))));
                return;
            }
            ViewList = new ObservableCollection<IInvoiceReport>(reports);
            TotalRows = reports.Count;
            //TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }

        public void Update()
        {
            base.OnUpdate(null);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");
            var thread = new Thread(() => OnUpdateAsync(dateIntermediate));
            thread.Start();
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null)
            {
                return;
            }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }

        #endregion
    }

    public class InvoiceReportViewModel : TableViewModel<IInvoiceReport>
    {

        #region Internal properties
        private List<IInvoiceReport> _items = new List<IInvoiceReport>();
        private List<InvoiceType> _invoiceTypes;
        #endregion

        #region External properties
        public override ObservableCollection<IInvoiceReport> ViewList
        {
            get
            {
                return new ObservableCollection<IInvoiceReport>(_items);
            }
            protected set
            {
                _items = value.ToList();

                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Total");
            }
        }
        #endregion

        #region Constructors
        public InvoiceReportViewModel(List<InvoiceType> invoiceTypes)
            : base()
        {
            _invoiceTypes = invoiceTypes;
            IsShowUpdateButton = true;
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Հաշվետվություն ըստ ապրանքագրերի";
            OnUpdate(null);
        }

        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            RaisePropertyChanged(IsInProgressProperty);

            var reports = InvoicesManager.GetInvoicesReports(dateIntermediate.Item1, dateIntermediate.Item2, _invoiceTypes);
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                return;
            }
            ViewList = new ObservableCollection<IInvoiceReport>(reports);
            TotalRows = reports.Count;
            //TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }

        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն ըստ ապրանքագրերի {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");
            var thread = new Thread(() => Update(dateIntermediate));
            thread.Start();
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null)
            {
                return;
            }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }

        #endregion
    }

    public class SaleInvoiceReportByStocksViewModel : SaleInvoiceReportByPartnerViewModel
    {
        public SaleInvoiceReportByStocksViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
        }
    }

    public class SaleInvoiceReportByPartnerViewModel : TableViewModel<IInvoiceReport>
    {
        #region Internal properties
        private List<IInvoiceReport> _items = new List<IInvoiceReport>();
        private ViewInvoicesEnum _viewInvoicesEnum;
        #endregion

        #region External properties
        public override ObservableCollection<IInvoiceReport> ViewList
        {
            get
            {
                return new ObservableCollection<IInvoiceReport>(_items);
            }
            protected set
            {
                _items = value.ToList();
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Total");
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceReportByPartnerViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base()
        {
            _viewInvoicesEnum = viewInvoicesEnum;
            IsShowUpdateButton = true;
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Վաճառք ըստ պատվիրատուների";
            OnUpdate(null);
        }

        protected virtual void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            RaisePropertyChanged(IsInProgressProperty);
            List<IInvoiceReport> reports = null;
            List<PartnerModel> partners = null;
            List<PartnerType> partnerTypes = null;
            List<StockModel> stocks = null;
            switch (_viewInvoicesEnum)
            {
                case ViewInvoicesEnum.None:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.CashManager.GetStocks, true).ToList(); }));
                    reports = new List<IInvoiceReport>(UpdateByStock(stocks, dateIntermediate));
                    break;
                case ViewInvoicesEnum.ByPartner:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartners(dateIntermediate.Item1, dateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partnerTypes = SelectItemsManager.SelectPartnersTypes(true);
                    }));

                    if (partnerTypes != null && partnerTypes.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnerTypes(dateIntermediate.Item1, dateIntermediate.Item2, InvoiceType.SaleInvoice, partnerTypes, ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnersDetiled(dateIntermediate.Item1, dateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList()));
                    }
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.CashManager.GetStocks, true).ToList(); }));

                    var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).Select(s => s.Id).ToList(), stocks);

                    reports = new List<IInvoiceReport>(invoiceItems.Select(s =>
                        new SaleReportByPartnerDetiled
                        {
                            Partner = s.Invoice.ProviderName,
                            Invoice = s.Invoice.InvoiceNumber,
                            Date = s.Invoice.CreateDate,
                            Code = s.Code,
                            Description = s.Description,
                            Mu = s.Mu,
                            Quantity = s.Quantity ?? 0,
                            Cost = s.CostPrice,
                            Price = s.Price ?? 0,
                            Sale = (s.Quantity??0)*(s.Price??0),
                            Note = s.Invoice.Notes
                        }).ToList());
                    break;
                case ViewInvoicesEnum.BySaleChart:
                    var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2)
                            .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).ToList();
                    if (!invoices.Any())
                    {
                        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                        break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                ViewList = new ObservableCollection<IInvoiceReport>(reports);
            }
            if (reports != null)
            {
                TotalRows = reports.Count;
                TotalCount = (double)reports.Sum(s => s.Quantity);
                Total = (double)reports.Sum(i => i.Sale ?? 0);
            }
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);

        }

        private void InitializeReport(List<InvoiceReport> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                ViewList = new ObservableCollection<IInvoiceReport>(reports);
            }
            if (reports != null)
            {
                TotalRows = reports.Count;
                TotalCount = (double)reports.Sum(s => s.Quantity);
                Total = (double)reports.Sum(i => i.Sale ?? 0);
            }
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("{0} {1} - {2}", Title, dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");
            var thread = new Thread(() => Update(dateIntermediate));
            thread.Start();
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null)
            {
                return;
            }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }

        private List<InvoiceReport> UpdateByStock(List<StockModel> stocks, Tuple<DateTime, DateTime> dateIntermediate)
        {
            var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice || s.InvoiceTypeId == (long)InvoiceType.ReturnFrom).Select(s => s.Id).ToList(), stocks);
            var list = stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii=>ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Count(ii => ii.ProductItem.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.CostPrice ?? 0)),
                Sale = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId==(int)InvoiceType.ReturnFrom)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0))
            }).ToList();
            list.Add(new InvoiceReport
            {
                Description = "Ընդամենը",
                Count = list.Sum(s => s.Count),
                Quantity = list.Sum(s => s.Quantity),
                Cost = list.Sum(s => s.Cost),
                ReturnAmount = list.Sum(s=>s.ReturnAmount),
                Sale = list.Sum(s => s.Sale)
            });
            return list;
        }

        #endregion
    }

    public class SaleInvoiceReportByPartnersDetiledViewModel : SaleInvoiceReportByPartnerViewModel
    {
        #region Constructors
        public SaleInvoiceReportByPartnersDetiledViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Վաճառք ըստ պատվիրատուների մանրամասն";
        }
        #endregion
    }

    public class SaleInvoiceReportByStockDetiledViewModel : SaleInvoiceReportByPartnerViewModel
    {
        #region Constructors
        public SaleInvoiceReportByStockDetiledViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Վաճառք ըստ բաժինների մանրամասն";
        }

        protected override void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            base.Update(dateIntermediate);
        }

        #endregion
    }

    public class SaleInvoiceReportByChartViewModel : SaleInvoiceReportByPartnerViewModel
    {
        private ItemProperty _reportChartType;
        private readonly object _sync = new object();
        private List<IInvoiceReport> _invoiceReports;

        #region Internal properties
        #endregion Internal properties

        #region External proeprties
        public ObservableCollection<KeyValuePair<object, decimal>> ChartDatas { get; set; }
        public List<InvoiceModel> Invoices { get; set; }
        public List<ItemProperty> ReportChartTypes { get; set; }

        public ItemProperty ReportChartType
        {
            get { return _reportChartType; }
            set
            {
                _reportChartType = value; RaisePropertyChanged("ReportChartType");
                UpdateChartData();
            }
        }

        #endregion External properties

        #region Constructors
        public SaleInvoiceReportByChartViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Վաճառքի վերլուծություն";
            Invoices = new List<InvoiceModel>();
            ChartDatas = new ObservableCollection<KeyValuePair<object, decimal>>();
            ReportChartTypes = new List<ItemProperty> { 
                new ItemProperty { Value = "Ժամվա", Key = 0 }, 
                new ItemProperty { Value = "Օրվա", Key = 1 }, 
                new ItemProperty { Value = "Շաբաթվա", Key = 2 }, 
                new ItemProperty { Value = "Ամսվա", Key = 3 }, 
                new ItemProperty { Value = "Տարվա", Key = 4 } };
            ReportChartType = ReportChartTypes.First();
        }

        protected void UpdateChartData()
        {
            lock (_sync)
            {
                if (ReportChartType == null)
                {
                    return;
                }
                decimal summ;
                int count;
                switch ((int)ReportChartType.Key)
                {
                    //ByHour
                    case 0:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByHour = _invoiceReports.OrderBy(s => s.Date.Hour).GroupBy(s => s.Date.Hour).ToList();
                        foreach (var invoice in invociesGroupByHour)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s=>s.Date.Day).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.ToString("T"), summ / count));
                        }
                        break;
                    //ByDay
                    case 1:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByDay = _invoiceReports.OrderBy(s => s.Date.Day).GroupBy(s => s.Date.Day).ToList();

                        foreach (var invoice in invociesGroupByDay)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s=>s.Date.Month).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Day, summ / count));
                        }
                        break;
                    //ByWeek
                    case 2:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByWeek = _invoiceReports.OrderBy(s => s.Date.DayOfWeek).GroupBy(s => s.Date.DayOfWeek).ToList();

                        foreach (var invoice in invociesGroupByWeek)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s=>s.Date.Day).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.ToString("ddd"), summ / count));
                        }
                        break;
                    //ByMonth
                    case 3:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByMonth = _invoiceReports.OrderBy(s => s.Date.Month).GroupBy(s => s.Date.Month).ToList();
                        foreach (var invoice in invociesGroupByMonth)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s=>s.Date.Year).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Month, summ/count));
                        }
                        break;
                    //ByYear
                    case 4:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByYear = _invoiceReports.OrderBy(s => s.Date.Year).GroupBy(s => s.Date.Year);

                        foreach (var invoice in invociesGroupByYear)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Year, summ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            //base.Update(dateIntermediate);
            lock (_sync)
            {
                _invoiceReports = InvoicesManager.GetInvoiceReports(dateIntermediate.Item1, dateIntermediate.Item2, new List<InvoiceType>() { InvoiceType.SaleInvoice });
                UpdateChartData();
            }
        }

        #endregion
    }

    
}
