using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using AccountingTools.Enums;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Data.Models;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UIHelper.Managers;
using UserControls.Enumerations;

namespace UserControls.ViewModels.Reports
{
    public class ViewProductsViewModel : ItemsDataViewModelBase<ProductModel>
    {
        private readonly bool _changedOnly;
        private DateTime? _date;
        private Tuple<DateTime, DateTime> _dateIntermediate;
        public ViewProductsViewModel(bool changedOnly = false)
        {
            _changedOnly = changedOnly;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Title = "Ապրանքների ցուցակ";
        }

        protected override void UpdateAsync()
        {

            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                _date = _changedOnly ? DateTime.Now : SelectManager.GetDate(_date);
                _dateIntermediate = SelectManager.GetDateIntermediate();
                //    if (_changedOnly)
                //  {
                //        var win = new SelectCount(new SelectCountModel(_days, "Մուտքագրել օրերի քանակը"), Visibility.Collapsed);
                //        win.ShowDialog();
                //        if (!win.DialogResult.HasValue || !win.DialogResult.Value) { UpdateCompleted(false); return; }
                //        _days = (int)win.SelectedCount;
                //        _date = DateTime.Now;
                //    }
                //    else
                //    {
                //        _date = UIHelper.Managers.SelectManager.GetDate(_date);
                //    }
            });


