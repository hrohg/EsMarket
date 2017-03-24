using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.Views.ReceiptTickets.Views;

namespace UserControls.ViewModels.Invoices
{
    public class InvoiceViewModelBase : DocumentViewModel, IInvoiceViewModel
    {
        #region Internal fields
        #endregion Internal fields

        #region Internal properties
        protected EsMemberModel Member { get { return ApplicationManager.Instance.GetEsMember; } }
        protected EsUserModel User { get { return ApplicationManager.GetEsUser; } }

        protected bool IsInvocieValid
        {
            get
            {
                return Invoice != null && InvoiceItems != null && InvoiceItems.Count > 0;
            }
        }
        protected bool IsInvocieAppoved
        {
            get { return Invoice != null && Invoice.ApproveDate!=null; }
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
                FromStock = _invoice.FromStockId != null ? StockManager.GetStock(_invoice.FromStockId, _invoice.MemberId) : null;
                ToStock = _invoice.ToStockId != null ? StockManager.GetStock(_invoice.ToStockId, _invoice.MemberId) : null;
                Partner = Invoice.Partner ?? PartnersManager.GetPartner(Invoice.PartnerId, Invoice.MemberId);
                var invoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
                InvoiceItems = new ObservableCollection<InvoiceItemsModel>();
                _invoiceItems.CollectionChanged += OnInvoiceItemsChanged;
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
        public string Filter { get; set; }
        public ObservableCollection<InvoiceItemsModel> FilteredInvoiceItems
        {
            get
            {
                return new ObservableCollection<InvoiceItemsModel>(InvoiceItems.Where(s => string.IsNullOrEmpty(Filter) ||
                    s.Code.Contains(Filter) ||
                    s.Description.Contains(Filter) ||
                    s.Price.ToString().Contains(Filter)));
            }
        }
        #endregion Filter

        #region Stocks
        private const string FromStockProperty = "FromStock";
        protected List<StockModel> FromStocks { get; set; }
        private StockModel _fromStock;
        public StockModel FromStock
        {
            get
            {
                return _fromStock;
            }
            set
            {
                _fromStock = value;
                Invoice.FromStockId = value != null ? value.Id : (long?)null;
                Invoice.ProviderName = value != null ? value.FullName : string.Empty;
                if (value != null) FromStocks = new List<StockModel> { value };
                RaisePropertyChanged(FromStockProperty);
                IsModified = true;
            }
        }

        private const string ToStockProperty = "ToStock";
        private StockModel _toStock;
        public StockModel ToStock
        {
            get
            {
                return _toStock;
            }
            set
            {
                _toStock = value;
                Invoice.ToStockId = value != null ? value.Id : (long?)null;
                Invoice.RecipientName = value != null ? value.FullName : string.Empty;
                RaisePropertyChanged(ToStockProperty);
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

        #region Constructors
        public InvoiceViewModelBase()
        {
            Initialize();
        }
        public InvoiceViewModelBase(Guid id)
            : this()
        {
            Invoice = InvoicesManager.GetInvoice(id, ApplicationManager.Instance.GetEsMember.Id);
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            Title = "Ապրանքագիր";
            if (Invoice == null) Invoice = new InvoiceModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
            PrintInvoiceCommand = new RelayCommand<PrintModeEnum>(OnPrintInvoice, CanPrintInvoice);
            ImportInvoiceCommand = new RelayCommand<ExportInportEnum>(OnImportInvoice, CanImportInvoice);
            ExportInvoiceCommand = new RelayCommand<ExportInportEnum>(OnImportInvoice, CanImportInvoice);
        }
        protected virtual void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (InvoiceItemsModel item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= OnInvoiceItemPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (InvoiceItemsModel item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += OnInvoiceItemPropertyChanged;
                }
            }
            OnInvoiceItemPropertyChanged(null, null);
        }
        protected virtual void OnInvoiceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
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
            return IsInvocieValid;
        }
        protected virtual void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            //var ctrl = new InvoicePreview(new ReceiptTicketViewModel(new ResponseReceiptModel()));
            //PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);

        }
        #endregion

        #region Import
        protected virtual bool CanImportInvoice(ExportInportEnum importFrom)
        {
            return IsInvocieValid && !IsInvocieAppoved;
        }
        protected virtual void OnImportInvoice(ExportInportEnum importFrom)
        {
            var filePath = FileManager.OpenExcelFile();
            if (File.Exists(filePath))
            {
                var invoiceObject = ExcelImportManager.ImportInvoice(filePath);
                if (invoiceObject == null) return;
                var importInvoice = invoiceObject.Item1 as InvoiceModel;
                var importInvoiceItems = invoiceObject.Item2 as List<InvoiceItemsModel>;
                if (importInvoice == null || importInvoiceItems == null) return;
                Invoice = new InvoiceModel(User, Member, Invoice.InvoiceTypeId);
                Invoice.CreateDate = importInvoice.CreateDate;
                if (Invoice.CreateDate == DateTime.MinValue) Invoice.CreateDate = DateTime.Now;
                InvoiceItems.Clear();
                foreach (var invoiceItem in importInvoiceItems)
                {
                    if (invoiceItem == null || string.IsNullOrEmpty(invoiceItem.Code)) return;
                    var product = new ProductsManager().GetProductsByCodeOrBarcode(invoiceItem.Code, Member.Id);
                    if (product == null)
                    {
                        MessageBox.Show(invoiceItem.Code +
                            " կոդով ապրանք չի հայտնաբերվել։ Գործողությունն ընդհատված է։ Փորձեք կրկին։",
                            "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    invoiceItem.InvoiceId = Invoice.Id;
                    invoiceItem.ProductId = product.Id;
                    invoiceItem.Description = product.Description;
                    invoiceItem.Mu = product.Mu;
                    invoiceItem.Note += product.Note;
                    invoiceItem.Product = product;
                    InvoiceItems.Add(invoiceItem);
                }
            }
        }
        #endregion Import

        #region Export
        protected virtual bool CanExportInvoice(ExportInportEnum exportTo)
        {
            return IsInvocieValid;
        }
        protected virtual void ExportPrintInvoice(ExportInportEnum exportTo)
        {
            ExcelExportManager.ExportInvoice(Invoice, InvoiceItems);
        }
        #endregion Export

        #endregion Command methods

        #region Commands
        public ICommand PrintInvoiceCommand { get; private set; }
        public ICommand ImportInvoiceCommand { get; private set; }
        public ICommand ExportInvoiceCommand { get; private set; }
        #endregion Commands
    }
}
