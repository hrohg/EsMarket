using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ES.Business.Managers;
using ES.Common.Managers;
using ES.DataAccess.Models;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;

namespace UserControls.ViewModels
{
    public class SaleProductsViewModel : TableViewModel<InvoiceItems>
    {

        #region Internal properties
        private List<InvoiceItems> _items = new List<InvoiceItems>(); 
        #endregion

        #region External properties
        public override ObservableCollection<InvoiceItems> ViewList
        {
            get
            {
                return new ObservableCollection<InvoiceItems>(_items);
            }
        }
        #endregion

        public SaleProductsViewModel(object o)
            : base()
        {
            Title = Description = "Վաճառք";
            IsShowUpdateButton = true;
            OnUpdate(o);
        }

        #region Internal methods
        private void Update(Tuple<DateTime, DateTime> dateIntermediate)
        {
            IsLoading = true;
            RaisePropertyChanged(IsInProgressProperty);
            IsLoading = true; RaisePropertyChanged(IsInProgressProperty);
            var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (int)InvoiceType.SaleInvoice);
            var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Select(s => s.Id));
            _items = invoiceItems.GroupBy(s => s.ProductId).Where(s => s.FirstOrDefault() != null).Select(s =>
                new InvoiceItems
                {
                    Id = s.First().ProductId,
                    Code = s.First().Product.Code,
                    Description = s.First().Product.Description,
                    Mu = s.First().Product.Mu,
                    Price = s.First().Price,
                    Quantity = s.Sum(t => t.Quantity),
                    Note = s.First().Product.Note
                }).ToList();
            _items = _items.OrderBy(s => s.Code).ThenBy(s => s.Description).ThenBy(s => s.Code).ToList();
            RaisePropertyChanged("ViewList");
            TotalRows = _items.Count;
            TotalCount = (double) _items.Sum(s => s.Quantity??0);
            Total = (double) _items.Sum(i => (i.Quantity ?? 0)*(i.Price??0));
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate(object o)
        {
            base.OnUpdate(o);
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if(dateIntermediate==null) return;
            Description = string.Format("Վաճառք {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            var thread = new Thread(() => Update(dateIntermediate));
            thread.Start();
        }
        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null) { return; }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }
        #endregion
    }

}
