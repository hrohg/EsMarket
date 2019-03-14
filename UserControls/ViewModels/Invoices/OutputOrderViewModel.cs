using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Models;
using ES.DataAccess.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;

namespace UserControls.ViewModels.Invoices
{
    public class OutputOrderViewModel : InvoiceViewModel
    {
        #region External properties
        public override bool AddBySingle
        {
            get { return base.AddBySingle; }
            set
            {
                base.AddBySingle = value;
                ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle = value;
            }
        }

        #endregion External properties

        #region Contructors
        protected OutputOrderViewModel(InvoiceType type)
            : base(type)
        {
        }

        protected OutputOrderViewModel(Guid id)
            : base(id)
        {
        }
        #endregion Constructors

        #region Internal methods
        protected override void OnInitialize()
        {
            base.OnInitialize();
            FromStocks = StockManager.GetStocks(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSaleStocks.ToList()).ToList();
            AddBySingle = ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle;
        }

        protected override void SetPrice()
        {
            foreach (var item in InvoiceItems)
            {
                item.Price = GetProductPrice(item.Product);
            }
            RaisePropertyChanged("InvoiceItems");
            if (InvoiceItem != null && InvoiceItem.Product != null)
            {
                InvoiceItem.Price = InvoiceItem.Product.GetProductPrice();
            }
            RaisePropertyChanged("InvoiceItem");
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageManager.ShowMessage(message, "Անբավարար միջոցներ");
                return false;
            }

            if ((InvoiceItem.Quantity == null || InvoiceItem.Quantity == 0))
            {
                if (addSingle && exCount >= 1)
                {
                    InvoiceItem.Quantity = 1;
                }
                else
                {
                    InvoiceItem.Quantity = GetAddedItemCount(exCount, false);
                }
            }

            return InvoiceItem.Quantity != null && InvoiceItem.Quantity > 0;
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            lock (_sync)
            {
                if (!CanApprove(null))
                {
                    MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                    return;
                }

                IsLoading = true;
                PrepareToApprove();
            }
        }
        protected virtual void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;

            //Paid
            CashDesk cashDesk = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                cashDesk = InvoicePaid.Paid > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks).SingleOrDefault() : null;
            }));
            InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;

            CashDesk bankAccount = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                bankAccount = InvoicePaid.ByCheck > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts).SingleOrDefault() : null;
            }));

            InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
        }
        protected override void PreviewAddInvoiceItem(object o)
        {
            if (!SetQuantity(AddBySingle))
            {
                //return;
            }
            base.PreviewAddInvoiceItem(o);
        }

        protected override InvoiceItemsModel GetInvoiceItem(string code)
        {
            var invoiceItem = base.GetInvoiceItem(code);
            invoiceItem.Price = GetProductPrice(invoiceItem.Product);
            return invoiceItem;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            PreviewAddInvoiceItem(o);
        }

        protected override void OnExportInvoice(ExportImportEnum exportTo)
        {
            switch (exportTo)
            {
                case ExportImportEnum.AccDocXml:
                    InvoicesManager.ExportInvoiceToXmlAccDoc(Invoice, InvoiceItems.ToList());
                    break;
                case ExportImportEnum.Xml:
                case ExportImportEnum.Excel:
                    base.OnExportInvoice(exportTo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportTo", exportTo, null);
            }
        }
        protected override void OnImportInvoice(ExportImportEnum importFrom)
        {
            switch (importFrom)
            {
                case ExportImportEnum.Xml:
                    ImportFromXml();
                    break;
                case ExportImportEnum.AccDocXml:
                    break;
                case ExportImportEnum.Excel:
                    ImportFromExcel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("importFrom", importFrom, null);
            }
        }

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize))
            {
                return;
            }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new SaleInvoiceView(new SaleInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId), InvoicePaid, Member));

            switch (printSize)
            {
                case PrintModeEnum.Normal:
                    PrintManager.PrintPreview(ctrl, "Print invoices", true);
                    break;
                case PrintModeEnum.Small:
                case PrintModeEnum.Large:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printSize", printSize, null);
            }
        }

        private bool CheckBeforeApprove()   
        {
            if (InvoiceItems.Any(s => s.Quantity == 0))
            {
                MessageBox.Show("0-յական քանակով ելքագրում արտոնված չէ։ Ստուգեք ելքագրվող ապրանքների քանակը։",
                    "Չարտոնված գործողություն", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        protected override void OnApprove(object o)
        {
            //Approve Invoice
            if (CheckBeforeApprove()) new Thread(() => OnApproveAsync(false)).Start();
        }

        #endregion Internal methods

        #region External methods
        public override void OnApproveAndClose(object o)
        {
            //Approve Invoice
            if (CheckBeforeApprove()) new Thread(() => OnApproveAsync(true)).Start();
        }
        #endregion External methods

        #region Commands

        private ICommand _checkExistingCommand;
        public ICommand CheckExistingCommand
        {
            get { return _checkExistingCommand ?? (_checkExistingCommand = new RelayCommand(OnCheckExisting, CanCheckExisting)); }
        }

        private bool CanCheckExisting(object obj)
        {
            return CanApprove(null);
        }

        private void OnCheckExisting(object obj)
        {
            InvoicesManager.CheckForQuantityExisting(InvoiceItems.ToList(), FromStocks.Select(s => s.Id));
        }

        #endregion Commands
    }
}
