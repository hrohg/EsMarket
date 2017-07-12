using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;

namespace UserControls.ViewModels.Invoices
{
    public class InternalWaybillViewModel : InvoiceViewModel
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
        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new MoveInvoiceView(new MoveInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId), StockManager.GetStock(Invoice.ToStockId)));
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
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Գործողության ընդհատում: Գործողությունը հնարավոր չէ իրականացնել:", MessageTypeEnum.Warning));
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
        public InternalWaybillViewModel(EsUserModel user, EsMemberModel member)
            : base(user, member)
        {
            Initialize();
        }
        public InternalWaybillViewModel(Guid id, EsUserModel user, EsMemberModel member)
            : base(id, user, member)
        {
            Initialize();
        }

        #region Internal methods

        private void Initialize()
        {
            FromStock = StockManager.GetStock(Invoice.FromStockId);
            ToStock = StockManager.GetStock(Invoice.ToStockId);
            Invoice.InvoiceTypeId = (int)InvoiceType.MoveInvoice;
            IsModified = false;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        protected override decimal GetPartnerPrice(EsProductModel product)
        {
            return product != null ? (product.Price ?? 0) : 0;

        }
        #endregion

        #region External methods
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemCount(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList(), Member.Id);
            if (exCount > 0 && (InvoiceItem.Quantity == null || InvoiceItem.Quantity == 0))
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
            if (exCount == 0 || InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageBox.Show(message, "Անբավարար միջոցներ");
                return false;
            }
            return InvoiceItem.Quantity != null && InvoiceItem.Quantity > 0;
        }
        public override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o)) { return; }
            if (!SetQuantity(MoveBySingle)) { return; }
            base.OnAddInvoiceItem(o);
        }
        #endregion

        #region Commands

        private ICommand _selectStockCommand;
        public ICommand SelectStockCommand
        {
            get
            {
                return _selectStockCommand ?? (_selectStockCommand = new RelayCommand<StockTypeEnum>(OnSelectStock));
            }
        }

        private void OnSelectStock(StockTypeEnum stockTypeEnum)
        {
            var stocks = StockManager.GetStocks(ApplicationManager.Member.Id);
            if (stocks == null) return;
            StockModel stock = null;
            if (stocks.Count == 1)
            {
                stock = stocks.FirstOrDefault();
            }
            var selectItem = new ControlPanel.Controls.SelectItems(stocks.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), false);
            if (selectItem.ShowDialog() != true || selectItem.SelectedItems.FirstOrDefault() == null) { return; }
            stock = stocks.FirstOrDefault(s =>
            {
                var firstOrDefault = selectItem.SelectedItems.FirstOrDefault();
                return firstOrDefault != null && (long) firstOrDefault.SelectedValue == s.Id;
            });
            switch (stockTypeEnum)
            {
                case StockTypeEnum.None:
                    break;
                case StockTypeEnum.Outgoing:
                    FromStock = stock;
                    break;
                case StockTypeEnum.Incomming:
                    ToStock = stock;
                    break;
                case StockTypeEnum.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("stockTypeEnum", stockTypeEnum, null);
            }
        }

        #endregion Commands
    }

    [Flags]
    public enum StockTypeEnum
    {
        None = 0,
        Outgoing = 1,
        Incomming = 2,
        All = 0xF
    }
}
