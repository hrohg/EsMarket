using System;
using System.Linq;
using ES.Business.Managers;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets.Views;

namespace UserControls.ViewModels.Invoices
{
    public class PackingListForSallerViewModel : InvoiceViewModelBase
    {

        #region External properties
        public override bool IsModified
        {
            get { return false; }
        }
        public override StockModel FromStock
        {
            get { return base.FromStock; }
            set
            {
                base.FromStock = value;
                //Invoice.ProviderName = value != null ? value.FullName : string.Empty;
            }
        }
        public override StockModel ToStock
        {
            get { return base.ToStock; }
            set
            {
                base.ToStock = value;
                //Invoice.RecipientName = value != null ? value.FullName : string.Empty;
            }
        }
        #endregion External properties

        #region Constructors

        public PackingListForSallerViewModel() : base()
        {
            Initialize();
        }
        public PackingListForSallerViewModel(Guid invoiceId)
            : base(invoiceId)
        {
            Initialize();
        }
        #endregion

        #region External methods
        protected override void OnExportInvoice(ExportImportEnum exportTo)
        {
            switch (exportTo)
            {
                case ExportImportEnum.AccDocXml:
                    break;
                case ExportImportEnum.Xml:
                    InvoicesManager.ExportShippingProductsInvoiceToXml(Invoice, InvoiceItems.ToList());
                    break;
                case ExportImportEnum.Excel:
                    base.OnExportInvoice(exportTo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportTo", exportTo, null);
            }
        }
        #endregion External methods

        #region Internal methods
        private void Initialize()
        {
            Title = string.Format("Ապրանքների ցուցակ {0}", IsInvoiceValid && Invoice.InvoiceNumber != null ? Invoice.InvoiceNumber : string.Empty);
            IsModified = true;
            Description = string.Format("{0} {1}", Title, IsInvoiceValid ? (Partner != null ? Partner.FullName : FromStock != null ? FromStock.FullName : string.Empty) : string.Empty);
        }

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new InvoicePreview(this);
            PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);

        }
        #endregion Internal methods
    }

    public class PackingListViewModel : InvoiceViewModel
    {
        #region Constructors
        public PackingListViewModel() : base(InvoiceType.ProductOrder) { }
        public PackingListViewModel(Guid id) : base(id) { }
        #endregion Constructors
        protected override void OnInitialize() { }

        protected override void OnApprove(object o)
        {

        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            OnApprove(null);
        }
    }
    public class ViewMoveInvoiceViewModel : InvoiceViewModelBase
    {

        #region External properties

        public override bool IsModified
        {
            get { return false; }
        }
        #endregion External properties

        #region Constructors

        public ViewMoveInvoiceViewModel(Guid invoiceId) : base(invoiceId)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = string.Format("Տեղափոխման ապրանքագիր {0}", Invoice.InvoiceNumber != null ? Invoice.InvoiceNumber : string.Empty);
            IsModified = false;
            Description = string.Format("{0} {1} -> {2}", Title, Invoice.ProviderName, Invoice.RecipientName);
        }

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();

            var ctrl = new InvoicePreview((InvoiceViewModelBase)this.MemberwiseClone());
            PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);

        }
        #endregion Internal methods
    }
}
