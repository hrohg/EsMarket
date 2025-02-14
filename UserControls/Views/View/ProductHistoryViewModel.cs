﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;

namespace UserControls.Views.View
{
    public class ProductHistoryViewModel : DocumentViewModel
    {
        #region Internal properties

        private ObservableCollection<InvoiceItemsModel> _items;
        #endregion

        #region External properties
        public ObservableCollection<InvoiceItemsModel> InvoiceItems
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("InvoiceItems");
                RaisePropertyChanged("Input");
                RaisePropertyChanged("Move");
                RaisePropertyChanged("Output");
            }
        }
        public InvoiceItemsModel SelectedItem { get; set; }
        public decimal Input
        {
            get
            {
                return InvoiceItems.Where(s => s.Invoice.InvoiceTypeId == (int)InvoiceType.PurchaseInvoice || s.Invoice.InvoiceTypeId == (int)InvoiceType.ReturnFrom).Sum(s => s.Quantity ?? 0);
            }
        }
        public decimal Move
        {
            get
            {
                return InvoiceItems.Where(s => s.Invoice.InvoiceTypeId == (int)InvoiceType.MoveInvoice).Sum(s => s.Quantity ?? 0);
            }
        }
        public decimal Output
        {
            get
            {
                return InvoiceItems.Where(
                    s => s.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice ||
                    s.Invoice.InvoiceTypeId == (int)InvoiceType.InventoryWriteOff ||
                    s.Invoice.InvoiceTypeId == (int)InvoiceType.ReturnTo)

                    .Sum(s => s.Quantity ?? 0);
            }
        }
        #endregion

        #region Constructors
        public ProductHistoryViewModel()
        {
            Initialize();
            var date = UIHelper.Managers.SelectManager.GetDateIntermediate();
            if (date == null) return;
            var products = Helpers.SelectItemsManager.SelectProductByCheck(true);
            new Thread(() =>
            {
                var items = InvoicesManager.GetProductHistory(products.Select(s => s.Id).ToList(), date.Item1, date.Item2, ApplicationManager.Instance.GetMember.Id);
                DispatcherWrapper.Instance.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, () => InvoiceItems = new ObservableCollection<InvoiceItemsModel>(items));
            }).Start();


        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքաշրջանառություն";
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>();
        }
        #endregion
    }
}
