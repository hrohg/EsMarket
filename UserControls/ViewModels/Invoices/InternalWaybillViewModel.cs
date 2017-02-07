using System;
using System.Linq;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;

namespace UserControls.ViewModels.Invoices
{
    public class InternalWaybillInvoiceModel : InvoiceViewModel
    {
        #region External properties
        public override string Title
        {
            get { return string.Format("Տեղափոխում{0}", Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty); }
        }
        public override string Description
        {
            get { return string.Format("{0}({1} - {2})", Title, FromStock != null ? FromStock.FullName : string.Empty, ToStock != null ? ToStock.FullName : string.Empty); }
        }
        public override bool CanAddInvoiceItem(object o)
        {
            return base.CanAddInvoiceItem(o) && FromStocks != null;
        }
        protected override void OnPrintInvoice(PrintSizeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new MoveInvoiceView(new MoveInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId, Invoice.MemberId), StockManager.GetStock(Invoice.ToStockId, Invoice.MemberId)));
            PrintManager.PrintPreview(ctrl, "Print move invoice", true);
        }
        public override bool CanApprove(object o)
        {
            return base.CanApprove(o)
                && FromStock != null
                && ToStock != null
                && FromStock.Id != ToStock.Id;
        }
        public override void OnApprove(object o)
        {
            if (!CanApprove(o))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Գործողության ընդհատում: Գործողությունը հնարավոր չէ իրականացնել:", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            base.OnApprove(o);
        }
        public override void OnApproveAndClose(object o)
        {
            OnApprove(o);
            if (Invoice.ApproveDate != null)
            {
                OnClose(o);
            }
        }
        #endregion
        public InternalWaybillInvoiceModel(EsUserModel user, EsMemberModel member)
            : base(user, member)
        {
            Initialize();
        }
        public InternalWaybillInvoiceModel(Guid id, EsUserModel user, EsMemberModel member)
            : base(id, user, member)
        {
            Initialize();
        }

        #region Internal methods

        private void Initialize()
        {
            FromStock = StockManager.GetStock(Invoice.FromStockId, Member.Id);
            ToStock = StockManager.GetStock(Invoice.ToStockId, Member.Id);
            Invoice.InvoiceTypeId = (int)InvoiceType.MoveInvoice;
            IsModified = false;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        #endregion

        #region External methods
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemCount(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList(), Member.Id);
            if (exCount == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Անբավարար միջոցներ: Տվյալ ապրանքատեսակից առկա չէ:", MessageModel.MessageTypeEnum.Warning));
                return false;
            }
            if (InvoiceItem.Quantity == null)
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
            if (InvoiceItem.Quantity == null || InvoiceItem.Quantity <= 0)
            {
                return false;
            }
            if (!(InvoiceItem.Quantity > exCount)) return true;
            InvoiceItem.Quantity = null;
            ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code), MessageModel.MessageTypeEnum.Warning));
            return false;
        }
        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
            if (InvoiceItem.Product != null)
            {
                InvoiceItem.Price = InvoiceItem.Product.Price;
            }
        }
        public override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o)) { return; }
            if (!SetQuantity(MoveBySingle)) { return; }
            base.OnAddInvoiceItem(o);
        }
        #endregion
    }
}
