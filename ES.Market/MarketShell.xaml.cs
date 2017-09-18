using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.Market.Controls;
using ES.Market.Edit;
using ES.Market.ViewModels;
using ES.Market.Views;
using ES.MsOffice.ExcelManager;
using ES.Shop.Controls;
using UserControls.PriceTicketControl;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl.ViewModels;
using UserControls.ViewModels;
using UserControls.Views.CustomControls;
using ExportManager = ES.Business.Managers.ExportManager;
using MessageBox = System.Windows.MessageBox;
using UserControlSession = UserControls.Session;
using ItemsToSelect = UserControls.ControlPanel.Controls.ItemsToSelect;
using SelectItems = UserControls.ControlPanel.Controls.SelectItems;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace ES.Market
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MarketShell : Window
    {
        #region Private methods
        private void LoadPackingListTab(long invoiceTypeId, bool? isAccepted = null)
        {
            var invoice = SelectItemsManager.SelectInvoice(invoiceTypeId, isAccepted, false).FirstOrDefault();
            if (invoice == null)
            {
                MessageBox.Show("Ապրանքագիր չի հայտնաբերվել։");
                return;
            }
            var nextTab = TabShop.Items.Add(new TabItem
            {
                Header = "Ապրանքների ցուցակ" + (" - " + invoice.InvoiceNumber),
                Content = new PackingListUctrl(invoice, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetMember),
                AllowDrop = true
            });
            TabShop.SelectedIndex = nextTab;
        }
        private void AddTabItem(string header, object content, bool allowDrop)
        {
            var nextTab = TabShop.Items.Add(new TabItem
            {
                Header = header,
                Content = content,
                AllowDrop = allowDrop
            });
            TabShop.SelectedIndex = nextTab;
        }
        #endregion

        public MarketShell()
        {
            if(DataContext==null)
            MessageBox.Show("Ոչ արտոնագրված գործողություն։ Ծրագիրը կփակվի։ Խնդրում ենք արտոնագրել ծրագրային ապահովումը։",
                 "Արտոնագրային սխալ", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
            //InitializeComponent();
        }
        public MarketShell(ShellViewModel vm)
        {
            DataContext = vm;
            if (ApplicationManager.Instance.GetMember == null || ApplicationManager.GetEsUser == null)
            {
                MessageBox.Show("Ոչ արտոնագրված գործողություն։ Ծրագիրը կփակվի։ Խնդրում ենք արտոնագրել ծրագրային ապահովումը։",
                 "Արտոնագրային սխալ", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            if (ApplicationManager.Instance.UserRoles == null || !ApplicationManager.Instance.UserRoles.Any())
            {
                MessageBox.Show("Ծրագիրը կփակվի, քանի որ դուք ոչ մի համակարգում ընդգրկված չեք։ Ընդգրկվելու համար խնդրում ենք դիմել տնօրինություն։ ");
                Close();
                return;
            }

            if (ApplicationManager.Instance.GetMember == null || ApplicationManager.GetEsUser == null)
            {
                this.Close();
                return;
            }
            UserControlSession.Inst.BEManager = DataManager.GetInstance();

            InitializeComponent();
        }

        private void WinShop_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var items = TabShop.Items;
            if (items != null)
            {
                foreach (TabItem item in items)
                {
                    var tab = item.DataContext as ITabItem;
                    if(tab==null) continue;
                    if (tab.IsModified &&
                        MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք փակել ծրագիրը:\n" +
                                        "Եթե չհիշած ապրանքագրեր կան տվյալները չեն պահպանվի։ Խնդրում ենք համոզվել որ բոլոր տվյալները պահպանված են։",
                    "Աշխատանքի ավարտ",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }

        }

        #region Menu items
        
        #region Admin
        private void MiExecute_Click(object sender, EventArgs e)
        {
            ExecuteManager.ExecuteTest();
        }
       
        #endregion
        
        #region Product order
        protected void MiProductOrderByMinQuantity_Click(object sender, EventArgs e)
        {
            var products = ProductsManager.GetProductOrderByProduct(ApplicationManager.Instance.GetMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var ui =
                new UIListView(products.Select(
                        s =>
                            new
                            {
                                Կոդ = s.Code,
                                Անվանում = s.Description,
                                Առկա = s.ExistingQuantity,
                                Պահանջարկ = s.Quantity,
                                Գին = s.Price,
                                Գումար = s.Quantity * s.Price
                            }), "Անհրաժեշտ ապրանքների ցուցակ");
            ui.Show();
        }
        protected void MiProductOrderByProviders_Click(object sender, EventArgs e)
        {
            var products = ProductsManager.GetProductOrderByProduct(ApplicationManager.Instance.GetMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var partners = PartnersManager.GetPartner();
            var ui =
                new UIListView(products.Select(
                        s =>
                        {
                            var firstOrDefault = partners.FirstOrDefault(t => t.Id == s.ProviderId);
                            return new
                                 {
                                     Կոդ = s.Code,
                                     Անվանում = s.Description,
                                     Առկա = s.ExistingQuantity,
                                     Պահանջարկ = s.Quantity,
                                     Ինքնարժեք = s.CostPrice,
                                     Գին = s.Price,
                                     Արժեք = s.Quantity * s.CostPrice,
                                     Գումար = s.Quantity * s.Price,
                                     Մատակարար = firstOrDefault != null ? firstOrDefault.FullName : ""
                                 };
                        }), "Անհրաժեշտ ապրանքների ցուցակ");
            ui.Show();
        }
        protected void MiProductOrderByBrands_Click(object sender, EventArgs e)
        {
            var brands = SelectItemsManager.SelectBrands(ProductsManager.GetBrands(ApplicationManager.Instance.GetMember.Id), true);
            var products = ProductsManager.GetProductOrderByBrands(brands, ApplicationManager.Instance.GetMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var ui =
                new UIListView(products.Select(
                        s =>
                            new
                            {
                                Կոդ = s.Code,
                                Անվանում = s.Description,
                                Առկա = s.ExistingQuantity,
                                Պահանջարկ = s.Quantity,
                                Ինքնարժեք = s.CostPrice,
                                Գին = s.Price,
                                Արժեք = s.Quantity * s.CostPrice,
                                Գումար = s.Quantity * s.Price,
                            }), "Անհրաժեշտ ապրանքների ցուցակ");
            ui.Show();
        }

        #endregion

        #region Cash desk
        
        //todo
        private PartnerModel SelectPartner(PartnerType partnerTypeId = 0)
        {
            var partners = partnerTypeId != 0 ? PartnersManager.GetPartner(partnerTypeId) : PartnersManager.GetPartners();
            if (partners.Count == 0) return null;
            var selectedItems =
                new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName + " " + s.Mobile, SelectedValue = s.Id }).ToList(), false);
            selectedItems.ShowDialog();
            if (selectedItems.DialogResult == null || selectedItems.DialogResult != true || selectedItems.SelectedItems == null)
            { return null; }
            return partners.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id));

        }
        #endregion

        #region AccountingRecords
        
        //251
        protected void MiTransferIntoCashDesk_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords.Credit = (long)AccountingPlanEnum.CashDesk;
            var fromCashDesk = SelectItemsManager.SelectCashDesks(null, false, "Ընտրել ելքագրվող դրամարկղը").FirstOrDefault();
            var toCashDesk = SelectItemsManager.SelectCashDesks(null, false, "Ընտրել մուտքագրվող դրամարկղը").FirstOrDefault();
            if (fromCashDesk == null || toCashDesk == null)
            {
                MessageBox.Show("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            if (fromCashDesk.Id == toCashDesk.Id)
            {
                MessageBox.Show("Հնարավոր չէ տեղափոխել նույն դրամարկղում։", "Սխալ տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = toCashDesk.Id;
            accountingRecords.CreditGuidId = fromCashDesk.Id;
            var description = "Ելքագրվող դրամարկդ։ " + fromCashDesk.Name + " առկա է։ " + fromCashDesk.Total + " դր․" + "\n" +
                "մուտքագրվող դրամարկդ։ " + toCashDesk.Name + " առկա է։ " + toCashDesk.Total + " դր․";
            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, description));
            ctrlAccountingRecords.ShowDialog();

            var repaymentAccountingRecord = ctrlAccountingRecords.AccountingRecord;
            if (!ctrlAccountingRecords.Result || repaymentAccountingRecord == null ||
                repaymentAccountingRecord.Amount == 0) return;
            if (accountingRecords.Amount > fromCashDesk.Total)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան առկա է։", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            accountingRecords.DebitGuidId = toCashDesk.Id;
            accountingRecords.CreditGuidId = fromCashDesk.Id;
            
            if (AccountingRecordsManager.SetCashTransfer(accountingRecords, ApplicationManager.Instance.GetMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //311
        protected void MiAddEquityBase_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords.Credit = (long)AccountingPlanEnum.EquityBase;
            var fromUser = SelectItemsManager.SelectUser(ApplicationManager.Instance.GetMember.Id, false, "Ընտրել ներդրող").FirstOrDefault();
            var toCashDesk = SelectItemsManager.SelectCashDesks(null, false, "Ընտրել մուտքագրվող դրամարկղը").FirstOrDefault();
            if (fromUser == null || toCashDesk == null)
            {
                MessageBox.Show("Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = toCashDesk.Id;
            accountingRecords.CreditLongId = fromUser.UserId;
            var description = "Ներդրող։ " + fromUser.FullName + "\n" +
                "Մուտքագրվող դրամարկդ։ " + toCashDesk.Name + " առկա է։ " + toCashDesk.Total + " դր․";
            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, description));
            ctrlAccountingRecords.ShowDialog();

            var repaymentAccountingRecord = ctrlAccountingRecords.AccountingRecord;
            if (!ctrlAccountingRecords.Result || repaymentAccountingRecord == null ||
                repaymentAccountingRecord.Amount == 0) return;
            accountingRecords.DebitGuidId = toCashDesk.Id;
            accountingRecords.CreditLongId = fromUser.UserId;
            if (AccountingRecordsManager.SetEquityBase(accountingRecords, ApplicationManager.Instance.GetMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        
        

        //712
        protected void MiCostOfSales_Click(object sender, EventArgs e)
        {
            new CtrlSingleAccountingRecords((int)AccountingPlanEnum.CostOfSales, (int)AccountingPlanEnum.CashDesk).Show();
        }

        protected void MiCostOfSales_Salary_Click(object sender, EventArgs e)
        {
            var costOfSales = SelectItemsManager.SelectSubAccountingPlan(
                SubAccountingPlanManager.GetSubAccountingPlanModels((long)AccountingPlanEnum.Debit_For_Salary, ApplicationManager.Instance.GetMember.Id, true), false, "Ընտրել");
            var debitForSalary = SelectItemsManager.SelectSubAccountingPlan(
                SubAccountingPlanManager.GetSubAccountingPlanModels((long)AccountingPlanEnum.Debit_For_Salary, ApplicationManager.Instance.GetMember.Id, true), false, "Ընտրել աշխատակից");
            var cashDesk = SelectItemsManager.SelectCashDesks(true, false);

            if (costOfSales.First() == null || debitForSalary.First() == null || cashDesk.First() == null)
            {
                return;
            }

            var accountingRecords1 = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords1.Debit = (long)AccountingPlanEnum.CostOfSales;
            accountingRecords1.DebitGuidId = costOfSales.First().Id;
            accountingRecords1.Credit = (long)AccountingPlanEnum.Debit_For_Salary;
            accountingRecords1.CreditGuidId = debitForSalary.First().Id;

            var accountingRecords2 = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords2.Debit = (long)AccountingPlanEnum.Debit_For_Salary;
            accountingRecords2.DebitGuidId = debitForSalary.First().Id;
            accountingRecords2.Credit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords2.CreditGuidId = cashDesk.First().Id;

            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingPlanEnum.CostOfSales;
            accountingRecords.DebitGuidId = debitForSalary.First().Id;
            accountingRecords.Credit = (long)AccountingPlanEnum.CashDesk;
            accountingRecords.CreditGuidId = cashDesk.First().Id;

            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, ""), false, false);
            ctrlAccountingRecords.ShowDialog();
            var repaymentAccountingRecord = ctrlAccountingRecords.AccountingRecord;
            if (!ctrlAccountingRecords.Result || repaymentAccountingRecord == null ||
                repaymentAccountingRecord.Amount == 0) return;
            accountingRecords1.Amount = accountingRecords2.Amount = accountingRecords.Amount;
            accountingRecords1.Description = accountingRecords2.Description = accountingRecords.Description;
        }
        #endregion

        #region AccountingAccounts

        protected void MiSingleAccountingRecords_Click(object sender, EventArgs e)
        {
            new CtrlSingleAccountingRecords((int)AccountingPlanEnum.CostOfSales, (int)AccountingPlanEnum.CashDesk).Show();
        }
        protected void MiMultipleAccountingRecords_Click(object sender, EventArgs e)
        {
            new CtrlMultipleAccountingRecords().Show();
        }
        private void MiDebitByPartners_Click(object sender, EventArgs e)
        {
            var debits = PartnersManager.GetPartners();
            var ui = new UIListView(debits.Where(s => s.Debit > 0 || s.Credit > 0).Select(s => new { Գործընկեր = s.FullName, Կանխավճար = s.Credit, Պարտք = s.Debit }).ToList());
            ui.Show();
        }
        protected void MiDebitDetile_Click(object sender, EventArgs e)
        {
            var partner = SelectPartner();
            if (partner == null) return;
            var accountReceivable = PartnersManager.GetAccountsReceivable(partner.Id);
            var list = accountReceivable.Select(s => new
            {
                Ամսաթիվ = s.Date.ToString("dd.MM.yyyy"),
                Պարտք = s.Amount,
                Վճարված = s.PaidAmount,
                Վերջնաժամկետ = s.ExpairyDate != null ? ((DateTime) s.ExpairyDate).ToString("dd.MM.yyyy") : string.Empty
            }).ToList();
            var ui = new UIListView(list, partner.FullName, (double)list.Sum(s=>s.Պարտք));
            ui.Show();
        }
        #endregion

        #region View
        /// <summary>
        /// Sale Packing List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MiViewSaleInvoices_Click(object sender, RoutedEventArgs e)
        {
            LoadPackingListTab((long)InvoiceType.SaleInvoice, true);
        }
        private void MiViewMoveInvoices_Click(object sender, RoutedEventArgs e)
        {
            LoadPackingListTab((long)InvoiceType.MoveInvoice, true);
        }
        private void MiViewUnacceptedPurchaseInvoices_Click(object sender, RoutedEventArgs e)
        {
            LoadPackingListTab((long)InvoiceType.PurchaseInvoice, false);
        }
        private void MiViewUnacceptedMoveInvoices_Click(object sender, RoutedEventArgs e)
        {
            LoadPackingListTab((long)InvoiceType.MoveInvoice, false);
        }
        
        #endregion

        #region Edit
        protected void MiManageServices_Click(object sender, EventArgs e)
        {
            var nextTab = TabShop.Items.Add(new TabItem
            {
                Header = "Ծառայությունների խմբագրում",
                Content = new UctrlEditServices(),
                AllowDrop = true

            });
            TabShop.SelectedIndex = nextTab;
        }
        #endregion

        #region Control Panel

        protected void MiBackupData_Click(object sender, EventArgs e)
        {
            new DatabaseManagement.MainWindow().ShowDialog();
        }
        protected void MiManageBrands_Click(object sender, EventArgs e)
        {
            MessageManager.OnMessage(EditManager.UpdateBrandsManager(ApplicationManager.Instance.GetMember.Id)
                ? new MessageModel(DateTime.Now, "Խմբագրումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success)
                : new MessageModel(DateTime.Now, "Խմբագրումը ձախողվել է։", MessageTypeEnum.Warning));
        }

        protected void MiUpdateMainProvisions_Click(object sender, EventArgs e)
        {
            if (UpdateManager.UpdateMainProvisions())
            {
                MessageBox.Show("Թարմացումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Թարմացումն ընդհատվել է։");
            }
        }

        protected void MiManageSubAccountingPlans_Click(object sender, EventArgs e)
        {
            new WinEditSubAccountinPlans().Show();
        }
        #endregion

        #region Releate
        #region PriceList

        protected void MiExportPricelistShtrikhM_Click(object sender, EventArgs e)
        {
            var products = SelectItemsManager.SelectProductByCheck(true);
            ExportManager.ExportPriceForShtrikhM(products);
        }
        protected void MiViewPriceList_Click(object sender, EventArgs e)
        {
            var products = new ProductsManager().GetProducts();
            new UIListView(products.Select(s => new { Կոդ = s.Code, Անվանում = s.Description, Չմ = s.Mu, Մեծածախ = s.DealerPrice, Մանրածախ = s.Price })
                , "Գնացուցակ").Show();
        }
        #endregion

        protected void MiPrintBarcode_Click(object sender, EventArgs e)
        {
            var product = SelectItemsManager.SelectProduct().FirstOrDefault();
            if (product == null) return;
            var ctrl = new UctrlBarcodeWithText(new BarcodeViewModel(product.Code, product.Barcode, product.Description, product.Price, null));
            PrintManager.PrintPreview(ctrl, "Print Barcode", true);
        }

        private void MiCreateAccountantDocument_Click(object sender, RoutedEventArgs e)
        {
            var path = new OpenFileDialog();
            path.ShowDialog();
            ExcelImportManager.ImportInvoice(path.FileName);
            new TaxServiceInvoices.Managers.InvoiceManager().ConvertExcelToAccountingDocument(path.FileName);
            //ExcelExportManager.ExportProducts(path.FileName);
        }
        #endregion

        #endregion
    }






}
