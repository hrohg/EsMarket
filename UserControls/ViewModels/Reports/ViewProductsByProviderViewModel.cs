using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Data.Models;
using UserControls.Helpers;
using UserControls.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace UserControls.ViewModels.Reports
{
    public class ViewProductsByProviderViewModel : TableViewModel<ProductProviderReportModel>
    {
        public ViewProductsByProviderViewModel(List<ProductProviderReportModel> list)
            : base(list)
        {
            //ViewList = new ObservableCollection<ProductProviderReportModel>(list);

        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            List<ProductModel> products = null;
            Tuple<DateTime, DateTime> dateIntermediate = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                products = SelectItemsManager.SelectProduct(true);
                dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            });

            if (dateIntermediate == null) return;

            var invoiceItems = InvoicesManager.GetInvoiceItemsByCode(products.Select(s => s.Code), dateIntermediate.Item1, dateIntermediate.Item2, ApplicationManager.Member.Id).OrderBy(s => s.InvoiceId).ToList();
            var invoices = InvoicesManager.GetInvoices(invoiceItems.Select(s => s.InvoiceId).Distinct());
            SetResult(invoiceItems.Select(s =>
                new ProductProviderReportModel
                {
                    InvoiceNumber = invoices.Where(t => t.Id == s.InvoiceId).Select(t => t.InvoiceNumber).First(),
                    Date = invoices.Where(t => t.Id == s.InvoiceId).Select(t => t.CreateDate).First(),
                    Partner = invoices.Where(t => t.Id == s.InvoiceId).Select(t => t.Partner.FullName).First(),
                    Code = s.Code,
                    Description = s.Description,
                    Mu = s.Mu,
                    Quantity = s.Quantity ?? 0,
                    Price = s.Price ?? 0,
                }).ToList());

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }
    }
}
