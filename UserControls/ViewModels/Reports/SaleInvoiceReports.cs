﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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

namespace UserControls.ViewModels.Reports
{
    public class SaleInvoiceReportsBase<T> : TableViewModel<T>
    {
        protected readonly ViewInvoicesEnum ViewInvoicesEnum;

        public SaleInvoiceReportsBase(ViewInvoicesEnum viewInvoicesEnum)
        {
            ViewInvoicesEnum = viewInvoicesEnum;
        }
    }

    public class SaleInvoiceReportTypeViewModel : SaleInvoiceReportsBase<IInvoiceReport>
    {
        #region Internal properties

        protected Tuple<DateTime, DateTime> DateIntermediate;
        #endregion

        #region External properties

        #endregion

        #region Constructors
        public SaleInvoiceReportTypeViewModel(ViewInvoicesEnum viewInvoicesEnum) : base(viewInvoicesEnum)
        {
            IsShowUpdateButton = true;
        }
        #endregion

        #region Internal methods
        protected override void Initialize()
        {
            Title = Description = "0-ական վաճառքների դիտում";
        }

        protected override void UpdateAsync()
        {
            base.UpdateAsync();
            GetDate();
            if (DateIntermediate == null) { UpdateStopped(); return; }
            var reports = GetReports();
            SetResult(reports);
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { UpdateCompleted(); });
        }
        protected override List<IInvoiceReport> GetReports()
        {
            List<IInvoiceReport> reports = base.GetReports();
            Description = string.Format("{0} {1} - {2}", Title, DateIntermediate.Item1.Date, DateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");
            List<PartnerModel> partners = null;
            List<PartnerType> partnerTypes = null;
            List<StockModel> stocks = null;
            List<InvoiceModel> invoices;
            switch (ViewInvoicesEnum)
            {
                case ViewInvoicesEnum.None:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.CashManager.GetStocks, true).ToList(); }));
                    reports = new List<IInvoiceReport>(UpdateByStock(stocks, DateIntermediate));
                    break;
                case ViewInvoicesEnum.ByProvider:
                case ViewInvoicesEnum.ByPartner:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartners(DateIntermediate.Item1, DateIntermediate.Item2, ViewInvoicesEnum == ViewInvoicesEnum.ByProvider ? InvoiceType.PurchaseInvoice : InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partnerTypes = SelectItemsManager.SelectPartnersTypes(true);
                    }));

                    if (partnerTypes != null && partnerTypes.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnerTypes(DateIntermediate.Item1, DateIntermediate.Item2, InvoiceType.SaleInvoice, partnerTypes, ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnersDetiled(DateIntermediate.Item1, DateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList()));
                    }
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.CashManager.GetStocks, true).ToList(); }));

                    var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(DateIntermediate.Item1, DateIntermediate.Item2).Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice).Select(s => s.Id).ToList(), stocks);

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
                    invoices = InvoicesManager.GetInvoices(DateIntermediate.Item1, DateIntermediate.Item2)
                            .Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice).ToList();
                    if (!invoices.Any())
                    {
                        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                    }
                    break;
                case ViewInvoicesEnum.ByZeroAmunt:
                    invoices = InvoicesManager.GetInvoices(DateIntermediate.Item1, DateIntermediate.Item2).Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice && s.Amount == 0).ToList();
                    if (!invoices.Any())
                    {
                        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                        break;
                    }
                    reports = new List<IInvoiceReport>(invoices.Select(s =>
                        new InvoiceReport()
                        {
                            Description = s.InvoiceNumber,
                            Date = s.CreateDate,
                            Cost = InvoicesManager.GetInvoiceCost(s.Id),
                            Sale = InvoicesManager.GetInvoiceTotal(s.Id),
                        }).ToList());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return reports;
        }
        protected override void UpdateCompleted(bool isSuccess = true)
        {
            base.UpdateCompleted(isSuccess);
            TotalCount = (double)ViewList.Sum(s => s.Quantity);
            Total = (double)ViewList.Sum(i => i.Sale ?? 0);
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

        private List<InvoiceReport> UpdateByStock(List<StockModel> stocks, Tuple<DateTime, DateTime> dateIntermediate)
        {
            var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (short)InvoiceType.SaleInvoice || s.InvoiceTypeId == (short)InvoiceType.ReturnFrom).Select(s => s.Id).ToList(), stocks);
            var list = stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Count(ii => ii.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.CostPrice ?? 0)),
                Sale = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0))
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
        protected void GetDate()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                DateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            });
        }
        #endregion
    }

    public class PurchaseInvoiceReportViewModel : SaleInvoiceReportTypeViewModel
    {
        public PurchaseInvoiceReportViewModel(ViewInvoicesEnum viewInvoicesEnum) : base(viewInvoicesEnum)
        {
        }
    }

    public class SaleInvoiceReportViewModel : SaleInvoiceReportTypeViewModel
    {
        public SaleInvoiceReportViewModel(ViewInvoicesEnum viewInvoicesEnum) : base(viewInvoicesEnum)
        {
        }
        #region Internal methods
        protected override void Initialize()
        {
            base.Initialize();
            Title = Description = "Վաճառք ըստ մատակարարի";
        }
        protected override List<IInvoiceReport> GetReports()
        {
            List<IInvoiceReport> reports = null; //base.GetReports();
            Description = string.Format("{0} {1} - {2}", Title, DateIntermediate.Item1.Date, DateIntermediate.Item2.Date);
            RaisePropertyChanged(() => Description);
            List<PartnerModel> partners = null;
            
            List<InvoiceModel> invoices;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                partners = SelectItemsManager.SelectPartners(true);
            }));

            if (partners != null && partners.Count > 0)
            {
                reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByProviders(DateIntermediate.Item1, DateIntermediate.Item2, ViewInvoicesEnum == ViewInvoicesEnum.ByProvider ? InvoiceType.PurchaseInvoice : InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ApplicationManager.Instance.GetMember.Id));
            }

            return reports;
        }
        #endregion
    }
}
