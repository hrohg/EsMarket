using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Data.Model;
using ES.Data.Models;
using ES.Common;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public class PurchaseInvoiceViewModel : InvoiceViewModel
    {
        #region internal methods

        #endregion
        #region External properties
        public override string Title
        {
            get { return string.Format("Գնում{0}", Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty); }
        }
        public override string Description
        {
            get { return string.Format("{0}{1}", Title, Partner != null ? string.Format(" ({0})", Partner.FullName) : string.Empty); }
        }
        public override PartnerModel Partner
        {
            get { return base.Partner; }
            set
            {
                base.Partner = value;
                Invoice.ProviderName = value != null ? value.FullName : null;
                IsModified = true;
                OnPropertyChanged(PartnerProperty);
            }
        }

        #endregion
        public PurchaseInvoiceViewModel(EsUserModel user, EsMemberModel member)
            : base(user, member)
        {
            Initialize();
        }
        public PurchaseInvoiceViewModel(Guid id, EsUserModel user, EsMemberModel member)
            : base(id, user, member)
        {
            Initialize();
        }
        #region Internal methods

        private void Initialize()
        {
            FromStocks = StockManager.GetStocks(Member.Id).ToList();
            Invoice.InvoiceTypeId = (int)InvoiceType.PurchaseInvoice;
            if (Partner == null)
            {
                var provideDefault = ApplicationManager.CashManager.GetEsDefaults(DefaultControls.Provider);
                Partner = provideDefault == null
                    ? ApplicationManager.CashManager.GetPartners.FirstOrDefault()
                    : ApplicationManager.CashManager.GetPartners.FirstOrDefault(s => s.Id == provideDefault.ValueInGuid);
            }

            IsModified = false;
            BuyBySingle = ApplicationManager.BuyBySingle;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        #endregion

        #region External Methods
        public override bool CanAddInvoiceItem(object o)
        {
            return base.CanAddInvoiceItem(o);
        }
        public override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o)) { return; }
            if (!SetQuantity(BuyBySingle)) { return; }
            base.OnAddInvoiceItem(o);
        }
        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
        }
        public override bool CanApprove(object o)
        {
            return base.CanApprove(o)
                && InvoicePaid.IsPaid
                && InvoicePaid.Change <= (InvoicePaid.Paid ?? 0)
                && Partner != null
                && (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - (Partner.Debit ?? 0);
        }
        public override void OnApprove(object o)
        {
            if (!CanApprove(o)) return;
            Invoice.ApproverId = User.UserId;
            Invoice.ApproveDate = DateTime.Now;
            Invoice.Approver = User.FullName;
            Invoice.ProviderName = Partner.FullName;
            if (ToStock == null)
            {
                ToStock = SelectItemsManager.SelectStocks(StockManager.GetStocks(Member.Id), false).FirstOrDefault();
            }
            if (ToStock == null)
            {
                MessageBox.Show("Պահեստ ընտրված չէ: Խնդրում ենք խմբագրել նոր պահեստ:", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            Invoice.ToStockId = ToStock.Id;
            Invoice.RecipientName = ToStock.FullName;
            if (InvoicesManager.ApproveInvoice(Invoice, InvoiceItems.ToList(), InvoicePaid)==null)
            {
                Invoice.ApproveDate = null;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողությունը հնարավոր չէ իրականացնել:", MessageModel.MessageTypeEnum.Warning)); return;
            }
            Invoice = InvoicesManager.GetInvoice(Invoice.Id, Invoice.MemberId);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
            IsModified = false;
        }
        protected override void OnPrintInvoice(PrintSizeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl =
                new PurchaseInvoiceLargeView(new PurchaseInvocieTicketViewModel(Invoice, InvoiceItems.ToList(),
                    StockManager.GetStock(Invoice.ToStockId, Invoice.MemberId), InvoicePaid));
            PrintManager.PrintPreview(ctrl, "Print purchase invoice", true);
        }
        #endregion
    }
}
