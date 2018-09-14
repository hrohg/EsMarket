using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Data.Models.Reports;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;

namespace UserControls.ViewModels.Reports
{
    public class SaleInvoiceReportsBase<T> : TableViewModel<T>
    {
        protected readonly ViewInvoicesEnum ViewInvoicesEnum;
        protected List<T> _items = new List<T>();

        public override ObservableCollection<T> ViewList
        {
            get { return new ObservableCollection<T>(_items); }
            protected set
            {
                _items = value.ToList();
                RaisePropertyChanged("ViewList");
                RaisePropertyChanged("Count");
                RaisePropertyChanged("Total");
            }
        }

        public SaleInvoiceReportsBase(ViewInvoicesEnum viewInvoicesEnum)
        {
            ViewInvoicesEnum = viewInvoicesEnum;
        }
    }

    public class SaleInvoiceReportTypeViewModel : SaleInvoiceReportsBase<IInvoiceReport>
    {
        #region Internal properties

        private Tuple<DateTime, DateTime> _dateIntermediate;
        #endregion

        #region External properties

        #endregion

        #region Constructors
        public SaleInvoiceReportTypeViewModel(ViewInvoicesEnum viewInvoicesEnum)
            : base(viewInvoicesEnum)
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
            List<IInvoiceReport> reports = null;
            List<PartnerModel> partners = null;
            List<PartnerType> partnerTypes = null;
            List<StockModel> stocks = null;
            List<InvoiceModel> invoices = null;
            switch (ViewInvoicesEnum)
            {
                case ViewInvoicesEnum.None:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    break;
                case ViewInvoicesEnum.ByStock:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.Instance.CashManager.GetStocks, true).ToList(); }));
                    reports = new List<IInvoiceReport>(UpdateByStock(stocks, _dateIntermediate));
                    break;
                case ViewInvoicesEnum.ByPartner:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartners(_dateIntermediate.Item1, _dateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList(), ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnerType:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partnerTypes = SelectItemsManager.SelectPartnersTypes(true);
                    }));

                    if (partnerTypes != null && partnerTypes.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnerTypes(_dateIntermediate.Item1, _dateIntermediate.Item2, InvoiceType.SaleInvoice, partnerTypes, ApplicationManager.Instance.GetMember.Id));
                    }
                    break;
                case ViewInvoicesEnum.ByPartnersDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        partners = SelectItemsManager.SelectPartners(true);
                    }));

                    if (partners != null && partners.Count > 0)
                    {
                        reports = new List<IInvoiceReport>(InvoicesManager.GetSaleInvoicesReportsByPartnersDetiled(_dateIntermediate.Item1, _dateIntermediate.Item2, InvoiceType.SaleInvoice, partners.Select(s => s.Id).ToList()));
                    }
                    break;
                case ViewInvoicesEnum.ByStocksDetiles:
                    Application.Current.Dispatcher.Invoke(new Action(() => { stocks = SelectItemsManager.SelectStocks(ApplicationManager.Instance.CashManager.GetStocks, true).ToList(); }));

                    var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(_dateIntermediate.Item1, _dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).Select(s => s.Id).ToList(), stocks);

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
                    invoices = InvoicesManager.GetInvoices(_dateIntermediate.Item1, _dateIntermediate.Item2)
                            .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).ToList();
                    if (!invoices.Any())
                    {
                        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
                        break;
                    }

                    break;
                case ViewInvoicesEnum.ByZeroAmunt:
                    invoices = InvoicesManager.GetInvoices(_dateIntermediate.Item1, _dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice && s.Amount == 0).ToList();
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
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                ViewList = new ObservableCollection<IInvoiceReport>(reports);
            }
            if (reports != null)
            {
                TotalRows = reports.Count;
                TotalCount = (double)reports.Sum(s => s.Quantity);
                Total = (double)reports.Sum(i => i.Sale ?? 0);
            }
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);

        }

        private void InitializeReport(List<InvoiceReport> reports)
        {
            if (reports == null || reports.Count == 0)
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            }
            else
            {
                ViewList = new ObservableCollection<IInvoiceReport>(reports);
            }
            if (reports != null)
            {
                TotalRows = reports.Count;
                TotalCount = (double)reports.Sum(s => s.Quantity);
                Total = (double)reports.Sum(i => i.Sale ?? 0);
            }
            IsLoading = false;
            RaisePropertyChanged(IsInProgressProperty);
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            Tuple<DateTime, DateTime> dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;
            Description = string.Format("{0} {1} - {2}", Title, dateIntermediate.Item1.Date, dateIntermediate.Item2.Date);
            RaisePropertyChanged("Description");
            base.OnUpdate();
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
            var invoiceItems = InvoicesManager.GetInvoiceItemsByStocks(InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2).Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice || s.InvoiceTypeId == (long)InvoiceType.ReturnFrom).Select(s => s.Id).ToList(), stocks);
            var list = stocks.Select(s => new InvoiceReport
            {
                Description = s.FullName,
                Count = invoiceItems.Where(ii => ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice).Count(ii => ii.ProductItem.StockId == s.Id),
                Quantity = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0)),
                Cost = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.CostPrice ?? 0)),
                Sale = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.SaleInvoice)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0)),
                ReturnAmount = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id && (ii.Invoice.InvoiceTypeId == (int)InvoiceType.ReturnFrom)).Sum(ii => (ii.Quantity ?? 0) * (ii.Price ?? 0))
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

        #endregion
    }
}
