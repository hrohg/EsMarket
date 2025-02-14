﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using EsMarket.SharedData.Models;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.Helpers;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;
using EsMarket.SharedData.Interfaces;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using ES.DataAccess.Models;

namespace UserControls.ViewModels
{
    //public class ReportDataViewModel : INotifyPropertyChanged
    //{
    //    #region Constants

    //    private const string IsInProgressProperty = "IsInProgress";
    //    #endregion

    //    #region Internal properties
    //    private bool _isInProgress;
    //    #endregion

    //    #region External properties
    //    public bool IsInProgress
    //    {
    //        get { return _isInProgress; }
    //        set
    //        {
    //            if (_isInProgress == value) return;
    //            _isInProgress = value;
    //            OnPropertyChanged(IsInProgressProperty);
    //        }
    //    }
    //    public ObservableCollection<object> Items { get; set; }
    //    #endregion

    //    #region Constructors

    //    #endregion

    //    #region Internal methods
    //    private void GetProductOrder(object o)
    //    {
    //        IsInProgress = true;
    //        var dateIntermediate = SelectManager.GetDateIntermediate();
    //        if (dateIntermediate == null) return;

    //        var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2);
    //        var invoiceItems = InvoicesManager.GetInvoiceItems(invoices.Select(s => s.Id));
    //        var productItems = ProductsManager.GetProductItems();
    //        var productOrder = new List<object>(productItems.GroupBy(s => s.Product).Select(s =>
    //            new ProductOrderModel
    //            {
    //                Code = s.First().Product.Code,
    //                Description = s.First().Product.Description,
    //                Mu = s.First().Product.Mu,
    //                MinPrice = s.First().Product.CostPrice,
    //                MinQuantity = s.First().Product.MinQuantity,
    //                ExistingQuantity = s.Sum(t => t.Quantity),
    //                SaleQuantity = invoiceItems.Where(t => t.ProductId == s.First().ProductId).Sum(t => t.Quantity ?? 0),
    //                Notes = s.First().Product.Note
    //            }).ToList());

    //        OnPropertyChanged("Items");
    //        IsInProgress = false;
    //    }
    //    private void OnGetProductOrder(object o)
    //    {
    //        ThreadStart ts = delegate { GetProductOrder(o); };
    //        Thread myThread = new Thread(ts);
    //        myThread.Start();
    //    }
    //    #endregion

    //    #region Commands
    //    public ICommand ProductOrderCommand { get { return new RelayCommand(OnGetProductOrder); } }
    //    #endregion

    //    #region INotifyPropertyChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(propertyName));
    //        }
    //    }
    //    #endregion

    //}

    public class ReportBaseViewModel : TableViewModel<InvoiceReport>
    {
        #region Internal properties
        protected Tuple<DateTime, DateTime> DateIntermediate { get; private set; }
        #endregion

        #region External properties

        #endregion

        #region Constructors

        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            Title = Description = "Հաշվետվություն";
            IsShowUpdateButton = true;
            base.Initialize();
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            DateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            if (DateIntermediate == null) return;
            Description = string.Format("Հաշվետվություն {0} - {1}", DateIntermediate.Item1.Date, DateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");

            IsLoading = true;

            var reports = InvoicesManager.GetSaleByPartners(DateIntermediate, ApplicationManager.Instance.GetMember.Id);
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                IsLoading = false;
                return;
            }

            SetResult(reports);
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
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

    public class InvoiceReportViewModel : TableViewModel<IInvoiceReport>
    {

        #region Internal properties
        private readonly List<InvoiceType> _invoiceTypes;
        protected Tuple<DateTime, DateTime> DateIntermediate;
        #endregion

        #region External properties

        #endregion