            var items = _changedOnly ? ProductsManager.GetChangedProducts(_dateIntermediate) : ProductsManager.GetProductsForView();
            if (items != null && _date != null)
            {
                foreach (var item in items)
                {
                    var provider = PartnersManager.GetProviderForProduct(item);
                    item.Provider = provider;
                    item.ExistingQuantity = ProductsManager.GetProductQuantity(item.Id, _date);
                    AddNewItem(item);
                }
            }
            UpdateCompleted(true);
        }

        protected override void OnExport(ExportImportEnum obj)
        {
            //base.OnExport(obj);
            ExcelExportManager.ExportList(Items.Select(s => new { Կոդ = s.Code, Անվանում = s.Description, Չմ = s.Mu, ՆախորդԳին = s.OldPrice, Գին = s.Price, Ամսաթիվ = s.LastModifiedDate, Մատակարար = s.Provider != null ? s.Provider.FullName : null, }));

        }
    }

    public class ViewProductsBalanceViewModel : TableViewModel<IInvoiceReport>
    {
        private ProductsViewEnum _viewEnum;
        private List<StockModel> _stocks;
        private DateTime? _date;

        public ViewProductsBalanceViewModel(ProductsViewEnum viewEnum)
        {
            _viewEnum = viewEnum;
            Initialize();
        }

        protected sealed override void Initialize()
        {
            base.Initialize();
            Title = (_viewEnum == ProductsViewEnum.ByPrice) ? "Մնացորդ (արժեք)" : "Մնացորդ";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                switch (_viewEnum)
                {
                    case ProductsViewEnum.ByPrice:
                        _date = UIHelper.Managers.SelectManager.GetDate(_date);
                        break;
                    case ProductsViewEnum.ByStocks:
                        _date = UIHelper.Managers.SelectManager.GetDate(_date);
                        _stocks = SelectItemsManager.SelectStocks(ApplicationManager.Instance.CashProvider.GetStocks);
                        break;
                    case ProductsViewEnum.ByDetile:
                        break;
                    case ProductsViewEnum.ByProducts:
                        _date = UIHelper.Managers.SelectManager.GetDate(_date);
                        break;
                    case ProductsViewEnum.ByProductItems:
                        break;
                    case ProductsViewEnum.ByCategories:
                        break;
                    case ProductsViewEnum.ByProviders:
                        break;
                    default:
                        var products = ProductsManager.GetProducts();
                        break;
                }
            });


            switch (_viewEnum)
            {
                case ProductsViewEnum.ByPrice:
                    if (_date == null)
                    {
                        IsLoading = false; return;
                    }
                    var dateNow = DateTime.Now;
                    var productBalance = ProductsManager.GetProductsBalance(dateNow);

                    DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                    {
                        foreach (var invoiceReport in productBalance)
                        {
                            ViewList.Add(invoiceReport);
                        }
                    });

                    if (dateNow.Date == _date.Value.Date)
                    {
                        DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                        {
                            ViewList.Add(new InvoiceReport
                            {
                                Description = string.Format("Ընդամենը մնացորդ {0} դրությամբ", _date),
                                Count = productBalance.Count(),
                                Quantity = productBalance.Sum(t => t.Quantity),
                                Cost = productBalance.Sum(t => t.Cost),
                                Price = productBalance.Sum(t => t.Price),
                                Sale = productBalance.Sum(t => t.Sale),
                            });
                        });
                        break;
                    }

                    var purcaseCostPrice = AccountingRecordsManager.GetAccountingRecordsByAccountingPlan(_date.Value, dateNow, AccountingPlanEnum.Purchase);
                    //var salePrice = AccountingRecordsManager.GetAccountingRecordsByAccountingPlan(_date.Value, dateNow, AccountingPlanEnum.Proceeds);
                    var salePrice = InvoicesManager.GetSaleInvoicesPrice(_date);
                    var saleCostPrice = InvoicesManager.GetPurchaseInvoicesPrice(_date);

                    DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                    {

                        ViewList.Add(new InvoiceReport
                        {
                            Description = string.Format("Ապրանքի ձեռքբերում մինչև {0}", _date),
                            //Count = pi.GroupBy(t => t.ProductId).Count(),
                            //Quantity = pi.Sum(t => t.Quantity),
                            Cost = purcaseCostPrice != null ? purcaseCostPrice.Item1 : 0,
                            Price = saleCostPrice,
                            //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),
                        });
                        ViewList.Add(new InvoiceReport
                       {
                           Description = string.Format("Ապրանքի վաճառք {0} մինչև {1}", _date, dateNow),
                           //Count = pi.GroupBy(t => t.ProductId).Count(),
                           //Quantity = pi.Sum(t => t.Quantity),
                           Cost = purcaseCostPrice != null ? purcaseCostPrice.Item2 : 0,
                           Price = salePrice != null ? salePrice.Item2 : 0,
                           //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),

                       });

                        //Total
                        ViewList.Add(new InvoiceReport
                        {
                            Description = string.Format("Ընդամենը մնացորդ {0} դրությամբ", _date),
                            //Count = pi.GroupBy(t => t.ProductId).Count(),
                            //Quantity = pi.Sum(t => t.Quantity),
                            Cost = purcaseCostPrice != null && productBalance.Any() ? productBalance.Last().Cost - (purcaseCostPrice.Item1 - purcaseCostPrice.Item2) : 0,
                            Price = productBalance.Any() ? productBalance.Last().Price : 0 + (salePrice != null ? (salePrice.Item2) : 0) - saleCostPrice,
                            //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),

                        });
                    });
                    break;
                case ProductsViewEnum.ByStocks:
                    if (_date == null || !_stocks.Any())
                    {
                        IsLoading = false; return;
                    }
                    SetResult(ProductsManager.GetProductsByStock(_date, _stocks));
                    break;
                case ProductsViewEnum.ByDetile:
                    break;
                case ProductsViewEnum.ByProducts:
                    var items = ProductsManager.GetProductsForView();
                    if (items != null && _date != null)
                    {
                        foreach (var item in items)
                        {
                            var provider = PartnersManager.GetProviderForProduct(item);
                            item.Provider = provider;
                            item.ExistingQuantity = ProductsManager.GetProductQuantity(item.Id, _date);
                            var item1 = item;
                            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                            {
                                ViewList.Add(new InvoiceReport { Code = item1.Code, Description = item1.Description, Quantity = item1.ExistingQuantity ?? 0, Price = item1.Price ?? 0, Cost = item1.CostPrice ?? 0, Sale = item1.ExistingQuantity * item1.Price });
                            });
                        }
                    }
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                case ProductsViewEnum.ByProviders:
                    break;
                default:
                    var products = ProductsManager.GetProducts();
                    break;
            }

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            RaisePropertyChanged("ViewList");
        }
    }
    public class ViewProductsResidualViewModel : TableViewModel<IInvoiceReport>
    {
        private List<StockModel> _stocks;
        private DateTime? _date;

        protected override void Initialize()
        {
            base.Initialize();
            Title = "Մնացորդ";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                _date = UIHelper.Managers.SelectManager.GetDate(_date);
                _stocks = SelectItemsManager.SelectStocks(ApplicationManager.Instance.CashProvider.GetStocks);

            });

            if (_date == null || !_stocks.Any())
            {
                IsLoading = false;
                return;
            }
            SetResult(ProductsManager.GetProductsByStock(_date, _stocks));
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            RaisePropertyChanged("TotalRows");
            RaisePropertyChanged("Total");
        }
    }

}
