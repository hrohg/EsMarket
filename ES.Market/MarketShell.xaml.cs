using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.DataAccess.Models;
using ES.Market.ViewModels;
using ES.Market.Views;
using ES.MsOffice.ExcelManager;
using ES.Shop.Controls;
using ES.Shop.Edit;
using UserControls.PriceTicketControl;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl.ViewModels;
using UserControls.ViewModels;
using UserControls.ViewModels.StockTakeings;
using UserControls.Views;
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
        #region Private properties
        

        private List<MembersRoles> _userRoles;
        #endregion

        #region Private methods
        private void SetPermitions()
        {
            var stocks = ApplicationManager.Instance.CashProvider.GetStocks;
            
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Admin") != null)
            {
                MiAdmin.Visibility = Visibility.Visible;
                MiAccounting.Visibility = MiAdministratorSettings.Visibility = MiAction.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Director") != null)
            {
                MiViewStockTaking.IsEnabled =
                    MiViewLastStockTaking.IsEnabled = true;
                MiControlPanel.Visibility =
                MiAccounting.Visibility =
                MiAction.Visibility =
                MiAdministratorSettings.Visibility =
                 Visibility.Visible;
            }

            if (_userRoles.FirstOrDefault(s => s.RoleName == "Manager") != null)
            {
                MiAccounting.Visibility = Visibility.Visible;
                MiViewStockTaking.IsEnabled =
                   MiViewLastStockTaking.IsEnabled =
                   MiManageServices.IsEnabled = true;
                MiControlPanel.Visibility = MiReport.Visibility =
                MiAdvancedSettings.Visibility = MiAdvancedSettings.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "SaleManager") != null)
            {
                MiViewStockTaking.IsEnabled =
                 MiViewLastStockTaking.IsEnabled =true;
                MiControlPanel.Visibility = MiReport.Visibility =

                MiProductOrderByProviders.Visibility = MiProductOrderByBrands.Visibility = Visibility.Visible;
            }
            if (_userRoles.FirstOrDefault(s => s.RoleName == "Storekeeper") != null)
            {
                MiViewStockTaking.IsEnabled =
                    MiViewLastStockTaking.IsEnabled =true;
            }
            if (_userRoles.FirstOrDefault(s => s.Id == (int)EsSettingsManager.MemberRoles.Seller || s.Id == (int)EsSettingsManager.MemberRoles.SeniorSeller) != null)
            {
                MiViewLastStockTaking.IsEnabled = true;
            }
        }
        private void ConfigureComponent()
        {
            SetPermitions();
            //if (_userRoles.FirstOrDefault(s => s.RoleName == "Seller") != null)
            //{ LoadInvoiceTab(new InvoiceModel(_user, _member, (long)InvoiceType.SaleInvoice)); }
        }

        private void LoadPackingListTab(long invoiceTypeId, bool? isAccepted = null)
        {
            var invoice = SelectItemsManager.SelectInvoice(invoiceTypeId, ApplicationManager.Instance.GetEsMember.Id, isAccepted, false).FirstOrDefault();
            if (invoice == null)
            {
                MessageBox.Show("Ապրանքագիր չի հայտնաբերվել։");
                return;
            }
            var nextTab = TabShop.Items.Add(new TabItem
            {
                Header = "Ապրանքների ցուցակ" + (" - " + invoice.InvoiceNumber),
                Content = new PackingListUctrl(invoice, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember),
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
            _userRoles = ApplicationManager.Instance.UserRoles;
            if (ApplicationManager.Instance.GetEsMember == null || ApplicationManager.GetEsUser == null)
            {
                MessageBox.Show("Ոչ արտոնագրված գործողություն։ Ծրագիրը կփակվի։ Խնդրում ենք արտոնագրել ծրագրային ապահովումը։",
                 "Արտոնագրային սխալ", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                return;
            }

            if (_userRoles == null || !_userRoles.Any())
            {
                MessageBox.Show("Ծրագիրը կփակվի, քանի որ դուք ոչ մի համակարգում ընդգրկված չեք։ Ընդգրկվելու համար խնդրում ենք դիմել տնօրինություն։ ");
                Close();
                return;
            }

            if (ApplicationManager.Instance.GetEsMember == null || ApplicationManager.GetEsUser == null)
            {
                this.Close();
                return;
            }
            UserControlSession.Inst.BEManager = DataManager.GetInstance();

            InitializeComponent();
            ConfigureComponent();
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
            //EslUpdateManager.UpdateDataFromServer(_member.Id);
        }
       
        #endregion
        
        #region Product order
        protected void MiProductOrderByMinQuantity_Click(object sender, EventArgs e)
        {
            var products = ProductsManager.GetProductOrderByProduct(ApplicationManager.Instance.GetEsMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var ui =
                new UIListView(
                    products.Select(
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
            var products = ProductsManager.GetProductOrderByProduct(ApplicationManager.Instance.GetEsMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var partners = PartnersManager.GetPartner(ApplicationManager.Instance.GetEsMember.Id);
            var ui =
                new UIListView(
                    products.Select(
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
            var brands = SelectItemsManager.SelectBrands(ProductsManager.GetBrands(ApplicationManager.Instance.GetEsMember.Id), true);
            var products = ProductsManager.GetProductOrderByBrands(brands, ApplicationManager.Instance.GetEsMember.Id);
            if (products == null || products.Count == 0)
            {
                MessageBox.Show("Նշված տվյալներով պատվեր չի հայտնաբերվել");
                return;
            }
            var ui =
                new UIListView(
                    products.Select(
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
        private void MiDebitViewDetile_Click(object sender, EventArgs e)
        {
            var partners = PartnersManager.GetPartners(ApplicationManager.Instance.GetEsMember.Id);
            string content, title;
            title = "Դեբիտորական պարտքի դիտում";
            if (partners == null)
            {
                content = "Թերի տվյալներ։\nԽնդրում ենք փորձել մի փոքր ուշ։";
                MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var view = new UIListView(partners.Where(s => s.Debit != null && s.Debit != 0)
                        .Select(s => new { Գործընկեր = s.Description, Դեբիտորական_պարտք = s.Debit }));
            view.Show();
        }

        private void MiRepaymentOfReceivable_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.AccountingReceivable;
            var cashDesk = SelectItemsManager.SelectDefaultCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել դրամարկղ").FirstOrDefault();
            if (cashDesk == null)
            {
                MessageBox.Show("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            //var partners =  PartnersManager.GetPartners(_member.Id);
            var partner = SelectPartner();
            if (partner == null)
            {
                MessageBox.Show("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords =
                new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                    "Դեբիտորական պարտքի մարում \n Գործընկեր։ " + partner.FullName + "\n" +
                    "Դեբտորական պարտք։ " + (partner.Debit != null ? partner.Debit.ToString() : "0") + "\n" +
                    "Կերդիտորական պարտք։ " + (partner.Credit != null ? partner.Credit.ToString() : "0")));
            ctrlAccountingRecords.ShowDialog();
            var repaymentAccountingRecord = ctrlAccountingRecords.AccountingRecord;
            repaymentAccountingRecord.DebitGuidId = cashDesk.Id;
            repaymentAccountingRecord.CreditGuidId = partner.Id;
            if (!ctrlAccountingRecords.Result || repaymentAccountingRecord == null ||
                repaymentAccountingRecord.Amount == 0) return;
            var depositAccountingRecords = new AccountingRecordsModel(date: repaymentAccountingRecord.RegisterDate,
                memberId: repaymentAccountingRecord.MemberId, registerId: repaymentAccountingRecord.RegisterId)
            {
                Amount = 0,
                Description = repaymentAccountingRecord.Description,
                Debit = (long)AccountingRecordsManager.AccountingPlan.CashDesk,
                Credit = (long)AccountingRecordsManager.AccountingPlan.ReceivedInAdvance,
                DebitGuidId = repaymentAccountingRecord.DebitGuidId,
                CreditGuidId = repaymentAccountingRecord.CreditGuidId,
            };

            if (partner.Debit != null && repaymentAccountingRecord.Amount > partner.Debit)
            {
                if (MessageBox.Show(
                    "Վճարվել է " + (repaymentAccountingRecord.Amount - partner.Debit) + " դրամ ավել։ \n" +
                    "Ավելացնել " + (repaymentAccountingRecord.Amount - partner.Debit) + " դրամ որպես կանխավճար։",
                    "Գերավճար", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                depositAccountingRecords.Amount = repaymentAccountingRecord.Amount - (decimal)partner.Debit;
            }
            repaymentAccountingRecord.Amount -= depositAccountingRecords.Amount;
            if (AccountingRecordsManager.SetPartnerPayment(depositeAccountRecords: depositAccountingRecords,
                repaymentAccountingRecords: repaymentAccountingRecord, memberid: ApplicationManager.Instance.GetEsMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //todo
        private PartnerModel SelectPartner(PartnerType partnerTypeId = 0)
        {
            var partners = partnerTypeId != 0 ? PartnersManager.GetPartner(ApplicationManager.Instance.GetEsMember.Id, partnerTypeId) : PartnersManager.GetPartners(ApplicationManager.Instance.GetEsMember.Id);
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
        protected void MiViewAccountingPlan_Click(object sender, EventArgs e)
        {
            var ctrl = new SelectDateIntermediate();
            ctrl.ShowDialog();
            if (ctrl.DialogResult != true) { return; }
            var accountingRecords = AccountingRecordsManager.GetAccountingRecords(beginDate: ctrl.StartDate ?? DateTime.Now, endDate: ctrl.EndDate ?? DateTime.Now);
            var uiReport = new UIListView(accountingRecords.Select(s =>
                new
                {
                    Ամսաթիվ = s.RegisterDate,
                    Դեբետ = AccountingRecordsManager.GetAccountingRecordsDescription(s.Debit),
                    Դեբետ_Անվանում = AccountingRecordsManager.GetAccountingRecordsDescription(s.Debit, s.DebitGuidId, s.DebitLongId),
                    Կրեդիտ = AccountingRecordsManager.GetAccountingRecordsDescription(s.Credit),
                    Կրեդիտ_Անվանում = AccountingRecordsManager.GetAccountingRecordsDescription(s.Credit, s.CreditGuidId, s.CreditLongId),
                    Գումար = s.Amount,
                    Նշումներ = s.Description
                }));
            uiReport.Show();
        }
        //251
        protected void MiTransferIntoCashDesk_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            var fromCashDesk = SelectItemsManager.SelectCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել ելքագրվող դրամարկղը").FirstOrDefault();
            var toCashDesk = SelectItemsManager.SelectCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել մուտքագրվող դրամարկղը").FirstOrDefault();
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
            if (AccountingRecordsManager.SetCashTransfer(accountingRecords, ApplicationManager.Instance.GetEsMember.Id))
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
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.EquityBase;
            var fromUser = SelectItemsManager.SelectUser(ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել ներդրող").FirstOrDefault();
            var toCashDesk = SelectItemsManager.SelectCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել մուտքագրվող դրամարկղը").FirstOrDefault();
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
            if (AccountingRecordsManager.SetEquityBase(accountingRecords, ApplicationManager.Instance.GetEsMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //521
        protected void MiRepaymentOfDebts_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.PurchasePayables;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            var partner = SelectItemsManager.SelectPartners(false, "Ընտրել գործընկեր դրամարկղը").FirstOrDefault();
            var fromCashDesk = SelectItemsManager.SelectCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել ելքագրվող դրամարկղը").FirstOrDefault();
            if (partner == null || fromCashDesk == null)
            {
                MessageBox.Show("Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = fromCashDesk.Id;
            accountingRecords.DebitGuidId = partner.Id;
            var description = "Գործընկեր։ " + partner.FullName + "։ Կրեդիտորական պարտք։" + partner.Credit + " դր․" + "\n" +
                "Ելքագրվող դրամարկդ։ " + fromCashDesk.Name + " առկա է։ " + fromCashDesk.Total + " դր․";
            var ui = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords, description));
            ui.ShowDialog();

            var accountingRecord = ui.AccountingRecord;
            if (!ui.Result || accountingRecord == null ||
                accountingRecords.Amount == 0) return;
            if (accountingRecords.Amount > fromCashDesk.Total)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան առկա է։", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (accountingRecords.Amount > partner.Credit)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։ Վճարվել է ավելի շատ քան կրեդիտորական պարտքն է։", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (AccountingRecordsManager.SetRepaymentOfDebts(accountingRecords, ApplicationManager.Instance.GetEsMember.Id))
            {
                MessageBox.Show("Վճարումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Վճարումն ընդհատվել է։ Խնդրում ենք փորձել ևս մեկ անգամ։");
            }
        }
        //523
        protected void MiReceivedInAdvance_Click(object sender, EventArgs e)
        {
            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.ReceivedInAdvance;
            var cashDesk = SelectItemsManager.SelectDefaultCashDesks(null, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել դրամարկղ").FirstOrDefault();
            if (cashDesk == null)
            {
                MessageBox.Show("Դրամարկղ հայտնաբերված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.DebitGuidId = cashDesk.Id;
            //var partners = PartnersManager.GetPartners(_member.Id);
            var partner = SelectPartner();
            if (partner == null)
            {
                MessageBox.Show("Գործընկեր ընտրված չէ։", "Թերի տվյալներ");
                return;
            }
            accountingRecords.CreditGuidId = partner.Id;
            var ctrlAccountingRecords = new CtrlAccountingRecords(new AccountingRecordsViewModel(accountingRecords,
                "Կանխավճար \n" +
                "Գործընկեր։ " + partner.FullName + "\n" +
                 "Դեբտորական պարտք։ " + (partner.Debit != null ? partner.Debit.ToString() : "0") + "\n" +
                    "Կերդիտորական պարտք։ " + (partner.Credit != null ? partner.Credit.ToString() : "0")));
            ctrlAccountingRecords.ShowDialog();
            var receivedInAdvance = ctrlAccountingRecords.AccountingRecord;
            receivedInAdvance.CreditGuidId = partner.Id;
            receivedInAdvance.DebitGuidId = cashDesk.Id;
            if (!ctrlAccountingRecords.Result || receivedInAdvance == null || receivedInAdvance.Amount == 0) return;
            AccountingRecordsManager.SetPartnerPayment(depositeAccountRecords: receivedInAdvance,
                repaymentAccountingRecords: null, memberid: ApplicationManager.Instance.GetEsMember.Id);
        }

        //712
        protected void MiCostOfSales_Click(object sender, EventArgs e)
        {
            new CtrlSingleAccountingRecords((int)AccountingRecordsManager.AccountingPlan.CostOfSales, (int)AccountingRecordsManager.AccountingPlan.CashDesk).Show();
        }

        protected void MiCostOfSales_Salary_Click(object sender, EventArgs e)
        {
            var costOfSales = SelectItemsManager.SelectSubAccountingPlan(
                SubAccountingPlanManager.GetSubAccountingPlanModels((long)AccountingRecordsManager.AccountingPlan.Debit_For_Salary, ApplicationManager.Instance.GetEsMember.Id, true), false, "Ընտրել");
            var debitForSalary = SelectItemsManager.SelectSubAccountingPlan(
                SubAccountingPlanManager.GetSubAccountingPlanModels((long)AccountingRecordsManager.AccountingPlan.Debit_For_Salary, ApplicationManager.Instance.GetEsMember.Id, true), false, "Ընտրել աշխատակից");
            var cashDesk = SelectItemsManager.SelectCashDesks(true, ApplicationManager.Instance.GetEsMember.Id, false, "Ընտրել դրամարկղ");

            if (costOfSales.First() == null || debitForSalary.First() == null || cashDesk.First() == null)
            {
                return;
            }

            var accountingRecords1 = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords1.Debit = (long)AccountingRecordsManager.AccountingPlan.CostOfSales;
            accountingRecords1.DebitGuidId = costOfSales.First().Id;
            accountingRecords1.Credit = (long)AccountingRecordsManager.AccountingPlan.Debit_For_Salary;
            accountingRecords1.CreditGuidId = debitForSalary.First().Id;

            var accountingRecords2 = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords2.Debit = (long)AccountingRecordsManager.AccountingPlan.Debit_For_Salary;
            accountingRecords2.DebitGuidId = debitForSalary.First().Id;
            accountingRecords2.Credit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
            accountingRecords2.CreditGuidId = cashDesk.First().Id;

            var accountingRecords = new AccountingRecordsModel(DateTime.Now, ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            accountingRecords.Debit = (long)AccountingRecordsManager.AccountingPlan.CostOfSales;
            accountingRecords.DebitGuidId = debitForSalary.First().Id;
            accountingRecords.Credit = (long)AccountingRecordsManager.AccountingPlan.CashDesk;
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

        /// <summary>
        /// View detiles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MiSingleAccountingRecords_Click(object sender, EventArgs e)
        {
            new CtrlSingleAccountingRecords((int)AccountingRecordsManager.AccountingPlan.CostOfSales, (int)AccountingRecordsManager.AccountingPlan.CashDesk).Show();
        }
        protected void MiMultipleAccountingRecords_Click(object sender, EventArgs e)
        {
            new CtrlMultipleAccountingRecords().Show();
        }
        private void MiDebitByPartners_Click(object sender, EventArgs e)
        {
            var debits = PartnersManager.GetPartners(ApplicationManager.Instance.GetEsMember.Id);
            var ui = new UIListView(debits.Where(s => s.Debit > 0 || s.Credit > 0).Select(s => new { Գործընկեր = s.FullName, Կանխավճար = s.Credit, Պարտք = s.Debit }).ToList());
            ui.Show();
        }
        protected void MiDebitDetile_Click(object sender, EventArgs e)
        {
            var partner = SelectPartner();
            if (partner == null) return;
            var accountReceivable = PartnersManager.GetAccountsReceivable(partner.Id);
            var ui = new UIListView(accountReceivable.Select(s => new
            {
                Ամսաթիվ = s.Date.ToString("dd.MM.yyyy"),
                Պարտք = s.Amount,
                Վճարված = s.PaidAmount,
                Վերջնաժամկետ = s.ExpairyDate != null ? ((DateTime)s.ExpairyDate).ToString("dd.MM.yyyy") : string.Empty
            }).ToList(), partner.FullName);
            ui.Show();
        }
        private void MiAccountingPlan_Click(object sender, EventArgs e)
        {
            var ctrlAccountingPlan = new CtrlAccountingRecords(ApplicationManager.Instance.GetEsMember.Id, ApplicationManager.GetEsUser.UserId);
            ctrlAccountingPlan.Show();
        }
        protected void MiAccountingRepaymentDetile_Click(object sender, EventArgs e)
        {
            var partner = SelectPartner();
            if (partner == null) return;
            var repayment = AccountingRecordsManager.GetAccountingRecords((long)AccountingRecordsManager.AccountingPlan.CashDesk, (long)AccountingRecordsManager.AccountingPlan.AccountingReceivable);
            var ui = new UIListView(repayment.Where(s => s.CreditGuidId == partner.Id).Select(s => new { Ամսաթիվ = s.RegisterDate, Վճարված = s.Amount, Նշումներ = s.Description }).ToList(), partner.FullName);
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
                Content = new UctrlEditServices(ApplicationManager.Instance.GetEsMember.Id),
                AllowDrop = true

            });
            TabShop.SelectedIndex = nextTab;
        }
        #endregion

        #region Control Panel
        #region Releate
        private void MiViewStockTaking_OnClick(object sender, EventArgs e)
        {
            var dateIntermediate = SelectManager.GetDateIntermediate();
            if (dateIntermediate == null)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var startDate = (DateTime)dateIntermediate.Item1;
            var endDate = (DateTime)dateIntermediate.Item2;
            var stockTakes = StockTakeManager.GetStockTakeByCreateDate(startDate, endDate, ApplicationManager.Instance.GetEsMember.Id);
            if (stockTakes == null || stockTakes.Count == 0)
            {
                MessageBox.Show("Տվյալ ժամանակահատվածում հաշվառում չի իրականացվել։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectItemId = SelectManager.GetSelectedItem(stockTakes.Select(s =>
                            new ItemsToSelect
                            {
                                DisplayName = s.StockTakeName + " " + s.CreateDate,
                                SelectedValue = s.Id
                            }).ToList(), false).Select(s => (Guid)s.SelectedValue).FirstOrDefault();
            var stockTake = stockTakes.FirstOrDefault(s => s.Id == selectItemId);
            if (stockTake == null)
            {
                MessageBox.Show("Գործողությունն ընդհատված է։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //var nextTab = TabShop.Items.Add(new TabItem
            //{
            //    Header = "Գույքագրում",
            //    Content = new StockTakingUctrl(new StockTakeViewModel(stockTake, ApplicationManager.Instance.GetEsMember.Id)),
            //    AllowDrop = true
            //});
            //TabShop.SelectedIndex = nextTab;
        }
        private void MiViewLastStockTaking_OnClick(object sender, EventArgs e)
        {
            var stockTaking = StockTakeManager.GetLastStockTake(ApplicationManager.Instance.GetEsMember.Id);
            if (stockTaking == null)
            {
                MessageBox.Show("Տվյալ ժամանակահատվածում հաշվառում չի իրականացվել։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //var nextTab = TabShop.Items.Add(new TabItem
            //{
            //    Header = "Գույքագրում",
            //    Content = new StockTakingUctrl(new StockTakeViewModel(stockTaking, ApplicationManager.Instance.GetEsMember.Id)),
            //    AllowDrop = true
            //});
            //TabShop.SelectedIndex = nextTab;
        }
        #endregion

        protected void MiBackupData_Click(object sender, EventArgs e)
        {
            new DatabaseManagement.MainWindow().ShowDialog();
        }
        protected void MiManageBrands_Click(object sender, EventArgs e)
        {
            ApplicationManager.MessageManager.OnNewMessage(EditManager.UpdateBrandsManager(ApplicationManager.Instance.GetEsMember.Id)
                ? new MessageModel(DateTime.Now, "Խմբագրումն իրականացել է հաջողությամբ։", MessageModel.MessageTypeEnum.Success)
                : new MessageModel(DateTime.Now, "Խմբագրումը ձախողվել է։", MessageModel.MessageTypeEnum.Warning));
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
            var products = SelectItemsManager.SelectProductByCheck(ApplicationManager.Instance.GetEsMember.Id, true);
            ExportManager.ExportPriceForShtrikhM(products);
        }
        protected void MiViewPriceList_Click(object sender, EventArgs e)
        {
            var products = new ProductsManager().GetProducts(ApplicationManager.Instance.GetEsMember.Id);
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
