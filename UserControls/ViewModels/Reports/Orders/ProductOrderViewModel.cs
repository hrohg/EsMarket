using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;

namespace UserControls.ViewModels.Reports.Orders
{
    public class ProductOrderBySaleViewModel : TableViewModel<ProductOrderModel>
    {
        #region Internal properties
        private ProductOrderTypeEnum _productOrderType;
        #endregion
        #region External properties
        protected override ObservableCollection<ProductOrderModel> ViewFilteredList
        {
            get
            {
                return new ObservableCollection<ProductOrderModel>(Reports.Where(s => IsShowNulls || s.DemandQuantity > 0));
            }
        }

        #endregion
        public ProductOrderBySaleViewModel(ProductOrderTypeEnum productOrderType)
        {
            IsShowUpdateButton = true;
            _productOrderType = productOrderType;
        }

        #region Internal methods

        protected override void Initialize()
        {
            IsClosable = true;
            Title = "Անհրաժեշտ ապրանքների ցուցակ";
            base.Initialize();
        }
        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            List<ProductProvider> productProviders;
            List<ProductOrderModel> report;
            switch (_productOrderType)
            {
                case ProductOrderTypeEnum.ByQuantity:

                    report = ProductsManager.GetProductOrderByProduct();

                    if (report == null || report.Count == 0)
                    {
                        MessageManager.ShowMessage("Նշված տվյալներով պատվեր չի հայտնաբերվել", "Պատվեր");
                        UpdateCompleted(false);
                        return;
                    }
                    for (var i = 0; i < report.Count; i++)
                        report[i].Index = i;

                    break;
                case ProductOrderTypeEnum.ByProviders:
                    Title = "Անհրաժեշտ ապրանքների ցուցակ";
                    report = ProductsManager.GetProductOrderByProduct();
                    if (report == null || report.Count == 0)
                    {
                        MessageManager.ShowMessage("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                        UpdateCompleted(false);
                        return;
                    }
                    for (var i = 0; i < report.Count; i++)
                    {
                        report[i].Index = i;
                    }

                    break;
                case ProductOrderTypeEnum.ByBrands:
                    Title = "Անհրաժեշտ ապրանքների ցուցակ";
                    var brands = SelectItemsManager.SelectBrands(ProductsManager.GetBrands(ApplicationManager.Instance.GetMember.Id), true);
                    report = ProductsManager.GetProductOrderByBrands(brands, ApplicationManager.Instance.GetMember.Id);
                    if (report == null || report.Count == 0)
                    {
                        MessageManager.ShowMessage("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                        UpdateCompleted(false);
                        return;
                    }
                    for (var i = 0; i < report.Count; i++)
                    {
                        report[i].Index = i;
                    }


                    break;
                case ProductOrderTypeEnum.BySale:
                    Tuple<DateTime, DateTime> dateIntermediate = null;
                    Application.Current.Dispatcher.Invoke(new Action(() => { dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate(); }));

                    if (dateIntermediate == null) { UpdateCompleted(false); return; }
                    Title = Description = string.Format("Պատվեր ըստ վաճառքի և մնացորդի (մանրամասն) {0} - {1}", dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);

                    var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2);
                    var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Where(s => s.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Select(s => s.Id));
                    var productItems = ProductsManager.GetProductItems();
                    var productOrder = invoiceItems.GroupBy(s => s.ProductId).Where(s => s.FirstOrDefault() != null).Select(s => new ProductOrderModel
                    {
                        ProductId = s.First().ProductId,
                        Code = s.First().Product.Code,
                        Description = s.First().Product.Description,
                        Mu = s.First().Product.Mu,
                        MinPrice = s.First().Product.CostPrice,
                        CostPrice = s.First().Product.CostPrice,
                        Price = s.First().Product.Price,
                        MinQuantity = s.First().Product.MinQuantity,
                        ExistingQuantity = productItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity),
                        SaleQuantity = invoiceItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity ?? 0),
                        Notes = s.First().Product.Note
                    }).ToList();
                    productOrder = productOrder.Where(s => s.DemandQuantity >= 0).ToList();
                    productProviders = ProductsManager.GetProductsProviders(productOrder.Select(s => s.ProductId).ToList());
                    var providers = PartnersManager.GetPartners(productProviders.Select(s => s.ProviderId).ToList());
                    foreach (var item in productOrder)
                    {
                        var productProvider = productProviders.FirstOrDefault(s => s.ProductId == item.ProductId);
                        if (productProvider == null) continue;
                        var provider = providers.FirstOrDefault(s => s.Id == productProvider.ProviderId);
                        if (provider == null) continue;
                        item.Provider = provider.FullName;
                    }
                    report = productOrder.OrderBy(s => s.Notes).ThenBy(s => s.Description).ThenBy(s => s.Code).ToList();
                    break;
                case ProductOrderTypeEnum.ViewPartners:
                    IsShowNulls = true;
                    var products = ApplicationManager.Instance.CashProvider.Products;
                    productProviders = ProductsManager.GetProductsProviders(products.Select(s => s.Id).ToList());

                    var partners = ApplicationManager.Instance.CashProvider.GetPartners;
                    productOrder = productProviders.Select(s => new ProductOrderModel
                    {
                        ProductId = s.ProductId,
                        Code = products.Single(p => p.Id == s.ProductId).Code,
                        Description = products.Single(p => p.Id == s.ProductId).Description,
                        Mu = products.Single(p => p.Id == s.ProductId).Mu,
                        MinPrice = products.Single(p => p.Id == s.ProductId).CostPrice,
                        CostPrice = products.Single(p => p.Id == s.ProductId).CostPrice,
                        Price = products.Single(p => p.Id == s.ProductId).Price,
                        MinQuantity = products.Single(p => p.Id == s.ProductId).MinQuantity,
                        //ExistingQuantity = productItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity),
                        //SaleQuantity = invoiceItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity ?? 0),
                        Notes = products.Single(p => p.Id == s.ProductId).Note,
                        Provider = partners.Single(p => p.Id == s.ProviderId).FullName
                    }).ToList();


                    report = productOrder.OrderBy(s => s.Notes).ThenBy(s => s.Description).ThenBy(s => s.Code).ToList();
                    if (!report.Any())
                    {
                        MessageManager.ShowMessage("Նշված տվյալներով պատվեր չի հայտնաբերվել", "Պատվեր");
                        UpdateCompleted(false);
                        return;
                    }



                    //for (var i = 0; i < _items.Count; i++)
                    //    _items[i].Index = i;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            SetResult(report);
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });

        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            TotalCount = (double)ViewList.Sum(s => s.ExistingQuantity);
            Total = (double)ViewList.Sum(i => i.Amount);

        }

        protected override void OnExport(ExportImportEnum o)
        {
            base.OnExport(o);
            ExcelExportManager.ExportList(ViewList.Select(s => new { Կոդ = s.Code, Անվանում = s.Description, Մնացորդ = s.ExistingQuantity, Քանակ = s.DemandQuantity, ԻՆքնարժեք = s.CostPrice, Գին = s.Price, Մատակարար = s.Provider, }));
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null)
            {
                return;
            }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            if (productOrder == null) return;
            PrintManager.PrintEx(productOrder);
        }

        #endregion
    }
}
