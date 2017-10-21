using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Models;

namespace UserControls.ViewModels.Invoices
{
    public class ReturnFromInvoiceViewModel : PurchaseInvoiceViewModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public override string Title
        {
            get { return string.Format("Ետ վերադարձ {0}", Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty); }
        }

        public bool CanChangePartner { get { return !InvoiceItems.Any(); } }
        #endregion External properties

        #region Constructors

        public ReturnFromInvoiceViewModel()
        {
            Invoice.InvoiceTypeId = (int)InvoiceType.ReturnFrom;
            Initialize();
        }

        public ReturnFromInvoiceViewModel(Guid id)
            : base(id)
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {

        }

        protected override void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnInvoiceItemsChanged(sender, e);
            RaisePropertyChanged("CanChangePartner");
        }
        #endregion Internal methods
    }
    public class ReturnToInvoiceViewModel : SaleInvoiceViewModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties
        public override string Title
        {
            get { return string.Format("Վերադարձ {0}", Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty); }
        }
        #endregion External properties

        #region Constructors

        public ReturnToInvoiceViewModel()
        {
            Invoice.InvoiceTypeId = (int)InvoiceType.ReturnTo;
            Initialize();
        }

        public ReturnToInvoiceViewModel(Guid id)
            : base(id)
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {

        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        protected override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            if (!SetQuantity(AddBySingle))
            {
                return;
            }

            base.OnAddInvoiceItem(o);


            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());
            if (!Invoice.PartnerId.HasValue) return;
            if (InvoiceItem.Quantity == null || InvoiceItem.Quantity == 0)
            {

                var quantity = InvoiceItem.Quantity ?? 0;
                var saleInvoiceItems = InvoicesManager.GetSaleInvoiceByProductId(InvoiceItem.ProductId, (Guid)Invoice.PartnerId, quantity);
                foreach (var invoiceItem in saleInvoiceItems)
                {
                    InvoiceItem.Quantity = Math.Min(invoiceItem.Quantity ?? 0, quantity);
                    quantity -= InvoiceItem.Quantity.Value;
                    base.OnAddInvoiceItem(o);
                    if (quantity > 0)
                    {
                        InvoiceItem = new InvoiceItemsModel(Invoice, invoiceItem.Product);
                    }
                }
            }
        }
        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
            var productItems = SelectItemsManager.SelectProductItems(ApplicationManager.Settings.MemberSettings.ActiveSaleStocks, true);
        }
        protected override void SetPrice() { }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (exCount == 0 || InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageBox.Show(message, "Անբավարար միջոցներ");
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
        #endregion Internal methods
    }
}