        #region Constructors
        public InvoiceReportViewModel(List<InvoiceType> invoiceTypes)
        {
            _invoiceTypes = invoiceTypes;
            IsShowUpdateButton = true;
        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Վաճառք";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            if (_invoiceTypes == null) { UpdateStopped(); return; }
            GetDate();
            if (DateIntermediate == null)
            {
                UpdateStopped(); return;
            }

            Description = string.Format("Վաճառքի հաշվետվություն {0} - {1}", DateIntermediate.Item1.Date, DateIntermediate.Item2.Date);

            SetResult(InvoicesManager.GetInvoicesReports(DateIntermediate.Item1, DateIntermediate.Item2, _invoiceTypes));
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                //TotalCount = (double)Reports.Sum(s => ((IInvoiceReport)s).Quantity ?? 0);
                Total = (double)ViewList.Sum(i => i.Sale ?? 0);
                base.UpdateCompleted(isSuccess);
            });
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
        protected void GetDate()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                DateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            });
        }
        #endregion
    }

    public class SaleInvoiceReportByStocksViewModel : SaleInvoiceReportByPartnerViewModel
    {
        private List<StockModel> _stocks;

        public SaleInvoiceReportByStocksViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Վաճառք ըստ բաժինների";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            if (DteIntermediate == null) { IsLoading = false; return; }
            _stocks = GetStocks();
            if (!_stocks.Any()) { IsLoading = false; return; }

            var invoiceItems = InvoicesManager.GetSaleInvoiceItemsByStocksForReport(DteIntermediate.Item1, DteIntermediate.Item2, _stocks.Select(s => s.Id).ToList());
            List<IInvoiceReport> reports = new List<IInvoiceReport>(_stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Count(ii => ii.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.CostPrice ?? 0)),
                Sale = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0))
            }).ToList());
            reports.Add(new InvoiceReport
            {
                Description = "Ընդամենը",
                Count = reports.Sum(s => ((InvoiceReport)s).Count),
                Quantity = reports.Sum(s => s.Quantity),
                Cost = reports.Sum(s => s.Cost),
                ReturnAmount = reports.Sum(s => ((InvoiceReport)s).ReturnAmount),
                Sale = reports.Sum(s => s.Sale)
            });


            if (!reports.Any())
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։",
                    MessageTypeEnum.Information));
            }
            else
            {
                SetResult(reports);
            }

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            TotalCount = (double)ViewList.Sum(s => s.Quantity);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
        }

        protected override void OnExport(ExportImportEnum o)
        {
            //base.OnExport(o);

            if (ApplicationManager.IsInRole(UserRoleEnum.Manager))
            {
                ExcelExportManager.ExportList(ViewList.Select(s => new
                {
                    Բաժին = ((InvoiceReport)s).Description,
                    Ապրանքագիր = ((InvoiceReport)s).Count,
                    Քանակ = ((InvoiceReport)s).Quantity,
                    Հասույթ = ((InvoiceReport)s).Sale,
                    ԻՆքնարժեք = ((InvoiceReport)s).Cost,
                    Եկամուտ = ((InvoiceReport)s).Profit,
                    Եկամտի_Տոկոս = ((InvoiceReport)s).Pers,
                    Ետվերադարձ = ((InvoiceReport)s).ReturnAmount,
                    Ընդամենը = ((InvoiceReport)s).Total
                }));
            }
            else
            {
                ExcelExportManager.ExportList(ViewList.Select(s => new
                {
                    Բաժին = ((InvoiceReport)s).Description,
                    Ապրանքագիր = ((InvoiceReport)s).Count,
                    Քանակ = ((InvoiceReport)s).Quantity,
                    Հասույթ = ((InvoiceReport)s).Sale,
                    Ետվերադարձ = ((InvoiceReport)s).ReturnAmount,
                    Ընդամենը = ((InvoiceReport)s).Total
                }));
            }
        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
        }
    }


    public class SaleInvoiceReportByPartnerViewModel : TableViewModel<IInvoiceReport>
    {
        #region Internal properties
        protected ViewInvoicesEnum ViewInvoicesEnum;
        protected Tuple<DateTime, DateTime> DteIntermediate;
        #endregion

        #region External properties

        #endregion

        #region Constructors
        public SaleInvoiceReportByPartnerViewModel(ViewInvoicesEnum viewInvoicesEnum)
        {
            ViewInvoicesEnum = viewInvoicesEnum;
            IsShowUpdateButton = true;
        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Վաճառք ըստ պատվիրատուների";
        }

        private void InitializeReport(List<IInvoiceReport> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                SetResult(reports);
            }
            if (reports != null)
            {

                TotalCount = (double)reports.Sum(s => s.Quantity);
                Total = (double)reports.Sum(i => i.Sale ?? 0);
            }
            IsLoading = false;
        }
        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            if (ViewInvoicesEnum == ViewInvoicesEnum.None) { UpdateStopped(); return; }

            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                {
                    DteIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
                });
            if (DteIntermediate == null) { UpdateStopped(); return; }
            Description = string.Format("{0} {1} - {2}", Title, DteIntermediate.Item1.Date, DteIntermediate.Item2.Date);

            List<IInvoiceReport> reports = null;
            List<PartnerModel> partners = null;
            List<PartnerType> partnerTypes = null;
            List<StockModel> stocks = null;
            switch (ViewInvoicesEnum)
            {
                case ViewInvoicesEnum.None:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    stocks = GetStocks();
                    reports = new List<IInvoiceReport>(UpdateByStock(stocks, DteIntermediate));
                    break;
                case ViewInvoicesEnum.ByProvider:
                case ViewInvoicesEnum.ByPartner:
                    partners = GetPartners();

                    if (partners != null && partners.Any())
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartners(DteIntermediate.Item1, DteIntermediate.Item2, ViewInvoicesEnum == ViewInvoicesEnum.ByProvider ? InvoiceType.PurchaseInvoice : InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ViewInvoicesEnum));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, new Action(() =>
                    {
                        partnerTypes = SelectItemsManager.SelectPartnersTypes(true);
                    }));

                    if (partnerTypes != null && partnerTypes.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnerTypes(DteIntermediate.Item1, DteIntermediate.Item2, InvoiceType.SaleInvoice, partnerTypes, ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    partners = GetPartners();
                    if (partners != null && partners.Any())
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnersDetiled(DteIntermediate.Item1, DteIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList()));
                    }
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    stocks = GetStocks();
                    var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(DteIntermediate.Item1, DteIntermediate.Item2).Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice).Select(s => s.Id).ToList(), stocks);

                    reports = new List<IInvoiceReport>(invoiceItems.Select(s =>
                        new SaleReportByPartnerDetiled
                        {
                            Partner = s.Invoice.ProviderName,
                            Invoice = s.Invoice.InvoiceNumber,
                            Date = s.Invoice.CreateDate,
                            Code = s.Code,
                            Description = s.Description,
                            Mu = s.Mu,
                            Quantity = s.Quantity ?? 0,
                            Cost = s.CostPrice ?? 0,
                            Price = s.Price ?? 0,
                            Sale = (s.Quantity ?? 0) * (s.Price ?? 0),
                            Note = s.Invoice.Notes
                        }).ToList());
                    break;
                case ViewInvoicesEnum.BySaleChart:
                    var invoices = InvoicesManager.GetInvoices(DteIntermediate.Item1, DteIntermediate.Item2)
                            .Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice).ToList();
                    if (!invoices.Any())
                    {
                        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                        break;
                    }

                    break;
                case ViewInvoicesEnum.ByZeroAmunt:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                SetResult(reports);
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }
        protected virtual List<PartnerModel> GetPartners()
        {
            List<PartnerModel> partners = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                partners = SelectItemsManager.SelectPartners(true);
            }));
            return partners;
        }
        protected virtual List<StockModel> GetStocks()
        {
            List<StockModel> stocks = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                stocks = SelectItemsManager.SelectStocks(ApplicationManager.CashManager.GetStocks, true).ToList();
            }));
            return stocks;
        }
        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            TotalCount = (double)ViewList.Sum(s => s.Quantity);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);

        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
            if (o == null)
            {
                return;
            }
            var productOrder = ((FrameworkElement)o).FindVisualChildren<Border>().FirstOrDefault();
            //if (productOrder == null)
            {
                PrintManager.PrintEx((FrameworkElement)o);
                return;
            }
            PrintManager.PrintEx(productOrder);
        }

        private List<InvoiceReport> UpdateByStock(List<StockModel> stocks, Tuple<DateTime, DateTime> dateIntermediate)
        {
            var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice || s.InvoiceTypeId == (short)InvoiceType.ReturnFrom).Select(s => s.Id).ToList(), stocks);
            var list = stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (short)InvoiceType.SaleInvoice).Count(ii => ii.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.CostPrice ?? 0)),
                Sale = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.ReturnFrom)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0))
            }).ToList();
            list.Add(new InvoiceReport
            {
                Description = "Ընդամենը",
                Count = list.Sum(s => s.Count),
                Quantity = list.Sum(s => s.Quantity),
                Cost = list.Sum(s => s.Cost),
                ReturnAmount = list.Sum(s => s.ReturnAmount),
                Sale = list.Sum(s => s.Sale)
            });
            return list;
        }

        protected override bool CanExport(ExportImportEnum o)
        {
            return base.CanExport(o);
        }

        protected override void OnExport(ExportImportEnum o)
        {
            base.OnExport(o);
            //ExcelExportManager.ExportList(ViewList.Select(s => new { Կոդ = s., Անվանում = s.Description, Մնացորդ = s.ExistingQuantity, Քանակ = s.DemandQuantity, ԻՆքնարժեք = s.CostPrice, Գին = s.Price, Մատակարար = s.Provider, }));
        }

        #endregion
    }

    public class SaleInvoiceReportByPartnersDetiledViewModel : SaleInvoiceReportByPartnerViewModel
    {
        protected List<PartnerModel> Partners { get; set; }
        #region Constructors
        public SaleInvoiceReportByPartnersDetiledViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = Description = "Վաճառք ըստ պատվիրատուների մանրամասն";
        }
        protected override void UpdateAsync()
        {
            base.UpdateAsync();
        }
        protected override List<PartnerModel> GetPartners()
        {
            Partners = base.GetPartners();
            return Partners;
        }
        protected override void OnExport(ExportImportEnum o)
        {

            base.OnExport(o);
            switch (o)
            {
                case ExportImportEnum.Excel:
                    ExcelExportManager.ExportList(ViewList.Cast<SaleReportByPartnerDetiled>().Select(s => new { Կոդ = s.Code, Անվանում = s.Description, Քանակ = s.Quantity, Գին = s.Price, Գումար = s.Quantity * s.Price }));
                    break;
                case ExportImportEnum.Xml:

                    var partner = Partners != null && Partners.Count == 1 ? Partners.First() : null;
                    var goodsInfos = ViewList.Cast<SaleReportByPartnerDetiled>().GroupBy(s => s.Code)
                                             .Select(s => new EsGoodInfo
                                             {
                                                 Code = s.First().Code,
                                                 Description = s.First().Description,
                                                 Unit = s.First().Mu,
                                                 Quantity = s.Sum(t => t.Quantity),
                                                 Price = s.First().Price,
                                                 Total = s.Sum(t => t.Quantity) * s.First().Price
                                             }).ToList();

                    var invoice = new EsMarketInvoice
                    {
                        InvoiceInfo = new InvoiceInfo { Type = EsMarket.SharedData.Enums.InvoiceTypeEnum.Sale },
                        SupplierInfo = new EsMarketPartner
                        {
                            Name = ApplicationManager.Settings.Branch.Name,
                            Address = ApplicationManager.Settings.Branch.Address,
                            Tin = ApplicationManager.Settings.Branch.Tin,
                            SupplyAddress = ApplicationManager.Settings.Branch.SupplyAddress
                        },
                        DeliveryInfo = new DeliveryInfo
                        {
                            DeliveryDate = DateTime.Today,
                            DeliveryMethod = EsMarket.SharedData.Enums.DeliveryTypeEnum.SelfeDelivery,
                            DeliveryLocation = new AddressModel { Address = partner != null ? partner.Address : "" }
                        },
                        BuyerInfo = partner != null ? new EsMarketPartner
                        {
                            Tin = partner.TIN,
                            Name = partner.FullName,
                            Address = partner.JuridicalAddress,
                            BankAccount = new BankAccount { BankName = partner.Bank, BankAccountNumber = partner.BankAccount }
                        } : null,
                        GoodsInfo = goodsInfos,
                        Notes = ""
                    };

                    InvoicesManager.ExportInvoiceToXmlAccDoc(invoice);
                    break;
                default: break;
            }



            //InvoicesManager.ExportInvoiceToXmlAccDoc();
        }

        #endregion
    }

    public class SaleInvoiceReportByStockDetiledViewModel : SaleInvoiceReportByPartnerViewModel
    {
        #region Constructors
        public SaleInvoiceReportByStockDetiledViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {

        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Վաճառք ըստ բաժինների մանրամասն";
        }

        protected override void OnExport(ExportImportEnum o)
        {
            //base.OnExport(o);
            ExcelExportManager.ExportList(ViewList.Select(s => new
            {
                Բաժին = ((SaleReportByPartnerDetiled)s).Partner,
                Ապրանքագիր = ((SaleReportByPartnerDetiled)s).Invoice,
                Ամսաթիվ = ((SaleReportByPartnerDetiled)s).Date,
                Կոդ = ((SaleReportByPartnerDetiled)s).Code,
                Անվանում = ((SaleReportByPartnerDetiled)s).Description,
                Չմ = ((SaleReportByPartnerDetiled)s).Mu,
                Քանակ = ((SaleReportByPartnerDetiled)s).Quantity,
                ԻՆքնարժեք = ((SaleReportByPartnerDetiled)s).Cost,
                Գին = ((SaleReportByPartnerDetiled)s).Price,
                Գումար = ((SaleReportByPartnerDetiled)s).Sale,
                Եկամուտ = ((SaleReportByPartnerDetiled)s).Profit,
                Եկամտի_Տոկոս = ((SaleReportByPartnerDetiled)s).Pers
            }));

        }

        #endregion
    }

    public class SaleInvoiceReportByChartViewModel : SaleInvoiceReportByPartnerViewModel
    {
        private ItemProperty _reportChartType;
        private readonly object _sync = new object();
        private List<IInvoiceReport> _invoiceReports;

        #region Internal properties
        #endregion Internal properties

        #region External proeprties
        public ObservableCollection<KeyValuePair<object, decimal>> ChartDatas { get; set; }
        public List<InvoiceModel> Invoices { get; set; }
        public List<ItemProperty> ReportChartTypes { get; set; }

        public ItemProperty ReportChartType
        {
            get { return _reportChartType; }
            set
            {
                _reportChartType = value; RaisePropertyChanged("ReportChartType");
                UpdateChartData();
            }
        }

        #endregion External properties

        #region Constructors
        public SaleInvoiceReportByChartViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            Title = Description = "Վաճառքի վերլուծություն";
            Invoices = new List<InvoiceModel>();
            ChartDatas = new ObservableCollection<KeyValuePair<object, decimal>>();
            ReportChartTypes = new List<ItemProperty> {
                new ItemProperty { Value = "Ժամվա", Key = 0 },
                new ItemProperty { Value = "Օրվա", Key = 1 },
                new ItemProperty { Value = "Շաբաթվա", Key = 2 },
                new ItemProperty { Value = "Ամսվա", Key = 3 },
                new ItemProperty { Value = "Տարվա", Key = 4 } };
            ReportChartType = ReportChartTypes.First();
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            if (DteIntermediate == null) { IsLoading = false; return; }
            lock (_sync)
            {
                _invoiceReports = InvoicesManager.GetInvoiceReports(DteIntermediate.Item1, DteIntermediate.Item2, new List<InvoiceType>() { InvoiceType.SaleInvoice });
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected void UpdateChartData()
        {
            lock (_sync)
            {
                if (ReportChartType == null || _invoiceReports == null)
                {
                    return;
                }
                decimal summ;
                int count;
                switch ((int)ReportChartType.Key)
                {
                    //ByHour
                    case 0:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByHour = _invoiceReports.OrderBy(s => s.Date.Hour).GroupBy(s => s.Date.Hour).ToList();
                        foreach (var invoice in invociesGroupByHour)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s => s.Date.Day).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.ToString("T"), summ / count));
                        }
                        break;
                    //ByDay
                    case 1:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByDay = _invoiceReports.OrderBy(s => s.Date.Day).GroupBy(s => s.Date.Day).ToList();

                        foreach (var invoice in invociesGroupByDay)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s => s.Date.Month).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Day, summ / count));
                        }
                        break;
                    //ByWeek
                    case 2:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByWeek = _invoiceReports.OrderBy(s => s.Date.DayOfWeek).GroupBy(s => s.Date.DayOfWeek).ToList();

                        foreach (var invoice in invociesGroupByWeek)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s => s.Date.Day).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.ToString("ddd"), summ / count));
                        }
                        break;
                    //ByMonth
                    case 3:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByMonth = _invoiceReports.OrderBy(s => s.Date.Month).GroupBy(s => s.Date.Month).ToList();
                        foreach (var invoice in invociesGroupByMonth)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.GroupBy(s => s.Date.Year).Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Month, summ / count));
                        }
                        break;
                    //ByYear
                    case 4:
                        ChartDatas.Clear();
                        if (!_invoiceReports.Any()) return;
                        var invociesGroupByYear = _invoiceReports.OrderBy(s => s.Date.Year).GroupBy(s => s.Date.Year);

                        foreach (var invoice in invociesGroupByYear)
                        {
                            if (!invoice.Any())
                            {
                                continue;
                            }
                            summ = invoice.Sum(s => s.Amount);
                            count = invoice.Count();
                            ChartDatas.Add(new KeyValuePair<object, decimal>(invoice.First().Date.Year, summ));
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion
    }

    public class PurchaseInvoiceReportByStocksViewModel : SaleInvoiceReportByPartnerViewModel
    {
        private List<StockModel> _stocks;

        public PurchaseInvoiceReportByStocksViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Գնում ըստ բաժինների";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            if (DteIntermediate == null) { IsLoading = false; return; }

            _stocks = GetStocks();
            if (!_stocks.Any()) { IsLoading = false; return; }

            var invoiceItems = InvoicesManager.GetPurchaseInvoiceItemsByStocksForReport(DteIntermediate.Item1, DteIntermediate.Item2, _stocks.Select(s => s.Id).ToList());
            List<IInvoiceReport> reports = new List<IInvoiceReport>(_stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (short)InvoiceType.PurchaseInvoice).Count(ii => ii.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.PurchaseInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.PurchaseInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                Sale = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.PurchaseInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.ProductPrice ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (short)InvoiceType.ReturnTo)).Sum(ii => (ii.Quantity ?? 0) * (ii.ProductPrice ?? 0)),
            }).ToList());
            reports.Add(new InvoiceReport
            {
                Description = "Ընդամենը",
                Count = reports.Sum(s => ((InvoiceReport)s).Count),
                Quantity = reports.Sum(s => s.Quantity),
                Cost = reports.Sum(s => s.Cost),
                ReturnAmount = reports.Sum(s => ((InvoiceReport)s).ReturnAmount),
                Sale = reports.Sum(s => s.Sale)
            });


            if (!reports.Any())
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։",
                    MessageTypeEnum.Information));
            }
            else
            {
                SetResult(reports);
            }

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }

        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            TotalCount = (double)ViewList.Sum(s => s.Quantity);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
        }

        protected override void OnExport(ExportImportEnum o)
        {
            //base.OnExport(o);
            ExcelExportManager.ExportList(ViewList.Select(s => new
            {
                Բաժին = ((InvoiceReport)s).Description,
                Ապրանքագիր = ((InvoiceReport)s).Count,
                Քանակ = ((InvoiceReport)s).Quantity,
                Հասույթ = ((InvoiceReport)s).Sale,
                ԻՆքնարժեք = ((InvoiceReport)s).Cost,
                Եկամուտ = ((InvoiceReport)s).Profit,
                Եկամտի_Տոկոս = ((InvoiceReport)s).Pers,
                Ետվերադարձ = ((InvoiceReport)s).ReturnAmount,
                Ընդամենը = ((InvoiceReport)s).Total
            }));

        }

        protected override void OnPrint(object o)
        {
            base.OnPrint(o);
        }
    }
    public class ProductsLogViewModel : TableViewModel
    {
        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("CostPrice");
            RaisePropertyChanged("Price");
            RaisePropertyChanged("ItemsView");
            RaisePropertyChanged("ViewList");
            RaisePropertyChanged("TotalRows");
            RaisePropertyChanged("TotalCount");
            RaisePropertyChanged("Total");
            RaisePropertyChanged("View");
        }
        protected override void Initialize()
        {
            base.Initialize();
            Title = "Ապրանքների խմբագրման պատմություն";
        }


        protected override void OnUpdate()
        {
            base.OnUpdate();
            var productsLog = ProductsManager.GetProductsLog(StartEndDate);
            Items.Clear();
            Items.AddRange(productsLog);
            OnItemsChanged(null, null);
        }
    }

}
