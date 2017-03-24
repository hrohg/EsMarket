using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using ProductOrderModel = ES.Data.Models.ProductOrderModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels
{
    public class ReportViewModel : INotifyPropertyChanged
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
            var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2, ApplicationManager.Instance.GetEsMember.Id);
            var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Select(s => s.Id));
            var productItems = new ProductsManager().GetProductItems(ApplicationManager.Instance.GetEsMember.Id);
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
                    Note = s.First().Product.Note
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

                OnPropertyChanged("ViewList");
                OnPropertyChanged("Count");
                OnPropertyChanged("Total");
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
            OnPropertyChanged(IsInProgressProperty);
            var handle = OnUpdate;
            if (handle == null) return;
            var reports = handle(dateIntermediate);
            if (reports == null || reports.Count == 0)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(()=>
                    ApplicationManager.MessageManager.OnNewMessage
                    (new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information))));
                return;
            }
            ViewList = new ObservableCollection<IInvoiceReport>(reports);
            TotalRows = reports.Count;
            //TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
            IsLoading = false;
            OnPropertyChanged(IsInProgressProperty);
        }

        public void Update()
        {
            base.OnUpdate(null);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            OnPropertyChanged("Description");
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

                OnPropertyChanged("ViewList");
                OnPropertyChanged("Count");
                OnPropertyChanged("Total");
            }
        }
        #endregion

        #region Constructors
        public InvoiceReportViewModel(List<InvoiceType> invoiceTypes)
            : base()
        {
            _invoiceTypes = invoiceTypes;
            Title = Description = "Հաշվետվություն ըստ ապրանքագրերի";
            IsShowUpdateButton = true;
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            OnUpdate(null);
        }

        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            OnPropertyChanged(IsInProgressProperty);

            var reports = InvoicesManager.GetInvoicesReports(dateIntermediate.Item1, dateIntermediate.Item2, _invoiceTypes, ApplicationManager.Instance.GetEsMember.Id);
            if (reports == null || reports.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                return;
            }
            ViewList = new ObservableCollection<IInvoiceReport>(reports);
            TotalRows = reports.Count;
            //TotalCount = (double)_items.Sum(s => s.Quantity ?? 0);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
            IsLoading = false;
            OnPropertyChanged(IsInProgressProperty);
        }

        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն ըստ ապրանքագրերի {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            OnPropertyChanged("Description");
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
                OnPropertyChanged("ViewList");
                OnPropertyChanged("Count");
                OnPropertyChanged("Total");
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceReportByPartnerViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base()
        {
            _viewInvoicesEnum = viewInvoicesEnum;
            Title = Description = "Հաշվետվություն ըստ ապրանքագրերի";
            IsShowUpdateButton = true;
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            OnUpdate(null);
        }

        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            OnPropertyChanged(IsInProgressProperty);
            List<InvoiceReportByPartner> reports = null;
            switch (_viewInvoicesEnum)
            {
                case ViewInvoicesEnum.None:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByPartner:
                    List<PartnerModel> partners = null;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = InvoicesManager.GetSaleInvoicesReportsByPartners(dateIntermediate.Item1, dateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ApplicationManager.Instance.GetEsMember.Id);
                    }
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                    List<PartnerType> partnerTypes = null;
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partnerTypes = SelectItemsManager.SelectPartnersTypes(true);
                    }));

                    if (partnerTypes != null && partnerTypes.Count > 0)
                    {
                        reports = InvoicesManager.GetSaleInvoicesReportsByPartnerTypes(dateIntermediate.Item1, dateIntermediate.Item2, InvoiceType.SaleInvoice, partnerTypes, ApplicationManager.Instance.GetEsMember.Id);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (reports == null || reports.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
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
            OnPropertyChanged(IsInProgressProperty);

        }

        private void InitializeReport(List<InvoiceReport> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
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
            OnPropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն ըստ ապրանքագրերի {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            OnPropertyChanged("Description");
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
}
