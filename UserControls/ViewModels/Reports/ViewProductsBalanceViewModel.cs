using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using AccountingTools.Enums;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Data.Models;
using ES.Data.Models.Products;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UIHelper.Managers;
using UserControls.Enumerations;
using ProductModel = ES.Data.Models.Products.ProductModel;

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

                if (_changedOnly)
                {
                    _dateIntermediate = SelectManager.GetDateIntermediate();
                    //        var win = new SelectCount(new SelectCountModel(_days, "Մուտքագրել օրերի քանակը"), Visibility.Collapsed);
                    //        win.ShowDialog();
                    //        if (!win.DialogResult.HasValue || !win.DialogResult.Value) { UpdateCompleted(false); return; }
                    //        _days = (int)win.SelectedCount;
                    //        _date = DateTime.Now;
                }
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
                    item.ExistingQuantity = ProductsManager.GetProductQuantityFromInvoices(item.Id, _date);
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

    public class ViewProductsBalanceViewModel<T> : TableViewModel<T>
    {
        #region Internal properties
        private ProductsViewEnum _viewEnum;
        private List<StockModel> _stocks;
        private Tuple<DateTime?, DateTime?> _dateIntermediate;
        protected List<PartnerModel> Partners;
        #endregion Internal properties

        #region External properties
        public List<StockModel> Stocks { get { return _stocks; } }
        #endregion External properties

        public ViewProductsBalanceViewModel(ProductsViewEnum viewEnum) : base()
        {
            _viewEnum = viewEnum;
            Initialize();
        }

        protected sealed override void Initialize()
        {
            base.Initialize();
            var title = "Մնացորդ";
            //_stocks = CashManager.Instance.GetStocks;
            switch (_viewEnum)
            {
                case ProductsViewEnum.ByStocks:
                    break;
                case ProductsViewEnum.ByDetile:
                    break;
                case ProductsViewEnum.ByProducts:
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                case ProductsViewEnum.ByPrice:
                    title += " (արժեք)";
                    break;
                case ProductsViewEnum.ByProviders:
                    title += " (ըստ պատվիրատուների)";

                    break;
            }
            Title = title;
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                switch (_viewEnum)
                {
                    case ProductsViewEnum.ByPrice:
                        _dateIntermediate = new Tuple<DateTime?, DateTime?>(DateTime.Now, null);
                        break;
                    case ProductsViewEnum.ByStocks:
                        _dateIntermediate = new Tuple<DateTime?, DateTime?>(SelectManager.GetDate(_dateIntermediate.Item1), null);
                        _stocks = ApplicationManager.Instance.CashProvider.GetStocks;
                        break;
                    case ProductsViewEnum.ByDetile:
                        break;
                    case ProductsViewEnum.ByProducts:
                        _dateIntermediate = new Tuple<DateTime?, DateTime?>(SelectManager.GetDate(_dateIntermediate.Item1), null);
                        break;
                    case ProductsViewEnum.ByProductItems:
                        break;
                    case ProductsViewEnum.ByCategories:
                        break;
                    case ProductsViewEnum.ByProviders:
                        Partners = SelectItemsManager.SelectPartners(ApplicationManager.CashManager.GetPartners, true, "Ընտրել մատակարար");
                        if (Partners == null || !Partners.Any()) { UpdateCompleted(false); return; }
                        //var di = SelectManager.GetDateIntermediate();
                        //_dateIntermediate = new Tuple<DateTime?, DateTime?>(di.Item1, di.Item2);
                        break;
                    default:
                        var products = ProductsManager.GetProducts();
                        break;
                }
            });


            switch (_viewEnum)
            {
                case ProductsViewEnum.ByPrice:
                    if (_dateIntermediate == null)
                    {
                        IsLoading = false; return;
                    }
                    var dateNow = DateTime.Now;
                    var productBalance = ProductsManager.GetProductsBalance(dateNow);

                    DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                    {
                        foreach (var invoiceReport in productBalance)
                        {
                            ViewList.Add((T)invoiceReport);
                        }
                    });

                    if (_dateIntermediate.Item1.HasValue && dateNow.Date == ((DateTime)_dateIntermediate.Item1).Date)
                    {
                        DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                        {
                            IInvoiceReport item = new InvoiceReport
                            {
                                Description = string.Format("Ընդամենը մնացորդ {0} դրությամբ", _dateIntermediate.Item1),
                                Count = productBalance.Count(),
                                Quantity = productBalance.Sum(t => t.Quantity),
                                Cost = productBalance.Sum(t => t.Cost),
                                Price = productBalance.Sum(t => t.Price),
                                Sale = productBalance.Sum(t => t.Sale),
                            };
                            ViewList.Add((T)item);
                        });
                        break;
                    }

                    var purcaseCostPrice = AccountingRecordsManager.GetAccountingRecordsByAccountingPlan((DateTime)_dateIntermediate.Item1, dateNow, AccountingPlanEnum.Purchase);
                    //var salePrice = AccountingRecordsManager.GetAccountingRecordsByAccountingPlan(_date.Value, dateNow, AccountingPlanEnum.Proceeds);
                    var salePrice = InvoicesManager.GetSaleInvoicesPrice(_dateIntermediate.Item1);
                    var saleCostPrice = InvoicesManager.GetPurchaseInvoicesPrice(_dateIntermediate.Item1);

                    DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                    {
                        IInvoiceReport item = new InvoiceReport
                        {
                            Description = string.Format("Ապրանքի ձեռքբերում մինչև {0}", _dateIntermediate.Item1),
                            //Count = pi.GroupBy(t => t.ProductId).Count(),
                            //Quantity = pi.Sum(t => t.Quantity),
                            Cost = purcaseCostPrice != null ? purcaseCostPrice.Item1 : 0,
                            Price = saleCostPrice,
                            //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),
                        };
                        ViewList.Add((T)item);
                        item = new InvoiceReport
                        {
                            Description = string.Format("Ապրանքի վաճառք {0} մինչև {1}", _dateIntermediate.Item1, dateNow),
                            //Count = pi.GroupBy(t => t.ProductId).Count(),
                            //Quantity = pi.Sum(t => t.Quantity),
                            Cost = purcaseCostPrice != null ? purcaseCostPrice.Item2 : 0,
                            Price = salePrice != null ? salePrice.Item2 : 0,
                            //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),

                        };
                        ViewList.Add((T)item);

                        //Total
                        item = new InvoiceReport
                        {
                            Description = string.Format("Ընդամենը մնացորդ {0} դրությամբ", _dateIntermediate.Item1),
                            //Count = pi.GroupBy(t => t.ProductId).Count(),
                            //Quantity = pi.Sum(t => t.Quantity),
                            Cost = purcaseCostPrice != null && productBalance.Any() ? productBalance.Last().Cost - (purcaseCostPrice.Item1 - purcaseCostPrice.Item2) : 0,
                            Price = productBalance.Any() ? productBalance.Last().Price : 0 + (salePrice != null ? (salePrice.Item2) : 0) - saleCostPrice,
                            //Sale = pi.Sum(t => t.Quantity * ((t.Products.Price ?? 0) - t.CostPrice)),

                        };
                        ViewList.Add((T)item);
                    });
                    break;
                case ProductsViewEnum.ByStocks:
                    if (_dateIntermediate.Item1.HasValue || !_stocks.Any())
                    {
                        IsLoading = false; return;
                    }
                    SetResult(ProductsManager.GetProductsByStock(_dateIntermediate.Item1, Stocks).Cast<T>().ToList());
                    break;
                case ProductsViewEnum.ByDetile:
                    break;
                case ProductsViewEnum.ByProducts:
                    var items = ProductsManager.GetProductsForView();
                    if (items != null && _dateIntermediate.Item1.HasValue)
                    {
                        foreach (var item in items)
                        {
                            var provider = PartnersManager.GetProviderForProduct(item);
                            item.Provider = provider;
                            item.ExistingQuantity = ProductsManager.GetProductQuantityFromInvoices(item.Id, _dateIntermediate.Item1);
                            var item1 = item;
                            IInvoiceReport newItem = new InvoiceReport
                            {
                                Code = item1.Code,
                                Description = item1.Description,
                                Quantity = item1.ExistingQuantity ?? 0,
                                Price = item1.Price ?? 0,
                                Cost = item1.CostPrice ?? 0,
                                Sale = item1.ExistingQuantity * item1.Price
                            };
                            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                            {

                                ViewList.Add((T)newItem);
                            });
                        }
                    }
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                case ProductsViewEnum.ByProviders:

                    _stocks = CashManager.Instance.GetStocks;

                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Կոդ", BindingText = "Code", CanSort = true, SortKey = "Կոդ" });
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Անվանում", BindingText = "Description", CanSort = true, SortKey = "Անվանում" });
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Չմ", BindingText = "Mu", CanSort = true, SortKey = "Չմ" });
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Գին", BindingText = "Price", CanSort = true, SortKey = "Գին" });
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Պատվեր", BindingText = "DemandQuantity", CanSort = true, SortKey = "Պատվեր" });
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Առկա", BindingText = "ExistingQuantity", CanSort = true, SortKey = "Առկա" });


                    foreach (var stock in Stocks)
                    {
                        ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = stock.Name, BindingText = "StockProducts", CanSort = true, SortKey = stock.Name, ConverterParameter = stock.Name, Converter = new Controls.StockProductsConverter() });
                    }
                    ColumnHeaderMetadatas.Add(new Controls.ProductDataGridColumnHeaderMetadata { Header = "Մատակարար", BindingText = "ProviderDescription", CanSort = true, SortKey = "Մատակարար" });

                    var invoiceItems = ProductsManager.GetProductItemsByPartners(Partners.Select(p => p.Id).ToList());

                    var iItems = invoiceItems.GroupBy(ii => new { ii.Product.Code, ii.CreateInvoice.InvoiceTypeId }).ToList();

                    foreach (var item in iItems)
                    {
                        var productItem = item.First();
                        IProductOrderModel productOrderModel = new ProductOrderModel
                        {
                            Product = productItem.Product,
                            Code = productItem.Product.Code,
                            Description = productItem.Product.Description,
                            Mu = productItem.Product.Mu,
                            CostPrice = productItem.ProductItem.CostPrice,
                            Price = productItem.Product.Price,
                            ExistingQuantity = item.Sum(s => s.ProductItem.Quantity),
                            Provider = productItem.Provider
                        };

                        foreach (var stockModel in Stocks)
                        {
                            productOrderModel.StockProducts.Add(
                                new StockProducts
                                {
                                    Product = ProductsManager.Convert(productItem.ProductItem, productItem.Product),
                                    Stock = stockModel,
                                    Quantity = item.Where(s => s.ProductItem.StockId == stockModel.Id).Sum(s => s.ProductItem.Quantity)
                                });
                        }
                        DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                            {
                                ViewList.Add((T)productOrderModel);
                            });
                    }

                    //var items = (from item in invoiceItems
                    //             group item by new { item.ProductId, item.CostPrice }
                    //         into product
                    //             where product != null
                    //             select product).ToList();

                    //foreach (var productItem in invoiceItems)
                    //{                        
                    //    if (productItem == null || productItem.Product == null) continue;
                    //    var productOrderModel = new ProductOrderModel
                    //    {
                    //        Product = productItem.First().Product,
                    //        Code = productItem.First().Product.Code,
                    //        Description = productItem.First().Product.Description,
                    //        Mu = productItem.First().Product.Mu,
                    //        CostPrice = productItem.First().CostPrice,
                    //        Price = productItem.First().Product.Price,
                    //        ExistingQuantity = productItem.Sum(s => s.Quantity)
                    //    };
                    //    foreach (var stockModel in _stocks)
                    //    {
                    //        productOrderModel.StockProducts.Add(
                    //            new StockProducts
                    //            {
                    //                Product = productItem.First(),
                    //                Stock = stockModel,
                    //                Quantity = productItem.Where(s => s.StockId == stockModel.Id).Sum(s => s.Quantity)
                    //            });
                    //    }
                    //    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                    //    {
                    //        Items.Add(productOrderModel);
                    //    });
                    //}



                    UpdateCompleted(true);
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

    public class ViewProductsBalanceByInvoiceViewModel : ViewProductsBalanceViewModel<IInvoiceReport>
    {
        public ViewProductsBalanceByInvoiceViewModel(ProductsViewEnum viewEnum) : base(viewEnum) { }
    }
    public class ViewProductsBalanceByPartnerViewModel : ViewProductsBalanceViewModel<IProductOrderModel>
    {
        public ViewProductsBalanceByPartnerViewModel() : base(ProductsViewEnum.ByProviders)
        {
            UpdateAsync();
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

    public class ViewModifiedProductsViewModel : ItemsDataViewModelBase<LogForProductsModel>
    {
        private bool _showPriceChangedOnly;
        private Tuple<DateTime, DateTime> _dateIntermediate;
        public ViewModifiedProductsViewModel()
        {

        }
        public ViewModifiedProductsViewModel(bool showPriceChangedOnly)
        {
            _showPriceChangedOnly = showPriceChangedOnly;
        }
        protected override void Initialize()
        {
            base.Initialize();
            Title = "Խմբագրված ապրանքների ցուցակ";
        }

        protected override void UpdateAsync()
        {

            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                _dateIntermediate = SelectManager.GetDateIntermediate();
            });

            var items = ProductsManager.GetModifiedProducts(_dateIntermediate, _showPriceChangedOnly);
            //if (items != null)
            //{
            //    foreach (var item in items)
            //    {
            //        item.Product = CashManager.Instance.GetProduct(item.ProductId);
            //        item.Modifier = CashManager.Instance.GetUser(item.ModifierId);
            //        AddNewItem(item);
            //    }
            //}
            AddNewItems(items);
            UpdateCompleted(true);
        }

        protected override void OnExport(ExportImportEnum obj)
        {
            //base.OnExport(obj);
            ExcelExportManager.ExportList(Items.Select(s => new { Ամսաթիվ = s.Date, Կոդ = s.Code, Անվանում = s.Description, Չմ = s.Product.Mu, Ինքնարժեք = s.CostPrice, Գին = s.Price }));

        }
    }
}
