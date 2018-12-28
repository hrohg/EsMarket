using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using UserControls.ControlPanel.Controls;
using UserControls.Models;
using UserControls.ViewModels;
using UserControls.ViewModels.Reports;
using UserControls.Views;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace ES.Market.Views.Reports.View
{
    /// <summary>
    /// Interaction logic for Reports.xaml
    /// </summary>
    public partial class DataReports : Window
    {

        public DataReports()
        {
            InitializeComponent();
        }
        public const string Description = "Հաշվետվություններ";
        #region Private methods
        private void LoadTab<T>(TableViewModel<T> model, string header)
        {
            int nextTab;

            var viewTable = new UctrlViewTable(model);
            //viewTable.DataContext = model;
            if (model != null && model.ViewList.Any())
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
            var newTab = new TabItem
            {
                Header = model.Title,
                Content = viewTable,
                AllowDrop = true
            };
            nextTab = TabReport.Items.Add(newTab);
            newTab.DataContext = model;
            model.OnClosed += CloseActiveTab;
            TabReport.SelectedIndex = nextTab;
        }

        private void CloseActiveTab(PaneViewModel vm)
        {
            vm.OnClosed -= CloseActiveTab;
            var tab = TabReport.Items.SourceCollection.OfType<TabItem>().FirstOrDefault(s => s.DataContext == vm);
            if (tab != null)
            {
                TabReport.Items.Remove(tab);
            }
        }
        //private void LoadCartTab(List<InvoiceModel> invoices, string header)
        //{
        //    int nextTab;

        //    nextTab = TabReport.Items.Add(new TabItem
        //    {
        //        Header = header,
        //        Content = new UctrlChartLine(invoices),
        //        AllowDrop = true
        //    });
        //    TabReport.SelectedIndex = nextTab;
        //}
        #endregion

        #region FinanceReportUctrl Events
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        void MiViewProductReportByProviders_OnClick(object sender, EventArgs e)
        {
            var products = SelectItemsManager.SelectProduct(true);
            var dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
            if (dateIntermediate == null) return;

            var invoiceItems = InvoicesManager.GetInvoiceItemsByCode(products.Select(s => s.Code), dateIntermediate.Item1, dateIntermediate.Item2, ApplicationManager.Member.Id).OrderBy(s => s.InvoiceId);
            var invoices = InvoicesManager.GetInvoices(invoiceItems.Select(s => s.InvoiceId).Distinct());
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
            //if (!list.Any())
            //{
            //    MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
            //    return;
            //}
            var viewModel = new ViewProductsByProviderViewModel(list) { IsClosable = true, Title = "Ապրանքների գնում ըստ մատակարարների" };
            //viewModel.SetItems(list);
            LoadTab(viewModel, "ReportByStocks");

        }

        //protected void MiViewSaleReportDetile_Click(object sender, EventArgs e)
        //{
        //    var dateIntermediate = SelectManager.GetDateIntermediate();
        //    if (dateIntermediate == null) return;

        //    var invoices = InvoicesManager.GetInvoices(dateIntermediate.Item1, dateIntermediate.Item2)
        //            .Where(s => s.InvoiceTypeId == (long)InvoiceType.SaleInvoice).ToList();

        //    if (!invoices.Any())
        //    {
        //        MessageManager.OnMessage(new MessageModel(DateTime.Now, "Ոչինչ չի հայտնաբերվել։", MessageTypeEnum.Information));
        //        return;
        //    }
        //    LoadCartTab(invoices, "ReportByStocks");

        //}
    }
}
