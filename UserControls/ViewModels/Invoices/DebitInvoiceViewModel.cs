using System;
using System.Collections.Generic;
using System.Linq;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Data.Models;

namespace UserControls.ViewModels.Invoices
{
    public class InventoryWriteOffViewModel : OutputOrderViewModel
    {
        #region Internal properties

        #endregion

        #region External properties

        public override string Title
        {
            get { return "Դուրսգրման ակտ"; }
        }

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
            Invoice.PartnerId = null;
            Invoice.Partner = null;
            Invoice.ProviderName = null;
        }

        protected override void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;
        }

        #endregion

        #region External methods

        protected override void PreviewAddInvoiceItem(object o)
        {
            //InvoiceItem.Price = 0;
            base.PreviewAddInvoiceItem(o);
        }

        protected override decimal GetProductPrice(ProductModel product)
        {
            return product != null ? (product.Price ?? 0) : 0;
        }

        protected override bool CanApprove(object o)
        {
            return Invoice != null && Invoice.ApproveDate == null && InvoiceItems != null && InvoiceItems.Count > 0;
        }

        private void Approve(List<long> fromStocks, bool closeOnExit)
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
                Invoice.InvoiceNumber = invoice.InvoiceNumber;
                Invoice.ApproveDate = invoice.ApproveDate;
                
                if (!closeOnExit) Invoice = invoice;
            }

            IsLoading = false;
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            try
            {
                base.OnApproveAsync(closeOnExit);
                if (IsModified && !Save())
                {
                    IsLoading = false;
                    return;
                }
                if (FromStocks != null && FromStocks.Any())
                {
                    Approve(FromStocks.Select(s => s.Id).ToList(), closeOnExit);
                    if (Invoice.ApproveDate != null) MessageManager.OnMessage(string.Format("{0} ապրանքագիրը հաստատվել է հաջողությամբ:", Invoice.InvoiceNumber),MessageTypeEnum.Success);

                    if (closeOnExit && Invoice.ApproveDate != null)
                    {
                        OnClose();
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