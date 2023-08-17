using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using ProductModel = ES.Data.Models.Products.ProductModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public class PurchaseInvoiceViewModel : InvoiceViewModel
    {
        #region Internal properties

        #endregion

        #region External properties
        public override string Description
        {
            get { return string.Format("{0}{1}", Title, Partner != null ? string.Format(" ({0})", Partner.FullName) : string.Empty); }
        }

        protected override void OnPartnerChanged()
        {
            base.OnPartnerChanged();
            Invoice.ProviderName = Partner != null ? Partner.FullName : null;
        }
        public override StockModel FromStock
        {
            get { return base.FromStock; }
            set
            {
                base.FromStock = value;                
            }
        }
        public override StockModel ToStock
        {
            get { return base.ToStock; }
            set
            {
                base.ToStock = value;
                Invoice.RecipientName = value != null ? value.FullName : string.Empty;
            }
        }
        public override bool AddBySingle
        {
            get { return base.AddBySingle; }
            set
            {
                base.AddBySingle = value;
                ApplicationManager.Settings.SettingsContainer.MemberSettings.PurchaseBySingle = value;
            }
        }
        #endregion

        public PurchaseInvoiceViewModel()
            : base(InvoiceType.PurchaseInvoice)
        {

        }
        public PurchaseInvoiceViewModel(InvoiceType type)
            : base(type)
        {

        }
        public PurchaseInvoiceViewModel(Guid id)
            : base(id)
        {

        }

        #region Internal methods

        protected override void OnInitialize()
        {
            if (Invoice.Partner == null) SetDefaultPartner(PartnersManager.GetDefaultParnerByInvoiceType((InvoiceType)Invoice.InvoiceTypeId));
            base.OnInitialize();
            //Title = "Գնում";
            FromStocks = StockManager.GetStocks().ToList();
            AddBySingle = ApplicationManager.Settings.SettingsContainer.MemberSettings.PurchaseBySingle;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            PreviewAddInvoiceItem(o);
        }
        protected override decimal GetProductPrice(ProductModel product)
        {
            return product != null ? (product.CostPrice ?? 0) : 0;
        }
        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);
            InvoicePaid.Paid = Invoice.Total = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
        }
        #endregion

        #region External Methods
        protected override void PreviewAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o)) { return; }
            if (!SetQuantity(AddBySingle)) { return; }
            base.PreviewAddInvoiceItem(o);
        }

        protected override void OnAddInvoiceItem()
        {
            base.OnAddInvoiceItem();
            //InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
            RaisePropertyChanged("InvoicePaid");
        }
        protected override bool CanApprove(object o)
        {
            var paid = InvoicePaid.Paid ?? 0;
            //var receivedPrepayment = InvoicePaid.ReceivedPrepayment ?? 0;
            //var partnerMaxDebit = Partner != null ? Partner.MaxDebit ?? 0 : 0;
            return base.CanApprove(o)
                && InvoicePaid.IsPaid
                && InvoicePaid.Change <= paid
                && Partner != null
                //&& (receivedPrepayment == 0 || receivedPrepayment <= partnerMaxDebit - Partner.Debit)
                && ApplicationManager.IsInRole(UserRoleEnum.SaleManager);
        }
        protected override void OnApprove(object o)
        {
            if (!CanApprove(o)) return;

            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.ProviderName = Partner.FullName;

            if (ToStock == null)
            {
                ToStock = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActivePurchaseStocks.ToList()).ToList()).FirstOrDefault();
            }
            if (ToStock == null)
            {
                MessageManager.ShowMessage("Պահեստ ընտրված չէ: Խնդրում ենք խմբագրել նոր պահեստ:", "Գործողության ընդհատում", MessageBoxImage.Error);
                return;
            }
            Invoice.ToStockId = ToStock.Id;
            Invoice.RecipientName = ToStock.FullName;
            if (InvoicePaid.ByCash > 0)
            {
                var cashDesk = SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.PurchaseCashDesks).SingleOrDefault();
                InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;
            }
            if (InvoicePaid.ByCheck > 0)
            {
                var bankAccount = SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.PurchaseBankAccounts).SingleOrDefault();
                InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
            }

            var invoice = InvoicesManager.ApproveInvoice(Invoice, InvoiceItems.ToList(), new List<StockModel> { ToStock }, InvoicePaid);
            if (invoice == null)
            {
                Invoice.AcceptDate = Invoice.ApproveDate = null;
                ToStock = null;
                Invoice.ToStockId = null;
                Invoice.RecipientName = null;
                MessageManager.OnMessage("Գործողությունը ձախողվել է:", MessageTypeEnum.Warning);
                return;
            }
            Invoice = invoice;
            LoadInvoice();
            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
            MessageManager.OnMessage(string.Format("Ապրանքագիր {0} հաստատված է։", Invoice.InvoiceNumber), MessageTypeEnum.Success);
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            OnApprove(null);
        }
        public override void OnApproveAndClose(object o)
        {
            OnApprove(o);
            if (Invoice.ApproveDate != null)
            {
                OnClose(o);
            }
        }
        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl =
                new PurchaseInvoiceLargeView(new PurchaseInvocieTicketViewModel(Invoice, InvoiceItems.ToList(),
                    StockManager.GetStock(Invoice.ToStockId), InvoicePaid));
            PrintManager.PrintPreview(ctrl, "Print purchase invoice", true);
        }

        #endregion
    }
}
