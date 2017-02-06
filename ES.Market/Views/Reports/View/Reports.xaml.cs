using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Model;
using ES.Data.Models;
using ES.Data.Models.Reports;
using ES.DataAccess.Models;
using ES.Shop.Controls;
using UserControls.ControlPanel.Controls;
using UserControls.ViewModels;
using UserControls.Views;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace ES.Market.Views.Reports.View
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class DataReports : Window
    {
        private List<MembersRoles> _userRoles;
        public DataReports(EsUserModel user, EsMemberModel member)
        {
            InitializeComponent();
            _member = member;
            _user = user;
            if (_member == null || _user == null)
            {
                return;
            }
            _userRoles = UsersManager.GetUserRoles(_user.UserId, _member.Id);
            SetPermitions();
        }
        private void SetPermitions()
        {
            var stocks = StockManager.GetStocks(_member.Id);
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Admin") != null)
            {
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Director") != null)
            {
                MiSale.Visibility = MiProducts.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Manager") != null)
            {
                MiSale.Visibility = MiProducts.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "SaleManager") != null)
            {
                MiSale.Visibility = MiProducts.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Storekeeper") != null)
            {
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Seller") != null)
            {
            }
        }

        public const string Description = "Հաշվետվություններ";
        private readonly EsMemberModel _member;
        private readonly EsUserModel _user;
        #region Private methods
        private void LoadTab<T>(TableViewModel<T> model, string header)
        {
            int nextTab;

            var viewTable = new UctrlViewTable(model);
            //viewTable.DataContext = model;
            if (model != null)
            {
                var columns = model.ViewList.First().GetType().GetProperties();
                foreach (var column in columns.Select(item => new DataGridTextColumn
                {
                    Header = item.Name,
                    Binding = new Binding(item.Name)
                }))
                {
                    viewTable.DgTable.Columns.Add(column);
                }
                viewTable.DgTable.Visibility = Visibility.Visible;
            }
            nextTab = TabReport.Items.Add(new TabItem
                    {
                        Header = header,
                        Content = viewTable,
                        AllowDrop = true
                    });
            TabReport.SelectedIndex = nextTab;
        }
        private void LoadCartTab(List<InvoiceModel> invoices, string header)
        {
            int nextTab;

            nextTab = TabReport.Items.Add(new TabItem
            {
                Header = header,
                Content = new UctrlChartLine(invoices),
                AllowDrop = true
            });
            TabReport.SelectedIndex = nextTab;
        }
        #endregion
        #region FinanceReportUctrl Events
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
        void MiViewSaleReportByStocks_Click(object sender, EventArgs e)
        {
            var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(_member.Id), true);
            var dateIntermediate = SelectManager.GetDateIntermediate();
            var invoiceItems =
                InvoicesManager.GetInvoiceItemsByStocks(
                InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2, _member.Id)
                    .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice)
                        .Select(s => s.Id), stocks.Select(t => t.Id));
            var list = stocks.Select(s =>
                        new InvoiceReport()
                        {
                            Description = s.FullName,
                            Count = invoiceItems.Count(ii => ii.ProductItem.StockId == s.Id),
                            Quantity = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id).Sum(ii => ii.Quantity ?? 0),
                            Cost = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id).Sum(ii => ii.Quantity * ii.CostPrice ?? 0),
                            Sale = invoiceItems.Where(ii => ii.ProductItem.StockId == s.Id).Sum(ii => ii.Quantity * ii.Price ?? 0)
                        }).ToList();
            list.Add(new InvoiceReport
            {
                Description = "Ընդամենը",
                Count = list.Sum(s => s.Count),
                Quantity = list.Sum(s => s.Quantity),
                Cost = list.Sum(s => s.Cost),
                Sale = list.Sum(s => s.Sale)
            });
            if (list.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                return;
            }
            var viewModel = new TableViewModel<InvoiceReport>(list);
            //viewModel.SetItems(list);
            LoadTab(viewModel, "ReportByStocks");

        }
        void MiViewSaleReportDetileByStocks_Click(object sender, EventArgs e)
        {
            var stocksIds = SelectItemsManager.SelectStocks(StockManager.GetStocks(_member.Id), true).Select(t => t.Id);
            var dateIntermediate = SelectManager.GetDateIntermediate();
            var invoiceItems =
                InvoicesManager.GetInvoiceItemsByStocks(
                InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2, _member.Id)
                    .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice)
                        .Select(s => s.Id), stocksIds);
            var list = invoiceItems.GroupBy(s => s.Code).Select(s =>
                        new InvoiceReportModel()
                        {
                            Code = s.First().Code,
                            Description = s.First().Description,
                            Mu = s.First().Mu,
                            Quantity = s.Sum(t => t.Quantity) ?? 0,
                            Price = s.First().Price ?? 0,
                            CostPrice = s.First().CostPrice ?? 0
                        }).ToList();
            if (list.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                return;
            }
            var viewModel = new TableViewModel<InvoiceReportModel>(list){IsShowCloseButton = true};
            LoadTab(viewModel, "ReportByStocks");

        }
        void MiViewProductReportByProviders_OnClick(object sender, EventArgs e)
        {
            var products = SelectItemsManager.SelectProduct(true);
            var dateIntermediate = SelectManager.GetDateIntermediate();
            var invoiceItems = InvoicesManager.GetInvoiceItemssByCode(products.Select(s => s.Code), dateIntermediate.Item1, dateIntermediate.Item2, _member.Id).OrderBy(s => s.InvoiceId);
            var invoices = InvoicesManager.GetInvoices(invoiceItems.Select(s => s.InvoiceId).Distinct(), _member.Id);
            var list = invoiceItems.Select(s =>
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
                        }).ToList();
            if (list.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                return;
            }
            var viewModel = new TableViewModel<ProductProviderReportModel>(list);
            //viewModel.SetItems(list);
            LoadTab(viewModel, "ReportByStocks");

        }

        protected void MiViewSaleReportDetile_Click(object sender, EventArgs e)
        {
            var dateIntermediate = SelectManager.GetDateIntermediate();
            var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2, _member.Id)
                    .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).ToList();

            if (!invoices.Any())
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                return;
            }
            LoadCartTab(invoices, "ReportByStocks");

        }
    }
}
