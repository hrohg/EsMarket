﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shared.Helpers;

namespace UserControls.ViewModels.Invoices
{
    public class PackingListForSallerViewModel : InvoiceViewModelBase
    {
        #region Constructors

        public PackingListForSallerViewModel():base()
        {
            Initialize();
        }
        public PackingListForSallerViewModel(Guid invoiceId):base(invoiceId)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = string.Format("Ապրանքների ցուցակ {0}", IsInvocieValid && Invoice.InvoiceNumber!=null? Invoice.InvoiceNumber: string.Empty);
            Description = string.Format("{0} {1}", Title, IsInvocieValid? (Partner != null ? Partner.FullName : FromStock != null ? FromStock.FullName : string.Empty):string.Empty);
        }
        protected override bool CanImportInvoice(ExportInportEnum importFrom)
        {
            return false;
        }
        #endregion Internal methods
    }
}