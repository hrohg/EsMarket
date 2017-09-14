using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using CashReg;
using CashReg.Helper;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.DataAccess.Models;
using ES.Market.Commands;
using ES.Market.Config;
using ES.Market.Views.Reports.View;
using ES.Market.Views.Reports.ViewModels;
using ES.Shop.Controls;
using ES.Shop.Users;
using Shared.Helpers;
using UserControls.ControlPanel.Controls;
using UserControls.Enumerations;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl.Helper;
using UserControls.ViewModels;
using UserControls.ViewModels.Documents;
using UserControls.ViewModels.Invoices;
using UserControls.ViewModels.Logs;
using UserControls.ViewModels.Managers;
using UserControls.ViewModels.Partners;
using UserControls.ViewModels.Products;
using UserControls.ViewModels.Reports;
using UserControls.ViewModels.Settings;
using UserControls.ViewModels.StockTakeings;
using UserControls.ViewModels.Tools;
using UserControls.Views;
using UserControls.Views.Accountant;
using UserControls.Views.CustomControls;
using UserControls.Views.View;
using ExportManager = ES.Business.Managers.ExportManager;
using ItemsToSelect = UserControls.ControlPanel.Controls.ItemsToSelect;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;
using TabControl = System.Windows.Controls.TabControl;

namespace ES.Market.ViewModels
{
    public class ShellViewModel : IShellViewModel, IDisposable
    {
        #region Events
        public delegate void LogOutDelegate();
        public event LogOutDelegate OnLogOut;
        #endregion Events

        #region Internal fields
        private const string UserProperty = "User";
        private const string MemberProperty = "Member";
        private const string UserRolesProperty = "UserRoles";
        private const string IsLocalModeProperty = "IsLocalMode";
        private const string MessagesProperty = "Messages";
        private const string MessageProperty = "Message";
        #endregion

        #region Internal properties
        private MarketShell _parentTabControl;
        private EsUserModel _user;
        private EsMemberModel _member;
        private List<MembersRoles> _userRoles;
        private bool _isLocalMode;

        private ObservableCollection<MessageModel> _messages = new ObservableCollection<MessageModel>();
        private UctrlLibraryBrowser _libraryBrowser;
        private bool _isLoading;
        private bool _isCashUpdateing;
        private DocumentViewModel _activeTab;

