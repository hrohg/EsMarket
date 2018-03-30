using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Data.Models;

namespace UserControls.ViewModels.Invoices
{
    public class InventoryWriteOffViewModel : SaleInvoiceViewModel
    {
        #region Internal properties

        #endregion
        #region External properties
        public override string Title { get { return "Դուրսգրման ակտ"; } }
        #endregion
        public InventoryWriteOffViewModel()
            : base(InvoiceType.InventoryWriteOff)
        {

        }

        public InventoryWriteOffViewModel(Guid id)
            : base(id)
        {

        }

        #region Internal methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }
        protected override void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;
        }
        #endregion

        #region External methods
        protected override void OnAddInvoiceItem(object o)
        {
            base.OnAddInvoiceItem(o);
            InvoiceItem.Price = 0;
        }
        protected override decimal GetProductPrice(EsProductModel product)
        {
            return product != null ? (product.Price ?? 0) : 0;
        }

        protected override bool CanApprove(object o)
        {
            return Invoice != null && Invoice.ApproveDate == null && InvoiceItems != null && InvoiceItems.Count > 0;
        }

        private void Approve(List<long> fromStocks)
        {
            IsLoading = true;
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;
            var invoice = InvoicesManager.RegisterInventoryWriteOffInvoice(Invoice, InvoiceItems.ToList(), fromStocks);

            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => ReloadApprovedInvoice(invoice)));
            if (invoice == null)
            {
                Invoice.AcceptDate = Invoice.ApproveDate = null;
                MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
            }
            else
            {
                Invoice = invoice;
                InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index));
                IsModified = false;
            }
            IsLoading = false;
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            try
            {
                if (IsModified && !Save())
                {
                    IsLoading = false;
                    return;
                }

                if (FromStocks != null && FromStocks.Any())
                {
                    Approve(FromStocks.Select(s => s.Id).ToList());
                    if (Invoice.ApproveDate != null) MessageManager.OnMessage(string.Format("{0} ապրանքագիրը հաստատվել է հաջողությամբ:", Invoice.InvoiceNumber), MessageTypeEnum.Success);

                    if (closeOnExit && Invoice.ApproveDate != null && Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => OnClose(null)));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
            }
            IsLoading = false;
        }

        #endregion

        #region Commands

        #endregion //Commands

    }
}
