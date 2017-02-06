﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Model;
using ES.Data.Models;
using UserControls.Helpers;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public class InventoryWriteOffViewModel : SaleInvoiceViewModel
    {
        #region Internal properties
        #endregion
        #region External properties
        public override string Title { get { return "Դուրսգրման ակտ"; } }
        #endregion
        public InventoryWriteOffViewModel(EsUserModel user, EsMemberModel member)
            : base(user, member)
        {
            Invoice.InvoiceTypeId = (int)InvoiceType.InventoryWriteOff;
            Initialize();
        }

        public InventoryWriteOffViewModel(Guid id, EsUserModel user, EsMemberModel member)
            : base(id, user, member)
        {
            Initialize();
        }

        #region Internal methods

        private void Initialize()
        {
            Invoice.PartnerId = null;
            Invoice.Partner = null;
            Invoice.RecipientName = null;
        }

        
        #endregion

        #region External methods
        public override void OnAddInvoiceItem(object o)
        {
            base.OnAddInvoiceItem(o);
            InvoiceItem.Price = 0;
        }
        public override decimal? GetPartnerPrice(EsProductModel product)
        {
            return product != null ? product.Price : 0;
        }

        public override bool CanApprove(object o)
        {
            return Invoice != null && Invoice.ApproveDate == null && InvoiceItems != null && InvoiceItems.Count > 0;
        }
        public override void OnApprove(object o)
        {
            if (!CanApprove(o))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողության ձախողում:", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            var fromStocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.GetEsMember.Id));
if(fromStocks==null || fromStocks.Count==0) return;
            var td = new Thread(()=>Approve(fromStocks.Select(s => s.Id).ToList()));
            td.Start();
        }

        private void Approve(List<long> fromStocks)
        {
            IsLoading = true;
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;
            var invoice = InvoicesManager.RegisterInventoryWriteOffInvoice(Invoice, InvoiceItems.ToList(), fromStocks);
            if (invoice == null)
            {
                Invoice.AcceptDate = Invoice.ApproveDate = null;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողության ձախողում:", MessageModel.MessageTypeEnum.Warning));
            }
            else
            {
                Invoice = invoice;
                InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
                IsModified = false;
            }
            IsLoading = false;
        }
        #endregion

        #region Commands

        #endregion //Commands

    }
}