        public DocumentViewModel ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                RaisePropertyChanged("AddSingleVisibility");
            }
        }
        #region Log
        private LogViewModel _logViewModel;

        public LogViewModel LogViewModel
        {
            get { return _logViewModel ?? (_logViewModel = new LogViewModel()); }
        }
        #endregion

        #region ProductitemsToolViewModel
        private ProductItemsToolsViewModel _productItemsToolsViewModel;
        private ProductItemsToolsViewModel ProductItemsToolsViewModel
        {
            get
            {
                if (_productItemsToolsViewModel == null)
                {
                    _productItemsToolsViewModel = new ProductItemsToolsViewModel();
                    _productItemsToolsViewModel.OnManageProduct += OnManageProduct;
                    _productItemsToolsViewModel.OnProductItemSelected += OnSetProduct;
                    AddTools<ProductItemsToolsViewModel>(_productItemsToolsViewModel, false);
                }
                return _productItemsToolsViewModel;
            }
        }

        private void OnSetProduct(ProductItemModel productitem)
        {
            var vm = Documents.FirstOrDefault(s => s.IsSelected) as InvoiceViewModel; //Documents.OfType<InvoiceViewModel>().FirstOrDefault();
            if (vm == null)
            {
                return;
            }
            vm.OnSetProductItem(productitem);
        }

        #endregion ProductitemsToolViewModel

        #endregion

        #region External properties

        #region Avalon dock
        public ObservableCollection<DocumentViewModel> Documents { get; set; }
        public ObservableCollection<ToolsViewModel> Tools { get; set; }
        #endregion Avalon dock

        public EsUserModel User { get { return _user; } set { _user = value; RaisePropertyChanged(UserProperty); } }
        public EsMemberModel Member { get { return _member; } set { _member = value; RaisePropertyChanged(MemberProperty); } }
        public List<MembersRoles> UserRoles { get { return _userRoles; } set { _userRoles = value; RaisePropertyChanged(UserRolesProperty); } }
        public string ProcessDescription { get; set; }
        public bool IsCashUpdateing
        {
            get { return _isCashUpdateing; }
            set
            {
                if (_isCashUpdateing == value) _isCashUpdateing = value;
                RaisePropertyChanged("IsCashUpdateing");
            }
        }
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (_isLoading == value)
                {
                    return;
                }
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }
        public bool IsLocalMode { get { return _isLocalMode; } set { _isLocalMode = value; RaisePropertyChanged(IsLocalModeProperty); } }
        public ObservableCollection<InvoiceViewModel> Invoices = new ObservableCollection<InvoiceViewModel>();
        public ObservableCollection<MessageModel> Messages { get { return new ObservableCollection<MessageModel>(_messages.OrderByDescending(s => s.Date)); } set { _messages = value; RaisePropertyChanged(MessagesProperty); RaisePropertyChanged(MessageProperty); } }
        public MessageModel Message { get { return Messages.LastOrDefault(); } }
        public string ServerDescription { get { return ApplicationManager.IsEsServer ? string.Format("{0}: {1}", "Cloud", Member.FullName) : string.Format("{0}: {1}", "Local server", Member.FullName); } }
        public string ServerPath
        {
            get
            {
                return ApplicationManager.GetServerPath().OriginalString;
            }
        }

        #region Toolbar

        public bool IsEcrActivated
        {
            get { return ApplicationManager.Settings.MemberSettings.IsEcrActivated; }
            set
            {
                ApplicationManager.Settings.MemberSettings.IsEcrActivated = value;
                RaisePropertyChanged("IsEcrActivated");
                RaisePropertyChanged("EcrButtonTooltip");
            }
        }
        public string EcrButtonTooltip { get { return IsEcrActivated ? "ՀԴՄ ակտիվ է" : "ՀԴՄ պասիվ է"; } }
        public Visibility AddSingleVisibility
        {
            get
            {
                return ActiveTab is InvoiceViewModel ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsSingle { get; set; }

        private bool _isSalePrinterActive;
        public bool IsSalePrinterActive
        {
            get
            {
                return !string.IsNullOrEmpty(ApplicationManager.Settings.MemberSettings.ActiveSalePrinter) && _isSalePrinterActive;
            }
            set { _isSalePrinterActive = value; RaisePropertyChanged("IsSalePrinterActive"); }
        }

        #endregion Toolbar

        #endregion

        #region Constructors
        public ShellViewModel()
        {
            Initialize();
        }
        public void Dispose()
        {

        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            User = ApplicationManager.GetEsUser;
            Member = ApplicationManager.Instance.GetMember;
            UserRoles = ApplicationManager.Instance.UserRoles;
            IsLocalMode = ApplicationManager.Settings.MemberSettings.IsOfflineMode;

            Documents = new ObservableCollection<DocumentViewModel>();
            Tools = new ObservableCollection<ToolsViewModel>();
            ApplicationManager.MessageManager.MessageReceived += OnNewMessage;
            Tools.Add(LogViewModel);
            AddDocument(new StartPageViewModel(this));
            LoadTempInvoices();
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            RefreshCashCommand = new RefreshCashCommand(this);
            //Base
            KeyPressedCommand = new RelayCommand<KeyEventArgs>(OnKeyPressed);

            //Admin
            ImportProductsCommand = new RelayCommand(OnImportProducts);
            ViewInternalWayBillCommands = new RelayCommand<ViewInvoicesEnum>(OnViewViewInternalWayBillCommands, CanViewViewInternalWayBillCommands);

            LogOutCommand = new RelayCommand(OnLogoff);
            ChangePasswordCommand = new RelayCommand(OnChangePassword);
            //Data
            WriteOffProductsCommand = new RelayCommand(OnWriteOffProducts);
            WriteOffStockTakingCommand = new RelayCommand(OnWriteOffStockTaking);
            WriteInStockTakingCommand = new RelayCommand(OnWriteInStockTaking);
            GetReportCommand = new RelayCommand<ReportTypes>(OnGetReport);

            #region Help
            PrintPriceTicketCommand = new RelayCommand<PrintPriceTicketEnum?>(OnPrintPriceTicket, CanPrintPriceTicket);
            #endregion Help

            #region Settings

            CheckConnectionCommand = new RelayCommand(OnCheckConnection);
            #endregion Settings

        }

        private void LoadTempInvoices()
        {
            var invoices = InvoicesManager.LoadAutosavedinvoices();
            InvoiceViewModel invoice = null;
            foreach (var invoiceItems in invoices)
            {
                if (invoiceItems.Item1 == null || invoiceItems.Item2 == null) continue;
                switch ((InvoiceType)invoiceItems.Item1.InvoiceTypeId)
                {
                    case InvoiceType.PurchaseInvoice:
                        invoice = new PurchaseInvoiceViewModel();
                        break;
                    case InvoiceType.SaleInvoice:
                        invoice = new SaleInvoiceViewModel();
                        break;
                    case InvoiceType.ProductOrder:
                        invoice = new PurchaseInvoiceViewModel();
                        break;
                    case InvoiceType.MoveInvoice:
                        invoice = new InternalWaybillViewModel();
                        break;
                    case InvoiceType.InventoryWriteOff:
                        invoice = new InventoryWriteOffViewModel();
                        break;
                    case InvoiceType.Statement:
                        //invoice = new PurchaseInvoiceViewModel();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (invoice != null)
                {
                    invoice.Invoice = invoiceItems.Item1;
                    invoice.InvoiceItems = new ObservableCollection<InvoiceItemsModel>(invoiceItems.Item2);
                }
                Documents.Add(new InvoiceViewModel());
            }
        }

        private bool CanViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            switch (o)
            {
                case ViewInvoicesEnum.None:
                case ViewInvoicesEnum.ByDetiles:
                    return true;
                    break;
                case ViewInvoicesEnum.ByStock:
                    return false;
                    break;

                default:
                    return false;
            }
        }

        private void OnViewViewInternalWayBillCommands(ViewInvoicesEnum o)
        {
            DocumentViewModel vm = null;
            switch (o)
            {
                case ViewInvoicesEnum.None:
                    vm = new ViewInternalWayBillViewModel(o);
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    vm = new InternalWayBillDetileViewModel(o);
                    break;
                default:
                    break;
            }
            AddDocument(vm);
        }

        #region Add remove documents and tools

        private void AddDocument<T>(DocumentViewModel vm, bool allowDoublicate = true)
        {
            var exDocument = Documents.FirstOrDefault(s => s is T);
            if (!allowDoublicate && exDocument != null)
            {
                //todo: Activate document
                exDocument.IsActive = true;
                exDocument.IsSelected = true;
                return;
            }
            AddDocument(vm);
        }

        private void OnActiveTabChanged(DocumentViewModel document, ActivityChangedEventArgs e)
        {
            Documents.Select(s => s.IsActive = (s == document && e.Value));
            ActiveTab = document;
            if (ActiveTab is InvoiceViewModel || ActiveTab is ProductManagerViewModel)
            {
                ProductItemsToolsViewModel.IsActive = true;
            }
        }

        private void AddInvoiceDocument(InvoiceViewModel vm)
        {
            var exInvoice = Documents.SingleOrDefault(s => s is InvoiceViewModel && ((InvoiceViewModel)s).Invoice.Id == vm.Invoice.Id);
            if (exInvoice != null)
            {
                exInvoice.IsSelected = true;
            }
            else
            {
                AddDocument(vm);
            }
        }

        private void AddDocument(DocumentViewModel vm)
        {
            if (vm.IsClosable)
            {
                vm.OnClosed += OnRemoveDocument;
            }
            vm.ActiveTabChangedEvent += OnActiveTabChanged;
            vm.IsActive = true;
            vm.IsSelected = true;
            Documents.Add(vm);
        }

        private void OnRemoveDocument(PaneViewModel vm)
        {
            if (vm == null) return;
            vm.OnClosed -= OnRemoveDocument;
            if (vm is DocumentViewModel)
            {
                ((DocumentViewModel)vm).ActiveTabChangedEvent -= OnActiveTabChanged;
            }

            Documents.Remove((DocumentViewModel)vm);
            if (vm is ProductManagerViewModel)
            {
                ((ProductManagerViewModel)vm).OnProductEdited -= ProductItemsToolsViewModel.UpdateProducts;
            }
            if (vm is InvoiceViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((InvoiceViewModel)vm).OnSetProductItem;
            }
            if (vm is StockTakeManagerViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((StockTakeManagerViewModel)vm).OnSetProductItem;
            }
        }

        private void AddTools(ToolsViewModel vm)
        {
            Tools.Add(vm);
            vm.OnClosed += OnRemoveTools;
            vm.IsActive = true;
            vm.IsSelected = true;
        }

        private void AddTools<T>(ToolsViewModel vm, bool allowDoublicate = true)
        {
            var exTools = Tools.FirstOrDefault(s => s is T);
            if (!allowDoublicate && exTools != null)
            {
                //todo: Activate tab
                exTools.IsActive = true;
                exTools.IsSelected = true;

                exTools.IsVisible = true;
                Tools.Remove(exTools);
                Tools.Add(exTools);
                return;
            }
            AddTools(vm);
        }

        private void OnRemoveTools(PaneViewModel vm)
        {
            if (vm == null) return;
            vm.OnClosed -= OnRemoveDocument;
            Tools.Remove((ToolsViewModel)vm);
        }

        #endregion Add remove documents and tools

        #region Base

        private void OnKeyPressed(KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.L:
                        OnLogoff(null);
                        break;
                }
            }
            switch (e.Key)
            {
                case Key.F1:
                    //handle X key reate new sale invoice
                    //
                    break;
                case Key.F2:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Seller)) return;
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.Seller) || ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller))
                    {
                        OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.SaleInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    break;
                case Key.F3:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.SaleManager))
                    {
                        OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.PurchaseInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    break;
                case Key.F4:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.StockKeeper)) return;
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper))
                    {
                        OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.MoveInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    break;
                case Key.F5:
                //Used
                case Key.F6:
                    //Used
                    break;
                case Key.F7:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (_userRoles.FirstOrDefault(s => s.RoleName != "Manager" || s.RoleName == "Storekeeper" || s.RoleName != "Seller") == null) break;
                    OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.ProductOrder, InvoiceState.New, MaxInvocieCount.All));
                    break;
                case Key.F8:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key logoff
                    if (_userRoles.FirstOrDefault(s => s.RoleName != "Manager") == null) break;
                    //MiManageProducts_Click(null, null);
                    break;
                case Key.F9:
                    //Used
                    break;
                case Key.F10:
                    //handle X key quite
                    _parentTabControl.Close();
                    break;
            }
        }

        #endregion Base

        private void CreateLibraryBrowser()
        {
            if (_libraryBrowser != null)
            {
                return;
            }
            var result = _parentTabControl.FindVisualChildren<Expander>().ToList();
            var leftPanel = result.FirstOrDefault(s => s.Name == "LeftPanel") as Expander;
            if (leftPanel == null) return;
            _libraryBrowser = new UctrlLibraryBrowser();
            var vm = new LibraryViewModel();
            var xml = new XmlManager();
            var fromStocks = StockManager.GetStocks(xml.GetItemsByControl(XmlTagItems.SaleStocks).Select(s => HgConvert.ToInt64(s.Value)).ToList()).ToList();
            vm.SelectProductItems(fromStocks.Select(s => s.Id).ToList());
            _libraryBrowser.DataContext = vm;
            leftPanel.UpdateLayout();
            var leftPanelBorder = leftPanel.FindVisualChildren<Border>().FirstOrDefault(s => s.Name == "LeftPanel_Border");
            if (leftPanelBorder == null)
            {
                return;
            }
            leftPanelBorder.Child = _libraryBrowser;
            _libraryBrowser.UpdateLayout();
        }

        private void OnNewMessage(MessageModel message)
        {
            LogViewModel.AddLog(message);
            _messages.Add(message);
            RaisePropertyChanged(MessagesProperty);
        }

        private void AddInvoiceDocument(object vm)
        {
            if (vm is DocumentViewModel)
            {
                AddDocument((DocumentViewModel)vm);
                ((PaneViewModel)vm).IsActive = true;
                if (vm is InvoiceViewModel)
                {
                    ProductItemsToolsViewModel.OnProductItemSelected += ((InvoiceViewModel)vm).OnSetProductItem;
                }
                if (vm is StockTakeManagerViewModel)
                {
                    ProductItemsToolsViewModel.OnProductItemSelected += ((StockTakeManagerViewModel)vm).OnSetProductItem;
                }
                OnCreateProductItemsTools(ProductItemsToolsViewModel);
                return;
            }

            //todo: remove code
            int nextTab;
            var tabShop = _parentTabControl.FindChild<TabControl>();
            if (tabShop == null)
            {
                return;
            }
            //CreateLibraryBrowser();


            if (vm is ProductOrderViewModel)
            {
                nextTab = tabShop.Items.Add(new TabItem
                {
                    Content = new ProductOrderUctrl(_user, _member),
                    DataContext = vm,
                    AllowDrop = true
                });
                tabShop.SelectedIndex = nextTab;
                _parentTabControl.UpdateLayout();
                return;
            }
        }

        private void CreateNewControl(List<object> models)
        {
            foreach (var model in models)
            {
                AddInvoiceDocument(model);
            }
        }

        protected void OnCreateProductItemsTools(ProductItemsToolsViewModel vm)
        {
            if (!Tools.Any(t => t is ProductItemsToolsViewModel))
            {
                Tools.Add(vm);
            }
        }

        private StockTakeModel GetOpeningStockTake(bool isClosed = false)
        {
            List<StockTakeModel> stockTakes;
            if (isClosed)
            {
                var dateIntermediate = SelectManager.GetDateIntermediate();
                if (dateIntermediate == null)
                {
                    MessageBox.Show("Գործողությունն ընդհատված է։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
                var startDate = (DateTime)dateIntermediate.Item1;
                var endDate = (DateTime)dateIntermediate.Item2;
                stockTakes = StockTakeManager.GetStockTakeByCreateDate(startDate, endDate);
                if (stockTakes == null || !stockTakes.Any())
                {
                    MessageBox.Show("Տվյալ ժամանակահատվածում հաշվառում չի իրականացվել։", "Թերի տվյալներ", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }
            }
            else
            {
                stockTakes = StockTakeManager.GetOpeningStockTakes();
            }

            if (stockTakes == null || !stockTakes.Any())
            {
                MessageManager.OnMessage(new MessageModel("Բաց գույքագրում առկա չէ։", MessageTypeEnum.Warning));
                return null;
            }
            var selectItemId = SelectManager.GetSelectedItem(stockTakes.OrderByDescending(s => s.CreateDate).Select(s => new ItemsToSelect
            {
                DisplayName = s.StockTakeName + " " + s.CreateDate,
                SelectedValue = s.Id
            }).ToList(), false).Select(s => (Guid)s.SelectedValue).FirstOrDefault();
            return stockTakes.FirstOrDefault(s => s.Id == selectItemId);
        }

        private void OnChangeServerSettings(object o)
        {
            var server = SelectItemsManager.SelectServers(DataServerSettings.GetDataServers());
            new ServerConfig(new ServerViewModel(server != null && server.Any() ? server.First() : new DataServer())).Show();
        }

        private void OnRemoveServerSettings(object o)
        {
            var dataServer = SelectItemsManager.SelectServers(DataServerSettings.GetDataServers());
            if (dataServer == null) return;
            DataServerSettings.RemoveDataServer(dataServer);
        }

        private bool CanSyncronizeServerData(object o)
        {
            var syncronizeMode = o as SyncronizeServersMode? ?? SyncronizeServersMode.None;
            switch (syncronizeMode)
            {
                case SyncronizeServersMode.DownloadMemberData:
                    return ApplicationManager.IsInRole(UserRoleEnum.Admin) && ApplicationManager.IsEsServer;
                case SyncronizeServersMode.DownloadUserData:
                    return (ApplicationManager.IsInRole(UserRoleEnum.Admin) || ApplicationManager.IsInRole(UserRoleEnum.Director)) && ApplicationManager.IsEsServer;
                case SyncronizeServersMode.DownloadBaseData:
                    return (ApplicationManager.IsInRole(UserRoleEnum.Admin) || ApplicationManager.IsInRole(UserRoleEnum.Director)) && !ApplicationManager.IsEsServer;
                case SyncronizeServersMode.SyncronizeBaseData:
                    return (ApplicationManager.IsInRole(UserRoleEnum.Admin) || ApplicationManager.IsInRole(UserRoleEnum.Manager)) && !ApplicationManager.IsEsServer;
                case SyncronizeServersMode.None:
                    return false;
                default:
                    return false;
            }
            return !ApplicationManager.IsEsServer;
        }

        private void OnSyncronizeServerData(object o)
        {
            var syncronizeMode = o as SyncronizeServersMode? ?? SyncronizeServersMode.None;
            if (DatabaseManager.SyncronizeServers(syncronizeMode, ApplicationManager.Instance.GetMember.Id))
            {
                MessageManager.OnMessage(new MessageModel("Տվյալների համաժամանակեցումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success));
            }
            else
            {
                MessageManager.OnMessage(new MessageModel("Տվյալների համաժամանակեցումը ձախողվել է։", MessageTypeEnum.Error));
            }
        }

        private void OnSyncronizeProducts(object o)
        {
            Thread myThread = new Thread(DatabaseManager.SyncronizeProducts);
            myThread.Start();
        }

        #endregion

        #region External methods

        public void ExportProductsForScale(object o)
        {
            if (!(o is ExportForScale)) return;
            switch ((ExportForScale)o)
            {
                case ExportForScale.ShtrixM:
                    ExportManager.ExportPriceForShtrikhM(SelectItemsManager.SelectProductByCheck(true));
                    break;
                case ExportForScale.Custom:
                    ExportManager.ExportPriceForScaleToXml(SelectItemsManager.SelectProductByCheck(true));
                    break;
                case ExportForScale.WaightsOnly:
                    ExportManager.ExportPriceForScaleToXml(new ProductsManager().GetProductsBy(ProductViewType.WeigthsOnly, _member.Id));
                    break;
                case ExportForScale.ShtrixK:
                    ExportManager.ExportPriceForShtrikhK(new ProductsManager().GetProductsBy(ProductViewType.All, _member.Id));
                    break;
                case ExportForScale.All:
                    break;
                default:
                    ExportManager.ExportPriceForScaleToXml(ApplicationManager.Instance.CashProvider.Products);
                    break;
            }
        }

        /// <summary>
        /// Settings
        /// </summary>
        /// <returns></returns>
        public void OnManageSettings()
        {
            var vm = new SettingsViewModel();
            AddDocument<SettingsViewModel>(vm, false);
        }

        /// <summary>
        /// Help
        /// </summary>
        /// <returns></returns>
        /// Barcode
        private bool CanPrintPriceTicket(PrintPriceTicketEnum? printPriceTicketEnum)
        {
            return printPriceTicketEnum != null;
        }

        public void OnPrintPriceTicket(PrintPriceTicketEnum? printPriceTicketEnum)
        {
            PriceTicketManager.PrintPriceTicket(printPriceTicketEnum);
        }

        public bool Close()
        {
            for (int last = Documents.Count - 1; last >= 0; --last)
            {
                Documents[last].IsActive = true;
                Documents[last].IsSelected = true;
                if (!Documents[last].Close()) return false;
            }
            return true;
        }

        #endregion

        #region Command methods

        public bool CanUpdateCash()
        {
            return IsLocalMode && !IsCashUpdateing;
        }

        public void UpdateCash()
        {
            if (!CanUpdateCash())
            {
                return;
            }
            var td = new Thread(() =>
            {
                IsLoading = IsCashUpdateing = true;
                ApplicationManager.Instance.CashProvider.UpdateCash();
                IsLoading = IsCashUpdateing = false;
            });
            td.Start();
        }

        private bool CanEditUserCommand(object o)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Director);
        }

        private void OnEditUsers(object o)
        {
            AddTools<ManageUsersViewModel>(new ManageUsersViewModel(), false);
        }

        #region Admin

        private void OnImportProducts(object o)
        {
            var filePath = new OpenFileDialog();
            filePath.Filter = "Xml file (*.xml) | *.xml";

            if (filePath.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            var td = new Thread(() => { ImportDsProducts(filePath.FileName); });
            td.Start();
        }

        [Serializable, XmlRoot("user")]
        public class row
        {
            public int ID { get; set; }
            public string name { get; set; }
            public string description1 { get; set; }
            public double price_ret { get; set; }
            public double last_price_in { get; set; }
            public string barcode { get; set; }
            public string unit { get; set; }
        }

        private void ImportProducts(string fileName)
        {
            var products = (List<EsProductModel>)XmlManager.Read(fileName, typeof(List<EsProductModel>));

            if (products == null || products.Count == 0)
            {
                return;
            }
            var insertedProuctCount = 0;
            bool insertAll = false;
            bool exitLoop = false;
            var result = MessageBox.Show(string.Format("Իրականացվել է {0} անվանում ապրանքի բեռնում: Ցանկանու՞մ եք ավելացնել ամբողջությամբ։", products.Count), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    break;
                case MessageBoxResult.Cancel:
                    return;
                    break;
                case MessageBoxResult.Yes:
                    insertAll = true;
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var product in products)
            {
                if (insertAll)
                {
                    product.EsMemberId = ApplicationManager.Instance.GetMember.Id;
                    product.LastModifierId = ApplicationManager.GetEsUser.UserId;
                    product.IsEnabled = true;
                    if (ProductsManager.InsertProduct(Convert(product)))
                        insertedProuctCount++;
                    continue;
                }
                if (exitLoop) break;
                result = MessageBox.Show(string.Format("Ցանականու՞մ եք ավելացնել {0} ({1}) ապրանքը։", product.Description, product.Code), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.No:
                        continue;
                    case MessageBoxResult.Cancel:
                        exitLoop = true;
                        break;
                    case MessageBoxResult.OK:

                    case MessageBoxResult.Yes:


                        product.EsMemberId = ApplicationManager.Instance.GetMember.Id;
                        product.LastModifierId = ApplicationManager.GetEsUser.UserId;
                        product.IsEnabled = true;
                        if (ProductsManager.InsertProduct(Convert(product)))
                            insertedProuctCount++;
                        break;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            MessageBox.Show(string.Format("Իրականացվել է {0} անվանում ապրանքի ավելացում:", insertedProuctCount), "Ապրանքների ավելացում", MessageBoxButton.OK);
        }

        private void ImportDsProducts(string fileName)
        {
            //XmlManager.Save(new List<row>(), fileName);
            var products = (List<row>)XmlManager.Read(fileName, typeof(List<row>));
            //var products = (List<EsProductModel>)XmlManager.Read(fileName, typeof(List<EsProductModel>));

            if (products == null || products.Count == 0)
            {
                return;
            }
            var insertedProuctCount = 0;
            bool insertAll = false;
            bool exitLoop = false;
            var result = MessageBox.Show(string.Format("Իրականացվել է {0} անվանում ապրանքի բեռնում: Ցանկանու՞մ եք ավելացնել ամբողջությամբ։", products.Count), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    break;
                case MessageBoxResult.Cancel:
                    return;
                    break;
                case MessageBoxResult.Yes:
                    insertAll = true;
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var item in products)
            {
                var product = new EsProductModel()
                {
                    Code = item.ID.ToString(),
                    Barcode = item.barcode,
                    Description = item.name,
                    Mu = item.unit,
                    CostPrice = (decimal)item.last_price_in,
                    //Price = (decimal) item.price_ret,
                    Note = item.description1
                };
                if (insertAll)
                {
                    product.EsMemberId = ApplicationManager.Instance.GetMember.Id;
                    product.LastModifierId = ApplicationManager.GetEsUser.UserId;
                    product.IsEnabled = true;
                    if (ProductsManager.InsertProduct(Convert(product)))
                        insertedProuctCount++;
                    continue;
                }
                if (exitLoop) break;
                result = MessageBox.Show(string.Format("Ցանականու՞մ եք ավելացնել {0} ({1}) ապրանքը։", product.Description, product.Code), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    case MessageBoxResult.None:
                    case MessageBoxResult.No:
                        continue;
                    case MessageBoxResult.Cancel:
                        exitLoop = true;
                        break;
                    case MessageBoxResult.OK:

                    case MessageBoxResult.Yes:


                        product.EsMemberId = ApplicationManager.Instance.GetMember.Id;
                        product.LastModifierId = ApplicationManager.GetEsUser.UserId;
                        product.IsEnabled = true;
                        if (ProductsManager.InsertProduct(Convert(product)))
                            insertedProuctCount++;
                        break;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            MessageBox.Show(string.Format("Իրականացվել է {0} անվանում ապրանքի ավելացում:", insertedProuctCount), "Ապրանքների ավելացում", MessageBoxButton.OK);
        }

        private ProductModel Convert(EsProductModel item)
        {
            return new ProductModel()
            {
                Code = item.Code,
                Barcode = item.Barcode,
                HcdCs = item.HcdCs,
                Description = item.Description,
                Mu = item.Mu,
                CostPrice = item.CostPrice,
                Price = item.Price,
                ExpiryDays = item.ExpiryDays,
                IsWeight = item.IsWeight,
                EsMemberId = item.EsMemberId,
                LastModifierId = item.LastModifierId,
                IsEnabled = item.IsEnabled
            };
        }

        #endregion

        #region Users

        public void OnLogoff(object o)
        {
            if (MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք դուրս գալ համակարգից:", "Աշխատանքի ավարտ", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var handler = OnLogOut;
                if (handler != null)
                {
                    handler();
                }
            }
        }

        public void OnChangePassword(object o)
        {
            new ChangePassword(ApplicationManager.GetEsUser).Show();
        }

        #endregion Users

        #region Documents

        private List<InvoiceModel> GetInvoices(InvoiceState state, InvoiceType type, int count)
        {
            switch (state)
            {
                case InvoiceState.All:
                    switch (type)
                    {
                        case InvoiceType.SaleInvoice:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetMember.Id);
                            break;
                        case InvoiceType.MoveInvoice:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetMember.Id);
                            break;
                        case InvoiceType.PurchaseInvoice:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetMember.Id);
                            break;
                        case InvoiceType.InventoryWriteOff:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetMember.Id);
                            break;
                        case InvoiceType.ProductOrder:
                            break;
                        case InvoiceType.Statement:
                            break;
                        default:
                            return null;
                            break;
                    }
                    return null;
                case InvoiceState.New:
                    return null;

                case InvoiceState.Accepted:
                    return InvoicesManager.GetInvoicesDescriptions(type, count, ApplicationManager.Instance.GetMember.Id);

                case InvoiceState.Saved:
                    return InvoicesManager.GetUnacceptedInvoicesDescriptions(type, ApplicationManager.Instance.GetMember.Id);

                case InvoiceState.Approved:
                    return InvoicesManager.GetInvoicesDescriptions(type, ApplicationManager.Instance.GetMember.Id);
            }
            return null;
        }

        #endregion Documents

        #region Cash desk

        private bool CanExecuteEcrAction(object o)
        {
            var actionMode = o as EcrExecuiteActions?;
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                case EcrExecuiteActions.OperatorLogin:
                case EcrExecuiteActions.PrintReturnTicket:
                case EcrExecuiteActions.PrintLatestTicket:
                case EcrExecuiteActions.PrintReportX:
                case EcrExecuiteActions.PrintReportZ:
                case EcrExecuiteActions.PrintReceiptTicket:
                    return true;
                case EcrExecuiteActions.CheckEcrConnection:
                    break;
                case EcrExecuiteActions.Zero:
                    break;
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    break;
                case EcrExecuiteActions.LogoutOperator:
                    break;
                case EcrExecuiteActions.ManageHeaderAndFooter:
                    break;
                case EcrExecuiteActions.ManageLogo:
                    break;
                case EcrExecuiteActions.PrintEcrReport:
                    break;
                case EcrExecuiteActions.GetReceiptData:
                    break;
                case EcrExecuiteActions.CashWithdrawal:
                case EcrExecuiteActions.CashIn:
                    return true;
                    break;
                case EcrExecuiteActions.PrintCash:
                    break;
               
                    break;
                case null:
                    break;
                default:
                    break;
            }
            return false;
        }

        private void ExecuteEcrAction(object o)
        {
            var actionMode = o as EcrExecuiteActions?;
            var ecrserver = new EcrServer(ApplicationManager.Settings.MemberSettings.EcrConfig);
            IsLoading = true;
            MessageModel message = null;
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                    message = ecrserver.TryConnection() ? new MessageModel("ՀԴՄ կապի ստուգումն իրականացել է հաջողությամբ: " + ecrserver.ActionDescription, MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.OperatorLogin:
                    message = ecrserver.TryOperatorLogin() ? new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReturnTicket:
                    var resoult = MessageBox.Show("Դուք ցանկանու՞մ եք վերադարձնել ՀԴՄ կտրոնն ամբողջությամբ:", "ՀԴՄ կտրոնի վերադարձ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resoult == MessageBoxResult.Yes)
                    {
                        message = ecrserver.PrintReceiptReturnTicket(true) ? new MessageModel("ՀԴՄ կտրոնի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Error);
                    }
                    else if (resoult == MessageBoxResult.No)
                    {
                        message = ecrserver.PrintReceiptReturnTicket(false) ? new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Error);
                    }
                    else
                    {
                        message = new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումն ընդհատվել է:", MessageTypeEnum.Warning);
                    }
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintLatestTicket:
                    message = ecrserver.PrintReceiptLatestCopy() ? new MessageModel("ՀԴՄ վերջին կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ վերջին կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReportX:
                    message = ecrserver.GetReport(ReportType.X) ? new MessageModel("ՀԴՄ X հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ X հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReportZ:
                    message = ecrserver.GetReport(ReportType.Z) ? new MessageModel("ՀԴՄ Z հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ Z հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.CheckEcrConnection:
                    break;
                case EcrExecuiteActions.Zero:
                    break;
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    break;
                case EcrExecuiteActions.LogoutOperator:
                    break;
                case EcrExecuiteActions.PrintReceiptTicket:
                    var ecrServer = new EcrServer(ApplicationManager.Settings.MemberSettings.EcrConfig);
                    var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ApplicationManager.Settings.MemberSettings.EcrConfig.ExcelFilePath);
                    if (string.IsNullOrEmpty(filePath))
                    {
                        IsLoading = false;
                        return;
                    }
                    try
                    {
                        if (ecrServer.PrintReceiptFromExcelFile(filePath))
                        {
                            IsLoading = false;
                            MessageManager.OnMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success));
                        }
                        else
                        {
                            IsLoading = false;
                            MessageManager.OnMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումն ընդհատվել է: {1} ({0})", ecrServer.ActionCode, ecrServer.ActionDescription), MessageTypeEnum.Warning));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                    var memberSettings = ApplicationManager.Settings.MemberSettings;
                    memberSettings.EcrConfig.ExcelFilePath = Path.GetDirectoryName(filePath);
                    MemberSettings.Save(memberSettings, memberSettings.MemberId);
                    break;
                case EcrExecuiteActions.ManageHeaderAndFooter:
                    break;
                case EcrExecuiteActions.ManageLogo:
                    break;
                case EcrExecuiteActions.PrintEcrReport:
                    break;
                case EcrExecuiteActions.GetReceiptData:
                    break;
                case EcrExecuiteActions.CashWithdrawal:
                    var partner = SelectItemsManager.SelectPartner();
                    if (partner == null) break;
                    var amountForm = new InputForm("Վերադարձվող գումար");
                    if (amountForm.ShowDialog() != DialogResult.OK) break;
                    if (ecrserver.SetCashWithdrawal(HgConvert.ToDecimal(amountForm.TicketValue), partner.FullName))
                        MessageManager.OnMessage("Կանխավճարի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                    else
                        MessageManager.OnMessage("Կանխավճարի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.PrintCash:
                    break;
                case EcrExecuiteActions.CashIn:
                    var cashInPartner = SelectItemsManager.SelectPartner();
                    if (cashInPartner == null) break;
                    var cashInAmountForm = new InputForm("Կանխավճար");
                    if (cashInAmountForm.ShowDialog() != DialogResult.OK) break;
                    if (ecrserver.SetCashWithdrawal(HgConvert.ToDecimal(cashInAmountForm.TicketValue), cashInPartner.FullName))
                        MessageManager.OnMessage("Կանխավճարի կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                    else
                        MessageManager.OnMessage("Կանխավճարի կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageTypeEnum.Warning);
                    break;
                case null:
                    break;
                default:
                    message = null;
                    break;
            }
            if (message != null)
            {
                MessageManager.OnMessage(message);
            }
            IsLoading = false;
        }

        private void OnGetCashDeskInfo(object o)
        {
            var cashDesks = CashDeskManager.GetCashDesks();
            var partners = PartnersManager.GetPartners();

            if (cashDesks == null || cashDesks.Count == 0)
            {
                return;
            }
            string content, title;
            title = "Դրամարկղի մնացորդի դիտում";
            if (cashDesks == null || partners == null)
            {
                content = "Թերի տվյալներ։\nԽնդրում ենք փորձել մի փոքր ուշ։";
                MessageBox.Show(content, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var cashDeskContent = string.Empty;
            if (!ApplicationManager.IsInRole(UserRoleEnum.Manager))
            {
                cashDesks = CashDeskManager.GetCashDesks(ApplicationManager.Settings.MemberSettings.SaleCashDesks);
                cashDeskContent = cashDesks.Where(s => s.IsCash).Aggregate(cashDeskContent, (current, item) => current + string.Format("Դրամարկղ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N")));
            }
            else
            {
                cashDeskContent += "Կանխիկ դրամարկղ \n";
                foreach (var item in cashDesks.Where(s => s.IsCash))
                {
                    cashDeskContent += string.Format("{0}  - {1} դր․ \n", item.Name, item.Total.ToString("N"));
                }
                cashDeskContent += string.Format("Ընդամենը կանխիկ - {0} դր․ \n\n", cashDesks.Where(s => s.IsCash).Sum(s => s.Total).ToString("N"));

                cashDeskContent += "Անկանխիկ դրամարկղ \n";
                foreach (var item in cashDesks.Where(s => !s.IsCash))
                {
                    cashDeskContent += string.Format("{0}  - {1} դր․ \n", item.Name, item.Total.ToString("N"));
                }
                cashDeskContent += string.Format("Ընդամենը անկանխիկ - {0} դր․ \n\n", cashDesks.Where(s => !s.IsCash).Sum(s => s.Total).ToString("N"));
                cashDeskContent += string.Format("Ընդամենը - {0} դր․ \n\n", cashDesks.Sum(s => s.Total).ToString("N"));
                cashDeskContent += "Պարտքեր \n";
                cashDeskContent += string.Format("Դեբիտորական - {0} դր․ \n", partners.Sum(s => s.Debit).ToString("N"));
                cashDeskContent += string.Format("Կրեդիտորական - {0} դր․ \n\n", partners.Sum(s => s.Credit).ToString("N"));
                cashDeskContent += string.Format("Ընդամենը դրամական միջոցներ - {0} դր․ \n\n", (cashDesks.Sum(s => s.Total) + partners.Sum(s => (s.Debit) + (s.Credit))).ToString("N"));
            }
            MessageBox.Show(cashDeskContent, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExecuteEcrAction(object o)
        {
            var td = new Thread(() => { ExecuteEcrAction(o); });
            td.SetApartmentState(ApartmentState.STA);
            td.Start();
        }

        #endregion

        #region Data

        private bool CanGetProductOrder(object o)
        {
            return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Director" || s.RoleName == "Manager");
        }

        private void OnGetProductOrder(object o)
        {
            var vm = new ProductOrderBySaleViewModel();
            AddDocument(vm);
        }

        private void OnGetSaleProducts(object o)
        {
            var vm = new SaleProductsViewModel(null);
            AddDocument(vm);
        }

        private void OnWriteOffProducts(object o)
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Instance.GetMember.Id)).FirstOrDefault();
            if (stock == null) return;
            var existingProducts = ProductsManager.GetProductItemsByStock(stock.Id, ApplicationManager.Instance.GetMember.Id);
            CreateWriteOffInvoice(existingProducts.Select(s => new Tuple<string, decimal>(s.Product.Code, s.Quantity)).ToList(), stock.Id);
        }

        private void CreateWriteOffInvoice(List<Tuple<string, decimal>> items, long? stockId, string notes = null)
        {
            if (!items.Any())
            {
                OnNewMessage(new MessageModel(DateTime.Now, "Դուրսգրման ենթակա ապրանք գոյություն չունի:", MessageTypeEnum.Information));
                return;
            }
            var vm = new InventoryWriteOffViewModel();
            vm.Invoice.FromStockId = stockId;
            vm.Invoice.Notes = notes;
            foreach (var item in items)
            {
                var code = item.Item1;
                var quantity = item.Item2;
                vm.SetInvoiceItem(code);
                if (vm.InvoiceItem.Product == null)
                {
                    continue;
                }
                vm.InvoiceItem.Quantity = quantity;
                vm.InvoiceItem.Index = vm.InvoiceItems.Count + 1;
                vm.InvoiceItems.Add(vm.InvoiceItem);
                vm.InvoiceItem = new InvoiceItemsModel();
            }
            AddInvoiceDocument(vm);
        }

        private void CreateWriteInInvoice(List<Tuple<string, decimal>> items, long? stockId, string notes = null)
        {
            if (!items.Any())
            {
                OnNewMessage(new MessageModel(DateTime.Now, "Մուտքագրման ենթակա ապրանք գոյություն չունի:", MessageTypeEnum.Information));
                return;
            }
            var vm = new PurchaseInvoiceViewModel();
            vm.ToStock = StockManager.GetStock(stockId);
            vm.Invoice.Notes = notes;
            foreach (var item in items)
            {
                var code = item.Item1;
                var quantity = item.Item2;
                vm.SetInvoiceItem(code);
                if (vm.InvoiceItem.Product == null)
                {
                    continue;
                }
                vm.InvoiceItem.Quantity = quantity;
                vm.InvoiceItem.Index = vm.InvoiceItems.Count + 1;
                vm.InvoiceItems.Add(vm.InvoiceItem);
                vm.InvoiceItem = new InvoiceItemsModel();
            }
            AddInvoiceDocument(vm);
        }

        private void OnWriteOffStockTaking(object o)
        {
            var stockTake = GetOpeningStockTake();
            if (stockTake == null) return;
            var stockTakeItems = StockTakeManager.GetStockTakeItems(stockTake.Id);
            CreateWriteOffInvoice(stockTakeItems.Where(s => s.Balance < 0).Select(s => new Tuple<string, decimal>(s.CodeOrBarcode, -s.Balance ?? 0)).ToList(), stockTake.StockId, string.Format("Գույքագրման համար {0}, ամսաթիվ {1}", stockTake.StockTakeName, stockTake.CreateDate));
        }

        private void OnWriteInStockTaking(object o)
        {
            var stockTake = GetOpeningStockTake();
            if (stockTake == null) return;
            var stockTakeItems = StockTakeManager.GetStockTakeItems(stockTake.Id);
            CreateWriteInInvoice(stockTakeItems.Where(s => s.Balance > 0).Select(s => new Tuple<string, decimal>(s.CodeOrBarcode, s.Balance ?? 0)).ToList(), stockTake.StockId, string.Format("Գույքագրման համար {0}, ամսաթիվ {1}", stockTake.StockTakeName, stockTake.CreateDate));
        }

        #endregion

        #region View

        public void OnGetReport(ReportTypes type)
        {
            switch (type)
            {
                case ReportTypes.ShortReport:
                    AddDocument(new ShortReportViewModel());
                    break;
                case ReportTypes.Report:
                    var ui = new DataReports();
                    ui.DataContext = new ReportsViewModel(ui);
                    ui.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
        }

        private void OnViewProductsByStock(object o)
        {
            var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Instance.GetMember.Id), true);
            AddDocument(new ProductItemsViewModel(stocks));
        }

        private bool CanGetProductHistory(object o)
        {
            return UserRoles.Any(s => s.RoleName == "Manager" || s.RoleName == "SaleManager");
        }

        private void OnGetProductsHistory(object o)
        {
            AddDocument(new ProductHistoryViewModel());
        }

        private void OnViewPackingListForSaller(object o)
        {
            var tuple = o as Tuple<InvoiceType, InvoiceState, MaxInvocieCount>;
            if (tuple != null)
            {
                var type = tuple.Item1;
                var state = tuple.Item2;
                var count = (int)tuple.Item3;
                var invoices = GetInvoices(state, type, count);
                if (invoices == null)
                {
                    OnNewMessage(new MessageModel("Ապրանքագիր չի հայտնաբերվել։", MessageTypeEnum.Information));
                    return;
                }
                invoices = SelectItemsManager.SelectInvoice(invoices, true);
                foreach (var invoiceModel in invoices)
                {
                    var vm = new PackingListForSallerViewModel(invoiceModel.Id);
                    AddDocument(vm);
                }
            }
        }

        #endregion

        #region Help

        #endregion

        #region Manager

        private bool CanManageProcucts(object o)
        {
            return _userRoles.Any(s => s.RoleName == "Manager");
        }

        private void OnManageProducts(object o)
        {
            var vm = new ProductManagerViewModel();
            AddDocument(vm);
            vm.OnProductEdited += ProductItemsToolsViewModel.UpdateProducts;
        }

        private void OnManageProduct(ProductModel product)
        {
            var vm = Documents.OfType<ProductManagerViewModel>().FirstOrDefault();
            if (vm == null)
            {
                vm = new ProductManagerViewModel();
                AddDocument(vm);
            }
            vm.IsActive = true;
            vm.IsSelected = true;
            vm.SetProduct(product);
        }

        private bool CanManagePartners(object o)
        {
            return _userRoles.Any(s => s.RoleName == "Manager");
        }

        private void OnManagePartners(object o)
        {
            AddDocument(new PartnerViewModel());
        }

        #endregion

        #region Settings

        private void OnCheckConnection(object o)
        {
            var server = SelectItemsManager.SelectServers();
            if (server == null || !server.Any())
            {
                MessageManager.OnMessage(new MessageModel("Սերվերը բացակայում է", MessageTypeEnum.Information));
                return;
            }
            if (DatabaseManager.CheckConnection(DatabaseManager.CreateConnectionString(server.First())))
            {
                MessageManager.OnMessage(new MessageModel("Սերվերի կապի ստաւոգումը հաջողվել է։", MessageTypeEnum.Information));
            }
            else
            {
                MessageManager.OnMessage(new MessageModel("Սերվերի կապի ստաւոգումը ձախողվել է։", MessageTypeEnum.Warning));
            }
        }

        #endregion Sttings

        #endregion

        #region Commands

        #region Base

        public ICommand KeyPressedCommand { get; private set; }

        #endregion Base

        #region Admin

        public ICommand ImportProductsCommand { get; private set; }

        public ICommand ExecuteEcrActionCommand
        {
            get { return new RelayCommand(OnExecuteEcrAction, CanExecuteEcrAction); }
        }

        public ICommand LogOutCommand { get; private set; }
        public ICommand ChangePasswordCommand { get; private set; }

        #region ConvertConfigFileCommand

        private ICommand _convertConfigFileCommand;

        public ICommand ConvertConfigFileCommand
        {
            get { return _convertConfigFileCommand ?? (_convertConfigFileCommand = new RelayCommand(OnConvertConfigFile)); }
        }

        private void OnConvertConfigFile(object obj)
        {
            SettingsContainer.ConvertConfigFile(ApplicationManager.Member.Id);
        }

        #endregion ConvertConfigFileCommand

        #endregion Admin

        #region Documents

        #region Get invoices command

        private ICommand _getInvoicesCommand;

        public ICommand GetInvoicesCommand
        {
            get { return _getInvoicesCommand ?? (_getInvoicesCommand = new RelayCommand(OnGetInvoices)); }
        }

        public void OnGetInvoices(object o)
        {
            var tuple = o as Tuple<InvoiceType, InvoiceState, MaxInvocieCount>;
            if (tuple != null)
            {
                var type = tuple.Item1;
                var state = tuple.Item2;
                var count = (int)tuple.Item3;
                List<InvoiceModel> invoices = null;
                switch (state)
                {
                    case InvoiceState.New:
                        switch (type)
                        {
                            case InvoiceType.SaleInvoice:
                                AddInvoiceDocument(new SaleInvoiceViewModel());
                                break;
                            case InvoiceType.MoveInvoice:
                                AddInvoiceDocument(new InternalWaybillViewModel());
                                break;
                            case InvoiceType.PurchaseInvoice:
                                AddInvoiceDocument(new PurchaseInvoiceViewModel());
                                break;
                            case InvoiceType.InventoryWriteOff:
                                AddInvoiceDocument(new InventoryWriteOffViewModel());
                                break;
                            default:
                                break;
                        }
                        return;
                    case InvoiceState.Saved:
                    case InvoiceState.All:
                    case InvoiceState.Accepted:
                    case InvoiceState.Approved:
                        invoices = GetInvoices(state, type, count);
                        break;
                    default:
                        break;
                }
                if (invoices == null)
                {
                    OnNewMessage(new MessageModel("Ապրանքագիր չի հայտնաբերվել։", MessageTypeEnum.Information));
                    return;
                }
                invoices = SelectItemsManager.SelectInvoice(invoices, true);
                InvoiceViewModel vm = null;
                foreach (var invoiceModel in invoices)
                {
                    switch ((InvoiceType)invoiceModel.InvoiceTypeId)
                    {
                        case InvoiceType.SaleInvoice:
                            vm = new SaleInvoiceViewModel(invoiceModel.Id);
                            break;
                        case InvoiceType.MoveInvoice:
                            vm = new InternalWaybillViewModel(invoiceModel.Id);
                            break;
                        case InvoiceType.PurchaseInvoice:
                            vm = new PurchaseInvoiceViewModel(invoiceModel.Id);
                            break;
                        case InvoiceType.ProductOrder:
                            break;
                        case InvoiceType.InventoryWriteOff:
                            vm = new InventoryWriteOffViewModel(invoiceModel.Id);
                            break;
                        case InvoiceType.Statement:
                            break;
                        default:
                            break;
                    }
                    if (vm != null)
                    {
                        AddInvoiceDocument(vm);
                    }
                }
            }
        }

        #endregion Get invoices command

        public ICommand WriteOffProductsCommand { get; private set; }

        #endregion Documents

        #region CashDesk

        public ICommand GetChashDesksInfoCommand
        {
            get { return new RelayCommand(OnGetCashDeskInfo); }
        }

        #region View

        private ICommand _viewDebitByPartnerCommand;

        public ICommand ViewDebitByPartnerCommand
        {
            get { return _viewDebitByPartnerCommand ?? (_viewDebitByPartnerCommand = new RelayCommand<DebitEnum>(OnViewDebitByPartner, CanViewDebitByPartner)); }
        }

        private bool CanViewDebitByPartner(DebitEnum obj)
        {
            return true;
        }

        private void OnViewDebitByPartner(DebitEnum value)
        {
            UserControls.Managers.CashDeskManager.ViewDebitByPartner(value);
        }

        #endregion View

        #region View report

        private ICommand _viewAccountantTableCommand;

        public ICommand ViewAccountantTableCommand
        {
            get { return _viewAccountantTableCommand ?? (_viewAccountantTableCommand = new RelayCommand<AccountingActionsEnum>(OnViewAccountantTable)); }
        }

        private void OnViewAccountantTable(AccountingActionsEnum accountingPlanEnum)
        {
            var dates = SelectManager.GetDateIntermediate();
            if (dates == null) return;
            var vm = new ViewAccountantTableViewModel(dates.Item1, dates.Item2);
            AddInvoiceDocument(vm);
            vm.UpdateAccountingRecords(accountingPlanEnum);
        }

        #endregion View report

        private ICommand _accountingActionCommand;

        public ICommand AccountingActionCommand
        {
            get { return _accountingActionCommand ?? (_accountingActionCommand = new RelayCommand<AccountingPlanEnum>(OnAccountingAction, CanExecuteAccountingAction)); }
        }

        public bool CanExecuteAccountingAction(AccountingPlanEnum accountingPlan)
        {
            switch (accountingPlan)
            {
                case AccountingPlanEnum.Purchase:
                    break;
                case AccountingPlanEnum.AccountingReceivable:
                    break;
                case AccountingPlanEnum.Prepayments:
                    break;
                case AccountingPlanEnum.CashDesk:
                    break;
                case AccountingPlanEnum.Accounts:
                    break;
                case AccountingPlanEnum.EquityBase:
                    break;
                case AccountingPlanEnum.PurchasePayables:
                    break;
                case AccountingPlanEnum.ReceivedInAdvance:
                    break;
                case AccountingPlanEnum.Debit_For_Salary:
                    break;
                case AccountingPlanEnum.Proceeds:
                    break;
                case AccountingPlanEnum.CostPrice:
                    break;
                case AccountingPlanEnum.CostOfSales:
                    break;
                case AccountingPlanEnum.OtherOperationalExpenses:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("accountingPlan", accountingPlan, null);
            }
            return ApplicationManager.IsInRole(UserRoleEnum.Cashier);
        }

        public void OnAccountingAction(AccountingPlanEnum accountingPlan)
        {
            AccountingActionManager.Action(accountingPlan);
        }

        private ICommand _viewAccountingRepaymentByPartnersCommand;

        public ICommand ViewAccountingRepaymentByPartnersCommand
        {
            get { return _viewAccountingRepaymentByPartnersCommand ?? (_viewAccountingRepaymentByPartnersCommand = new RelayCommand(OnViewAccountingRepaymentByPartners)); }
        }

        private void OnViewAccountingRepaymentByPartners(object obj)
        {
            var partners = SelectItemsManager.SelectPartners(true).Select(s => s.Id).ToList();
            if (!partners.Any()) return;
            var dates = SelectManager.GetDateIntermediate();
            if (dates == null) return;
            var repayment = AccountingRecordsManager.GetAccountingRecords(dates.Item1, dates.Item2, (long)AccountingPlanEnum.CashDesk, (long)AccountingPlanEnum.AccountingReceivable);
            var ui = new UIListView(repayment.Where(s => (s.CreditGuidId != null && partners.Contains(s.CreditGuidId.Value))).Select(s => new { Ամսաթիվ = s.RegisterDate, Վճարված = s.Amount, Նշումներ = s.Description }).ToList()) { Title = "Դեբիտորական պարտքի մարում ըստ պատվիրատուների" };
            ui.Show();
        }

        #endregion CashDesk

        #region Data

        public ICommand GetReportCommand { get; private set; }

        public ICommand GetProductsHistoryCommand
        {
            get { return new RelayCommand(OnGetProductsHistory, CanGetProductHistory); }
        }

        #region View

        public ICommand ViewInternalWayBillCommands { get; private set; }

        #region View Packing List For Saller Command

        private ICommand _viewPackingListForSallerCommand;

        public ICommand ViewPackingListForSallerCommand
        {
            get { return _viewPackingListForSallerCommand ?? (_viewPackingListForSallerCommand = new RelayCommand(OnViewPackingListForSaller)); }
        }

        #endregion View Packing List For Saller Command

        #endregion View

        #region Stock Take

        public ICommand WriteOffStockTakingCommand { get; private set; }
        public ICommand WriteInStockTakingCommand { get; private set; }

        #endregion Stock Take

        #endregion

        #region Toolbar buttons

        private ICommand _openCashdeskCommand;

        public ICommand OpenCashdeskCommand
        {
            get { return _openCashdeskCommand ?? (_openCashdeskCommand = new RelayCommand(OnOpenCashDesk, CanOpenCashDesk)); }
        }

        private bool CanOpenCashDesk(object obj)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Cashier) && !string.IsNullOrEmpty(ApplicationManager.Settings.MemberSettings.CashDeskPort);
        }

        private void OnOpenCashDesk(object obj)
        {
            if (!CanOpenCashDesk(obj)) return;
            UserControls.Managers.CashDeskManager.OpenCashDesk();
        }

        #endregion Toolbar buttons

        private ICommand _correctProductsCommand;

        public ICommand CorrectProductsCommand
        {
            get { return _correctProductsCommand ?? (_correctProductsCommand = new RelayCommand(OnCorrectProducts, CanCorrectProducts)); }
        }

        private bool CanCorrectProducts(object obj)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.SaleManager);
        }

        private void OnCorrectProducts(object obj)
        {
            var vm = new CorrectProductsViewModel();
            AddDocument(vm);
            vm.OnProductEdited += ProductItemsToolsViewModel.UpdateProducts;
        }

        public ICommand ManageProductsCommand
        {
            get { return new RelayCommand(OnManageProducts, CanManageProcucts); }
        }

        public ICommand ManageParnersCommand
        {
            get { return new RelayCommand(OnManagePartners, CanManagePartners); }
        }

        public ICommand RefreshCashCommand { get; private set; }

        /// <summary>
        /// Data
        /// </summary>
        public ICommand ProductOrderCommand
        {
            get { return new RelayCommand(OnGetProductOrder, CanGetProductOrder); }
        }

        public ICommand SaleProductsCommand
        {
            get { return new RelayCommand(OnGetSaleProducts); }
        }

        public ICommand ViewProductsByStockCommand
        {
            get { return new RelayCommand(OnViewProductsByStock); }
        }


        /// <summary>
        /// Otehr
        /// </summary>
        public ICommand ExportProductsForScaleCommand
        {
            get { return new ExportProductsForScaleCommand(this); }
        }

        #region Help

        #region OpenCarculatorCommand

        public ICommand OpenCarculatorCommand
        {
            get { return new RelayCommand(OnOpenCalc); }
        }

        public void OnOpenCalc(object obj)
        {
            Process.Start("calc");
        }

        #endregion OpenCarculatorCommand

        public ICommand PrintSampleInvoiceCommand
        {
            get { return new PrintSampleInvoiceCommand(); }
        }

        #region Tool

        private ICommand _toolsCommand;

        public ICommand ToolsCommand
        {
            get
            {
                if (_toolsCommand == null) _toolsCommand = new RelayCommand<ToolsEnum>(OnTools, CanExecuteTools);
                return _toolsCommand;
            }
        }

        public bool CanExecuteTools(ToolsEnum toolsEnum)
        {
            switch (toolsEnum)
            {
                case ToolsEnum.Log:
                    break;
                case ToolsEnum.ProductItems:
                    break;
                case ToolsEnum.Categories:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("toolsEnum", toolsEnum, null);
            }
            return true;
        }

        public void OnTools(ToolsEnum toolsEnum)
        {
            ToolsViewModel tool = null;
            switch (toolsEnum)
            {
                case ToolsEnum.Log:
                    tool = new LogViewModel();
                    AddTools<LogViewModel>(tool, false);
                    break;
                case ToolsEnum.ProductItems:
                    tool = new ProductItemsToolsViewModel();
                    AddTools<ProductItemsToolsViewModel>(tool, false);
                    break;
                case ToolsEnum.Categories:
                    tool = new CategoriesToolsViewModel();
                    ((CategoriesToolsViewModel)tool).OnSetCategory += OnSetCategory;
                    AddTools<CategoriesToolsViewModel>(tool, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("toolsEnum", toolsEnum, null);
            }
        }

        public void OnSetCategory(EsCategoriesModel category)
        {
            var activeDocument = Documents.FirstOrDefault(s => s.IsActive);
            var productManager = activeDocument as ProductManagerViewModelBase;
            if (productManager != null)
            {
                productManager.SetProductCategory(category);
            }
        }

        #endregion Categories tools

        #endregion Help

        #region Settings

        public ICommand ChangeSettingsCommand
        {
            get { return new ChangeSettingsCommand(this); }
        }

        public ICommand PrintPriceTicketCommand { get; private set; }

        #region Stock tacking

        private ICommand _stockTakingCommand;

        public ICommand StockTakingCommand
        {
            get { return _stockTakingCommand ?? (_stockTakingCommand = new RelayCommand<ProjectCreationEnum?>(OnGetStockTaking, CanGetStockTaking)); }
        }

        private bool CanGetStockTaking(ProjectCreationEnum? e)
        {
            if (e == null) return false;
            switch (e.Value)
            {
                case ProjectCreationEnum.New:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper);
                    break;
                case ProjectCreationEnum.Edit:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper);
                    break;
                case ProjectCreationEnum.EditLast:
                    break;
                case ProjectCreationEnum.View:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper);
                    break;
                case ProjectCreationEnum.ViewLast:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        private void OnGetStockTaking(ProjectCreationEnum? e)
        {
            if (e == null || !CanGetStockTaking(e)) return;
            StockTakeModel stockTake = null;
            StockTakeBaseViewModel vm = null;
            switch (e.Value)
            {
                case ProjectCreationEnum.New:
                    if (MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք ստեղծել նոր գույքագրում:", "Նոր գույքագրման ստեղծում", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(_member.Id));
                    if (stocks.Count == 0)
                    {
                        MessageManager.OnMessage(new MessageModel("Պահեստի բացակայում", MessageTypeEnum.Warning));
                        return;
                    }
                    stockTake = StockTakeManager.CreateStockTaking(stocks.First().Id, _user.UserId, _member.Id);
                    vm = new StockTakeManagerViewModel(stockTake);
                    break;
                case ProjectCreationEnum.Edit:
                    stockTake = GetOpeningStockTake();
                    if (stockTake != null)
                    {
                        vm = new StockTakeManagerViewModel(stockTake);
                    }
                    break;
                case ProjectCreationEnum.EditLast:
                    break;
                case ProjectCreationEnum.View:
                    stockTake = GetOpeningStockTake(true);
                    if (stockTake != null)
                    {
                        vm = new StockTakeViewModel(stockTake);
                    }
                    break;
                case ProjectCreationEnum.ViewLast:
                    stockTake = GetLastStockTake();
                    if (stockTake != null)
                    {
                        if (stockTake.ClosedDate.HasValue)
                            vm = new StockTakeManagerViewModel(stockTake);
                        else
                            vm = new StockTakeViewModel(stockTake);
                    }
                    break;
                default:
                    break;
            }
            if (stockTake == null)
            {
                MessageManager.OnMessage(new MessageModel("Գործողությունն ընդհատված է։", MessageTypeEnum.Warning));
                return;
            }
            AddInvoiceDocument(vm);
        }

        private StockTakeModel GetLastStockTake()
        {
            var stockTaking = StockTakeManager.GetLastStockTake();
            if (stockTaking == null)
            {
                MessageBox.Show("Որևէ հաշվառում չի հայտնաբերվել։", "Հաշվառում", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            return stockTaking;
        }

        #endregion Stock tacking

        public ICommand ChangeServerSettingsCommand
        {
            get { return new RelayCommand(OnChangeServerSettings); }
        }

        public ICommand RemoveServerSettingsCommand
        {
            get { return new RelayCommand(OnRemoveServerSettings); }
        }

        public ICommand SyncronizeServerDataCommand
        {
            get { return new RelayCommand(OnSyncronizeServerData, CanSyncronizeServerData); }
        }

        public ICommand SyncronizeProductsCommand
        {
            get { return new RelayCommand(OnSyncronizeProducts); }
        }

        #region Edit users command

        private ICommand _editUsersCommand;


        public ICommand EditUsersCommand
        {
            get { return _editUsersCommand ?? (_editUsersCommand = new RelayCommand(OnEditUsers, CanEditUserCommand)); }
        }

        #endregion Edit users command

        public ICommand CheckConnectionCommand { get; private set; }

        #endregion Settings

        #endregion Commands

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
