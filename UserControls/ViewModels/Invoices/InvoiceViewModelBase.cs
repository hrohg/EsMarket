using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Interfaces;

namespace UserControls.ViewModels.Invoices
{
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public abstract class InvoiceViewModelBase : DocumentViewModel, IInvoiceViewModel
    {
        #region Internal fields

        #endregion Internal fields

        #region Internal properties

        protected EsMemberModel Member
        {
            get { return ApplicationManager.Instance.GetMember; }
        }

        protected EsUserModel User
        {
            get { return ApplicationManager.GetEsUser; }
        }

        protected bool IsInvoiceValid
        {
            get { return Invoice != null && InvoiceItems != null && InvoiceItems.Count > 0; }
        }

        #endregion Internal properties

        #region External properties

        #region Invoice

        private const string InvoiceProperty = "Invoice";
        private InvoiceModel _invoice;

        public InvoiceModel Invoice
        {
            get { return _invoice; }
            set
            {
                if (value == null) return;
                _invoice = value;
                RaisePropertyChanged(InvoiceProperty);
                FromStock = _invoice.FromStockId != null ? StockManager.GetStock(_invoice.FromStockId) : null;
                ToStock = _invoice.ToStockId != null ? StockManager.GetStock(_invoice.ToStockId) : null;
                Partner = Invoice.Partner ?? PartnersManager.GetPartner(Invoice.PartnerId);

                var invoiceItems = InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index);
                InvoiceItems = new ObservableCollection<InvoiceItemsModel>();
                InvoiceItems.CollectionChanged += OnInvoiceItemsChanged;
                foreach (var invoiceItemsModel in invoiceItems)
                {
                    InvoiceItems.Add(invoiceItemsModel);
                }
                IsModified = true;
            }
        }

        private ObservableCollection<InvoiceItemsModel> _invoiceItems = new ObservableCollection<InvoiceItemsModel>();

        public ObservableCollection<InvoiceItemsModel> InvoiceItems
        {
            get { return _invoiceItems; }
            set
            {
                _invoiceItems = value;
                RaisePropertyChanged("InvoiceItems");
            }
        }


        #endregion Invoice

        #region Filter

        private const string FilteredInvoiceItemsProperty = "FilteredInvoiceItems";

        [XmlIgnore]
        public string Filter { get; set; }

        [XmlIgnore]
        public ObservableCollection<InvoiceItemsModel> FilteredInvoiceItems
        {
            get
            {
                return
                    new ObservableCollection<InvoiceItemsModel>(InvoiceItems.Where(s => string.IsNullOrEmpty(Filter) ||
                                                                                        s.Code.Contains(Filter) ||
                                                                                        s.Description.Contains(Filter) ||
                                                                                        s.Price.ToString()
                                                                                            .Contains(Filter)));
            }
        }

        #endregion Filter

        #region Stocks

        private const string FromStockProperty = "FromStock";
        protected List<StockModel> FromStocks { get; set; }
        private StockModel _fromStock;

        public StockModel FromStock
        {
            get { return _fromStock; }
            set
            {
                _fromStock = value;
                Invoice.FromStockId = value != null ? value.Id : (long?)null;
                Invoice.ProviderName = value != null ? value.FullName : string.Empty;
                if (value != null) FromStocks = new List<StockModel> { value };
                RaisePropertyChanged(FromStockProperty);
                RaisePropertyChanged("Description");
                IsModified = true;
            }
        }

        private const string ToStockProperty = "ToStock";
        private StockModel _toStock;

        public StockModel ToStock
        {
            get { return _toStock; }
            set
            {
                _toStock = value;
                Invoice.ToStockId = value != null ? value.Id : (long?)null;
                Invoice.RecipientName = value != null ? value.FullName : string.Empty;
                RaisePropertyChanged(ToStockProperty);
                RaisePropertyChanged("Description");
                IsModified = true;
            }
        }

        #endregion Stocks

        #region Partner

        protected const string PartnerProperty = "Partner";
        private PartnerModel _partner;

        public virtual PartnerModel Partner
        {
            get { return _partner; }
            set
            {
                _partner = value;
                Invoice.PartnerId = value != null ? value.Id : (Guid?)null;
                IsModified = true;
                RaisePropertyChanged(PartnerProperty);
            }
        }

        #endregion Partner

        #endregion External properties

        public override string Description
        {
            get
            {
                return string.Format("Տեղափոխություն ({0} -> {1})", FromStock != null ? FromStock.Name : string.Empty,
                    ToStock != null ? ToStock.Name : string.Empty);
            }
        }

        #region Constructors

        public InvoiceViewModelBase()
        {
            Initialize();
        }

        public InvoiceViewModelBase(Guid id)
            : this()
        {
            Invoice = InvoicesManager.GetInvoice(id);
            Initialize();
        }

        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքագիր";
            if (Invoice == null)
                Invoice = new InvoiceModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetMember);
            PrintInvoiceCommand = new RelayCommand<PrintModeEnum>(OnPrintInvoice, CanPrintInvoice);
            ExportInvoiceCommand = new RelayCommand<ExportImportEnum>(OnExportInvoice, CanExportInvoice);

        }

        protected virtual void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (InvoiceItemsModel item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= OnInvoiceItemsPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (InvoiceItemsModel item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += OnInvoiceItemsPropertyChanged;
                }
            }
            OnInvoiceItemsPropertyChanged(null, null);
        }

        protected virtual void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Invoice.Amount = InvoiceItems.Sum(s => (s.Product.Price ?? 0) * (s.Quantity ?? 0));
            IsModified = true;
            RaisePropertyChanged(FilteredInvoiceItemsProperty);
        }

        #endregion Internal methods

        #region External methods

        #endregion External methods

        #region Command methods

        #region Print invoice

        protected virtual bool CanPrintInvoice(PrintModeEnum printSize)
        {
            return IsInvoiceValid;
        }

        protected virtual void OnPrintInvoice(PrintModeEnum printSize)
        {
            //if (!CanPrintInvoice(printSize)) { return; }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            //var ctrl = new InvoicePreview(new ReceiptTicketViewModel(new ResponseReceiptModel()));
            //PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);

        }

        #endregion

       

        #region Export

        protected virtual bool CanExportInvoice(ExportImportEnum exportTo)
        {
            return IsInvoiceValid;
        }

        protected virtual void OnExportInvoice(ExportImportEnum exportTo)
        {
            switch (exportTo)
            {
                case ExportImportEnum.AccDocXml:
                    break;
                case ExportImportEnum.Xml:
                    InvoicesManager.ExportInvoiceToXml(Invoice, InvoiceItems.ToList());
                    break;
                case ExportImportEnum.Excel:
                    ExcelExportManager.ExportInvoice(Invoice, InvoiceItems);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportTo", exportTo, null);
            }

        }

        #endregion Export

        #endregion Command methods

        #region Commands

        [XmlIgnore]
        public ICommand PrintInvoiceCommand { get; private set; }

        [XmlIgnore]
        public ICommand ExportInvoiceCommand { get; private set; }

        #endregion Commands
    }

}
