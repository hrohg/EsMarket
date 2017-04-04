using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Interfaces;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using ES.Market.Views.Reports.View;
using ES.Shop.Commands;
using ES.Shop.Config;
using ES.Shop.Controls;
using ES.Shop.Users;
using ES.Shop.Views;
using ES.Shop.Views.Reports.View;
using ES.Shop.Views.Reports.ViewModels;
using Shared.Helpers;
using UserControls.PriceTicketControl;
using UserControls.ControlPanel.Controls;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl;
using UserControls.PriceTicketControl.Helper;
using UserControls.PriceTicketControl.Implementations;
using UserControls.PriceTicketControl.ViewModels;
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
using UserControls.Views.CustomControls;
using Application = System.Windows.Application;
using ExportManager = ES.Business.Managers.ExportManager;
using ItemsToSelect = UserControls.ControlPanel.Controls.ItemsToSelect;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;
using TabControl = System.Windows.Controls.TabControl;
using UserControl = System.Windows.Controls.UserControl;

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
        private const string PrintCashReceiptProperty = "PrintCashReceipt";
        private const string MessagesProperty = "Messages";
        private const string MessageProperty = "Message";
        #endregion

        #region Internal properties
        private MarketShell _parentTabControl;
        private EsUserModel _user;
        private EsMemberModel _member;
        private List<MembersRoles> _userRoles;
        private bool _isLocalMode;
        private bool _isEcrActivated;
        private ObservableCollection<MessageModel> _messages = new ObservableCollection<MessageModel>();
        private UctrlLibraryBrowser _libraryBrowser;
        private bool _isLoading;
        private bool _isCashUpdateing;

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
                    _productItemsToolsViewModel.OnManageProduct += OnSetProduct;
                }
                return _productItemsToolsViewModel;
            }
        }
        #endregion ProductitemsToolViewModel

        #endregion

        #region External properties

        #region Avalon dock
        public ObservableCollection<DocumentViewModel> Documents { get; set; }
        public ObservableCollection<ToolsViewModel> Tools { get; set; }
        #endregion Avalon dock

        public EsUserModel User { get { return _user; } set { _user = value; OnPropertyChanged(UserProperty); } }
        public EsMemberModel Member { get { return _member; } set { _member = value; OnPropertyChanged(MemberProperty); } }
        public List<MembersRoles> UserRoles { get { return _userRoles; } set { _userRoles = value; OnPropertyChanged(UserRolesProperty); } }
        public string ProcessDescription { get; set; }
        public bool IsCashUpdateing
        {
            get { return _isCashUpdateing; }
            set
            {
                if (_isCashUpdateing == value) _isCashUpdateing = value;
                OnPropertyChanged("IsCashUpdateing");
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
                OnPropertyChanged("IsLoading");
            }
        }
        public bool IsLocalMode { get { return _isLocalMode; } set { _isLocalMode = value; OnPropertyChanged(IsLocalModeProperty); } }
        public bool IsEcrActivated
        {
            get { return ApplicationManager.IsEcrActivated && _isEcrActivated; }
            set
            {
                if (value == _isEcrActivated)
                {
                    _isEcrActivated = !value;
                }
                else
                {
                    _isEcrActivated = value;
                }
                OnPropertyChanged(PrintCashReceiptProperty);
                OnPropertyChanged("IsEcrActivated");
            }
        }
        public ObservableCollection<InvoiceViewModel> Invoices = new ObservableCollection<InvoiceViewModel>();
        public ObservableCollection<MessageModel> Messages { get { return new ObservableCollection<MessageModel>(_messages.OrderByDescending(s => s.Date)); } set { _messages = value; OnPropertyChanged(MessagesProperty); OnPropertyChanged(MessageProperty); } }
        public MessageModel Message { get { return Messages.LastOrDefault(); } }
        public string ServerDescription { get { return ApplicationManager.IsEsServer ? string.Format("{0}: {1}", "Cloud", Member.FullName) : string.Format("{0}: {1}", "Local server", Member.FullName); } }
        public string ServerPath
        {
            get
            {
                return ApplicationManager.GetServerPath().OriginalString;
            }
        }
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
            Member = ApplicationManager.Instance.GetEsMember;
            UserRoles = ApplicationManager.Instance.UserRoles;
            IsLocalMode = ApplicationManager.LocalMode;
            IsEcrActivated = ApplicationManager.IsEcrActivated;

            Documents = new ObservableCollection<DocumentViewModel>();
            Tools = new ObservableCollection<ToolsViewModel>();
            ApplicationManager.MessageManager.NewMessageReceived += OnNewMessage;
            Tools.Add(LogViewModel);
            AddDocument(new StartPageViewModel(this));
            ApplicationManager.Instance.CashProvider.UpdateCash();
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
            ViewPackingListForSallerCommand = new RelayCommand(OnViewPackingListForSaller);

            LogOutCommand = new RelayCommand(OnLogoff);
            ChangePasswordCommand = new RelayCommand(OnChangePassword);
            //Data
            GetInvoicesCommand = new RelayCommand(OnGetInvoices);
            WriteOffProductsCommand = new RelayCommand(OnWriteOffProducts);
            WriteOffStockTakingCommand = new RelayCommand(OnWriteOffStockTaking);
            WriteInStockTakingCommand = new RelayCommand(OnWriteInStockTaking);
            GetReportCommand = new RelayCommand<ReportTypes>(OnGetReport);

            #region Help
            PrintPriceTicketCommand = new RelayCommand<PrintPriceTicketEnum?>(OnPrintPriceTicket, CanPrintPriceTicket);
            #endregion Help

            #region Settings
            EditUsersCommand = new RelayCommand(OnEditUsers, CanEditUserCommand);
            CheckConnectionCommand = new RelayCommand(OnCheckConnection);
            #endregion Settings

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
            ITabItem viewModel = null;
            switch (o)
            {
                case ViewInvoicesEnum.None:
                    viewModel = new InternalWayBillViewModel(o);
                    break;
                case ViewInvoicesEnum.ByStock:
                    break;
                case ViewInvoicesEnum.ByDetiles:
                    viewModel = new InternalWayBillDetileViewModel(o);
                    break;
                default:
                    break;
            }
            var tabControl = new UctrlViewTable { DataContext = viewModel };
            AddTabControl(tabControl, viewModel);
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
        private void AddDocument(DocumentViewModel vm)
        {
            if (vm.IsClosable)
            {
                vm.OnClosed += OnRemoveDocument;
            }
            vm.IsActive = true;
            vm.IsSelected = true;
            vm.Id = Guid.NewGuid();
            Documents.Add(vm);
        }
        private void OnRemoveDocument(PaneViewModel vm)
        {
            if (vm == null) return;
            vm.OnClosed -= OnRemoveDocument;
            Documents.Remove((DocumentViewModel)vm);
            if (vm is ProductManagerViewModel)
            {
                ((ProductManagerViewModel)vm).OnProductEdited -= ProductItemsToolsViewModel.UpdateProducts;
            }
            if (vm is InvoiceViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((InvoiceViewModel)vm).OnSetProductItem;
            } if (vm is StockTakeViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((StockTakeViewModel)vm).OnSetProductItem;
            }
        }
        private void AddTools(ToolsViewModel vm)
        {
            vm.Id = Guid.NewGuid();
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
                    if (!ApplicationManager.Instance.IsInRole(UserRoleEnum.Saller)) return;
                    //handle X key reate new sale invoice
                    if (_userRoles.FirstOrDefault(s => s.RoleName == "Seller") == null &&
                        _userRoles.FirstOrDefault(s => s.RoleName == "SaleManager") == null &&
                        _userRoles.FirstOrDefault(s => s.RoleName == "Manager") == null) { break; }
                    OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.SaleInvoice, InvoiceState.New, MaxInvocieCount.All));
                    break;
                case Key.F3:
                    if (!ApplicationManager.Instance.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (_userRoles.FirstOrDefault(s => s.RoleName == "Manager") == null) break;
                    OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.PurchaseInvoice, InvoiceState.New, MaxInvocieCount.All));
                    break;
                case Key.F4:
                    if (!ApplicationManager.Instance.IsInRole(UserRoleEnum.StockKeeper)) return;
                    //handle X key reate new sale invoice
                    if (_userRoles.FirstOrDefault(s => s.RoleName == "Storekeeper") == null) break;
                    OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.MoveInvoice, InvoiceState.New, MaxInvocieCount.All));
                    break;
                case Key.F5:
                //Used
                case Key.F6:
                    //Used
                    break;
                case Key.F7:
                    if (!ApplicationManager.Instance.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (_userRoles.FirstOrDefault(s => s.RoleName != "Manager" || s.RoleName == "Storekeeper" || s.RoleName != "Seller") == null) break;
                    OnGetInvoices(new Tuple<InvoiceType, InvoiceState, MaxInvocieCount>(InvoiceType.ProductOrder, InvoiceState.New, MaxInvocieCount.All));
                    break;
                case Key.F8:
                    if (!ApplicationManager.Instance.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key logoff
                    if (_userRoles.FirstOrDefault(s => s.RoleName != "Manager") == null) break;
                    //MiManageProducts_Click(null, null);
                    break;
                case Key.F9:
                    //Used
                    break;
                case Key.F10:
                    //handle X key quite
                    if (MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք ավարտել աշխատանքը:", "Աշխատանքի ավարտ",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _parentTabControl.Close();
                    }
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
            var fromStocks = StockManager.GetStocks(xml.GetItemsByControl(XmlTagItems.SaleStocks).Select(s => HgConvert.ToInt64(s.Value)).ToList(), Member.Id).ToList();
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
            OnPropertyChanged(MessagesProperty);
        }

        protected void AddTabControl(UserControl control, ITabItem vm)
        {
            int nextTab;
            var tabShop = _parentTabControl.FindChild<TabControl>();
            if (tabShop == null || control == null || vm == null)
            {
                return;
            }

            nextTab = tabShop.Items.Add(new TabItem
            {
                Content = control,
                DataContext = vm,
                AllowDrop = true
            });
            tabShop.SelectedIndex = nextTab;
            _parentTabControl.UpdateLayout();
        }

        private void CreateNewControl(object vm)
        {
            if (vm is DocumentViewModel)
            {
                AddDocument((DocumentViewModel)vm);
                ((PaneViewModel)vm).IsActive = true;
                if (vm is InvoiceViewModel)
                {
                    ProductItemsToolsViewModel.OnProductItemSelected += ((InvoiceViewModel)vm).OnSetProductItem;
                }
                if (vm is StockTakeViewModel)
                {
                    ProductItemsToolsViewModel.OnProductItemSelected += ((StockTakeViewModel)vm).OnSetProductItem;
                }
                OnCreateProductItemsTools(ProductItemsToolsViewModel);
                return;
            }
            int nextTab;
            var tabShop = _parentTabControl.FindChild<TabControl>();
            if (tabShop == null)
            {
                return;
            }
            //CreateLibraryBrowser();
            var content = new InventoryWriteOffUctrl() { DataContext = vm };
            if (vm is InventoryWriteOffViewModel)
            {
                nextTab = tabShop.Items.Add(new TabItem
                {
                    Content = content,
                    DataContext = vm,
                    AllowDrop = true
                });
                tabShop.SelectedIndex = nextTab;
                _parentTabControl.UpdateLayout();
                return;
            }
            if (vm is SaleInvoiceViewModel)
            {
                nextTab = tabShop.Items.Add(new TabItem
                {
                    Content = new SaleUctrl(_user, _member, (SaleInvoiceViewModel)vm),
                    DataContext = vm,
                    AllowDrop = true
                });
                tabShop.SelectedIndex = nextTab;
                _parentTabControl.UpdateLayout();
                return;
            }

            if (vm is PurchaseInvoiceViewModel)
            {
                nextTab = tabShop.Items.Add(new TabItem
                {
                    Content = new PurchaseUctrl(_user, _member, (PurchaseInvoiceViewModel)vm),
                    DataContext = vm,
                    AllowDrop = true
                });
                tabShop.SelectedIndex = nextTab;
                _parentTabControl.UpdateLayout();
                return;
            }
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
            if (vm is InternalWaybillInvoiceModel)
            {
                nextTab = tabShop.Items.Add(new TabItem
                {
                    Content = new UctrlMoveingInvoice(_user, _member, (InternalWaybillInvoiceModel)vm),
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
                CreateNewControl(model);
            }
        }

        protected void OnCreateProductItemsTools(ProductItemsToolsViewModel vm)
        {
            if (!Tools.Any(t => t is ProductItemsToolsViewModel))
            {
                Tools.Add(vm);
            }
        }

        private bool CanGetStockTaking(object o)
        {
            var projectMode = o as ProjectCreationEnum?;
            if (projectMode == null) return false;
            switch (projectMode)
            {
                case ProjectCreationEnum.New:
                    return _userRoles.Any(s => s.RoleName == "Director" || s.RoleName == "Manager" || s.RoleName == "Storekeeper");
                    break;
                case ProjectCreationEnum.Edit:
                    return _userRoles.Any(s => s.RoleName == "Director" || s.RoleName == "Manager" || s.RoleName == "Storekeeper" || s.RoleName == "SaleManager");
                    break;
            }
            return true;
        }

        private void OnGetStockTaking(object o)
        {
            var projectMode = o as ProjectCreationEnum?;
            if (projectMode == null) return;
            StockTakeModel stockTake = null;
            StockTakeViewModel vm = null;
            switch (projectMode)
            {
                case ProjectCreationEnum.New:
                    if (MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք ստեղծել նոր գույքագրում:", "Նոր գույքագրման ստեղծում", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                    var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(_member.Id));
                    if (stocks.Count == 0)
                    {
                        ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Պահեստի բացակայում", MessageModel.MessageTypeEnum.Warning));
                        return;
                    }
                    stockTake = StockTakeManager.CreateStockTaking(stocks.First().Id, _user.UserId, _member.Id);
                    break;
                case ProjectCreationEnum.Edit:
                    stockTake = GetOpeningStockTake();
                    break;
                default:
                    break;
            }
            if (stockTake == null)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողությունն ընդհատված է։", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            vm = new StockTakeViewModel(stockTake, _member.Id);
            AddDocument(vm);
        }

        private StockTakeModel GetOpeningStockTake()
        {
            var openingStockTaking = StockTakeManager.GetOpeningStockTakes(_member.Id);
            if (openingStockTaking == null || openingStockTaking.Count == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Բաց գույքագրում առկա չէ։", MessageModel.MessageTypeEnum.Warning));
                return null;
            }
            var selectItemId = SelectManager.GetSelectedItem(openingStockTaking.OrderByDescending(s => s.CreateDate).Select(s => new ItemsToSelect
            {
                DisplayName = s.StockTakeName + " " + s.CreateDate,
                SelectedValue = s.Id
            }).ToList(), false).Select(s => (Guid)s.SelectedValue).FirstOrDefault();
            return openingStockTaking.FirstOrDefault(s => s.Id == selectItemId);
        }

        private void OnChangeServerSettings(object o)
        {
            var server = SelectItemsManager.SelectServer(ConfigSettings.GetDataServers());
            new ServerConfig(new ServerViewModel(server ?? new DataServer())).Show();
        }

        private void OnRemoveServerSettings(object o)
        {
            var dataServer = SelectItemsManager.SelectServer(ConfigSettings.GetDataServers());
            ConfigSettings.RemoveDataServer(dataServer);
        }

        private bool CanSyncronizeServerData(object o)
        {
            var syncronizeMode = o as SyncronizeServersMode? ?? SyncronizeServersMode.None;
            switch (syncronizeMode)
            {
                case SyncronizeServersMode.DownloadMemberData:
                    return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin");
                case SyncronizeServersMode.DownloadUserData:
                    return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin" || s.RoleName == "Director");
                case SyncronizeServersMode.DownloadBaseData:
                    return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin" || s.RoleName == "Director");
                case SyncronizeServersMode.SyncronizeBaseData:
                    return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin" || s.RoleName == "Director" || s.RoleName == "Manager");
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
            if (DatabaseManager.SyncronizeServers(syncronizeMode, ApplicationManager.Instance.GetEsMember.Id))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Տվյալների համաժամանակեցումն իրականացել է հաջողությամբ։", MessageModel.MessageTypeEnum.Success));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Տվյալների համաժամանակեցումը ձախողվել է։", MessageModel.MessageTypeEnum.Error));
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
                    ExportManager.ExportPriceForShtrikhM(SelectItemsManager.SelectProductByCheck(_member.Id, true));
                    break;
                case ExportForScale.Custom:
                    ExportManager.ExportPriceForScaleToXml(SelectItemsManager.SelectProductByCheck(_member.Id, true));
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
            var vm = new SettingsViewModel(_member.Id);
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
            if (printPriceTicketEnum == null || !CanPrintPriceTicket(printPriceTicketEnum))
            {
                return;
            }
            var product = SelectItemsManager.SelectProduct().FirstOrDefault();
            if (product == null) return;
            UserControl priceTicket = null;
            switch (printPriceTicketEnum)
            {
                case PrintPriceTicketEnum.Normal:
                    break;
                case PrintPriceTicketEnum.Small:
                    break;
                case PrintPriceTicketEnum.Large:
                    priceTicket = new UctrlBarcodeX(new BarcodeViewModel(product.Code, product.Barcode, product.Description, product.Price, null));
                    break;
                case PrintPriceTicketEnum.LargePrice:
                    //var priceTicket = new PriceTicketDialog
                    //{
                    //    DataContext = new PriceTicketLargePriceVewModel(product.Code, product.Barcode, product.Description,product.Price, null)
                    //};
                    //priceTicket.Show();
                    priceTicket = new UctrlPriceTicket(new PriceTicketLargePriceVewModel(product.Code, product.Barcode, product.Description, product.Price, null));
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printPriceTicketEnum", printPriceTicketEnum, null);
            }
            PrintManager.PrintPreview(priceTicket, "Գնապիտակ", HgConvert.ToBoolean(printPriceTicketEnum));

            //if (HgConvert.ToBoolean(o))
            //{
            //    PrintManager.Print(ctrl, true);
            //}
            //else
            //{
            //    var pb = new UiPrintPreview(ctrl);
            //    pb.Show();
            //    pb.Print();
            //    pb.Close();
            //}
        }

        public bool IsAdminRule
        {
            get { return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin"); }
        }

        public bool IsAdminOrDirectorRule
        {
            get { return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin" || s.RoleName == "Director"); }
        }

        public bool IsManageRule
        {
            get { return ApplicationManager.Instance.UserRoles.Any(s => s.RoleName == "Admin" || s.RoleName == "Director" || s.RoleName == "Manager"); }
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
            return !ApplicationManager.IsEsServer;
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
                    product.EsMemberId = ApplicationManager.Instance.GetEsMember.Id;
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


                        product.EsMemberId = ApplicationManager.Instance.GetEsMember.Id;
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
                    product.EsMemberId = ApplicationManager.Instance.GetEsMember.Id;
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


                        product.EsMemberId = ApplicationManager.Instance.GetEsMember.Id;
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
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetEsMember.Id);
                            break;
                        case InvoiceType.MoveInvoice:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetEsMember.Id);
                            break;
                        case InvoiceType.PurchaseInvoice:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetEsMember.Id);
                            break;
                        case InvoiceType.InventoryWriteOff:
                            return InvoicesManager.GetInvoices(type, count, ApplicationManager.Instance.GetEsMember.Id);
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
                    break;
                case InvoiceState.Accepted:
                    return InvoicesManager.GetInvoicesDescriptions(type, count, ApplicationManager.Instance.GetEsMember.Id);
                    break;
                case InvoiceState.Saved:
                    return InvoicesManager.GetUnacceptedInvoicesDescriptions(type, ApplicationManager.Instance.GetEsMember.Id);
                    break;
                case InvoiceState.Approved:
                    return InvoicesManager.GetInvoicesDescriptions(type, ApplicationManager.Instance.GetEsMember.Id);
                    break;
            }
            return null;
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
                                CreateNewControl(new SaleInvoiceViewModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember));
                                break;
                            case InvoiceType.MoveInvoice:
                                CreateNewControl(new InternalWaybillInvoiceModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember));
                                break;
                            case InvoiceType.PurchaseInvoice:
                                CreateNewControl(new PurchaseInvoiceViewModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember));
                                break;
                            case InvoiceType.InventoryWriteOff:
                                CreateNewControl(new InventoryWriteOffViewModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember));
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
                    OnNewMessage(new MessageModel("Ապրանքագիր չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
                    return;
                }
                invoices = SelectItemsManager.SelectInvoice(invoices, true);
                InvoiceViewModel vm = null;
                foreach (var invoiceModel in invoices)
                {
                    switch ((InvoiceType)invoiceModel.InvoiceTypeId)
                    {
                        case InvoiceType.SaleInvoice:
                            vm = new SaleInvoiceViewModel(invoiceModel.Id, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
                            break;
                        case InvoiceType.MoveInvoice:
                            vm = new InternalWaybillInvoiceModel(invoiceModel.Id, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
                            break;
                        case InvoiceType.PurchaseInvoice:
                            vm = new PurchaseInvoiceViewModel(invoiceModel.Id, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
                            break;
                        case InvoiceType.ProductOrder:
                            break;
                        case InvoiceType.InventoryWriteOff:
                            vm = new InventoryWriteOffViewModel(invoiceModel.Id, ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
                            break;
                        case InvoiceType.Statement:
                            break;
                        default:
                            break;
                    }
                    if (vm != null)
                    {
                        AddDocument(vm);
                    }
                }
            }
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
                    break;
                case EcrExecuiteActions.PrintCash:
                    break;
                case EcrExecuiteActions.CashIn:
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
            var ecrserver = new EcrServer(ApplicationManager.EcrSettings);
            IsLoading = true;
            MessageModel message = null;
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                    message = ecrserver.TryConnection() ? new MessageModel("ՀԴՄ կապի ստուգումն իրականացել է հաջողությամբ: " + ecrserver.ActionDescription, MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageModel.MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.OperatorLogin:
                    message = ecrserver.TryOperatorLogin() ? new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReturnTicket:
                    var resoult = MessageBox.Show("Դուք ցանկանու՞մ եք վերադարձնել ՀԴՄ կտրոնն ամբողջությամբ:", "ՀԴՄ կտրոնի վերադարձ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resoult == MessageBoxResult.Yes)
                    {
                        message = ecrserver.PrintReceiptReturnTicket(true) ? new MessageModel("ՀԴՄ կտրոնի վերադարձն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Error);
                    }
                    else if (resoult == MessageBoxResult.No)
                    {
                        message = ecrserver.PrintReceiptReturnTicket(false) ? new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Error);
                    }
                    else
                    {
                        message = new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումն ընդհատվել է:", MessageModel.MessageTypeEnum.Warning);
                    }
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintLatestTicket:
                    message = ecrserver.PrintReceiptLatestCopy() ? new MessageModel("ՀԴՄ վերջին կտրոնի տպումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ վերջին կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReportX:
                    message = ecrserver.GetReport(ReportType.X) ? new MessageModel("ՀԴՄ X հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ X հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintReportZ:
                    message = ecrserver.GetReport(ReportType.Z) ? new MessageModel("ՀԴՄ Z հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ Z հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
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
                    var ecrServer = new EcrServer(ApplicationManager.EcrSettings);
                    var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ConfigSettings.GetConfig("EcrExcelFilePath"));
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
                            ApplicationManager.MessageManager.OnNewMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success));
                        }
                        else
                        {
                            IsLoading = false;
                            ApplicationManager.MessageManager.OnNewMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումն ընդհատվել է: {1} ({0})", ecrServer.ActionCode, ecrServer.ActionDescription), MessageModel.MessageTypeEnum.Warning));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    ConfigSettings.SetConfig("EcrExcelFilePath", Path.GetDirectoryName(filePath));
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
                    //message = ecrserver.SetCashWithdrawal() != null ? new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումն իրականացել է հաջողությամբ:", MessageModel.MessageType.Success) : new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageType.Warning);
                    IsLoading = false;
                    break;
                case EcrExecuiteActions.PrintCash:
                    break;
                case EcrExecuiteActions.CashIn:
                    //message = ecrserver.SetCashReceipt() != null ? new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումն իրականացել է հաջողությամբ:", MessageModel.MessageType.Success) : new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageType.Warning);
                    IsLoading = false;
                    break;
                case null:
                    break;
                default:
                    message = null;
                    break;
            }
            if (message != null)
            {
                ApplicationManager.MessageManager.OnNewMessage(message);
            }
            IsLoading = false;
        }

        private void OnGetCashDeskInfo(object o)
        {
            var cashDesks = CashDeskManager.TryGetCashDesk(_member.Id);
            var partners = PartnersManager.GetPartners(_member.Id);

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
            if (!UserRoles.Any(s => s.RoleName == "Manager" || s.RoleName == "Director"))
            {
                cashDesks = new List<CashDesk>
                {
                    CashDeskManager.GetCashDesk(ApplicationManager.GetThisDesk.Id, ApplicationManager.Instance.GetEsMember.Id)
                };
                cashDeskContent = cashDesks.Where(s => s.IsCash).Aggregate(cashDeskContent, (current, item) => current + string.Format("Դրամարկղ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N")));
            }
            else
            {
                foreach (var item in cashDesks.Where(s => s.IsCash))
                {
                    cashDeskContent += string.Format("Դրամարկղ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N"));
                }
                cashDeskContent += string.Format("Կանխիկ դրամարկղ - {0} դր․ \n\n", cashDesks.Where(s => s.IsCash).Sum(s => s.Total).ToString("N"));

                foreach (var item in cashDesks.Where(s => !s.IsCash))
                {
                    cashDeskContent += string.Format("Դրամարկղ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N"));
                }
                cashDeskContent += string.Format("Անկանխիկ դրամարկղ - {0} դր․ \n\n", cashDesks.Where(s => !s.IsCash).Sum(s => s.Total).ToString("N"));
                cashDeskContent += string.Format("Ընդամենը - {0} դր․ \n\n", cashDesks.Sum(s => s.Total).ToString("N"));
                cashDeskContent += string.Format("Դեբիտորական պարտք - {0} դր․ \n", partners.Sum(s => s.Debit ?? 0).ToString("N"));
                cashDeskContent += string.Format("Կրեդիտորական պարտք - {0} դր․ \n\n", partners.Sum(s => s.Credit ?? 0).ToString("N"));
                cashDeskContent += string.Format("Ընդամենը դրամական միջոցներ - {0} դր․ \n\n", (cashDesks.Sum(s => s.Total) + partners.Sum(s => (s.Debit ?? 0) + (s.Credit ?? 0))).ToString("N"));
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
            var viewModel = new ProductOrderBySaleViewModel(null);
            AddTabControl(new UctrlViewTable { DataContext = viewModel }, viewModel);
        }

        private void OnGetSaleProducts(object o)
        {
            var viewModel = new SaleProductsViewModel(null);
            AddTabControl(new UctrlViewTable { DataContext = viewModel }, viewModel);
        }

        private void OnWriteOffProducts(object o)
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Instance.GetEsMember.Id)).FirstOrDefault();
            if (stock == null) return;
            var existingProducts = ProductsManager.GetProductItemsByStock(stock.Id, ApplicationManager.Instance.GetEsMember.Id);
            CreateWriteOffInvoice(existingProducts.Select(s => new Tuple<string, decimal>(s.Product.Code, s.Quantity)).ToList(), stock.Id);
        }

        private void CreateWriteOffInvoice(List<Tuple<string, decimal>> items, long? stockId, string notes = null)
        {
            if (!items.Any())
            {
                OnNewMessage(new MessageModel(DateTime.Now, "Դուրսգրման ենթակա ապրանք գոյություն չունի:", MessageModel.MessageTypeEnum.Information));
                return;
            }
            var vm = new InventoryWriteOffViewModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
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
            AddDocument(vm);
        }

        private void CreateWriteInInvoice(List<Tuple<string, decimal>> items, long? stockId, string notes = null)
        {
            if (!items.Any())
            {
                OnNewMessage(new MessageModel(DateTime.Now, "Մուտքագրման ենթակա ապրանք գոյություն չունի:", MessageModel.MessageTypeEnum.Information));
                return;
            }
            var vm = new PurchaseInvoiceViewModel(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
            vm.ToStock = StockManager.GetStock(stockId, ApplicationManager.Instance.GetEsMember.Id);
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
            AddDocument(vm);
        }

        private void OnWriteOffStockTaking(object o)
        {
            var stockTake = GetOpeningStockTake();
            if (stockTake == null) return;
            var stockTakeItems = StockTakeManager.GetStockTakeItems(stockTake.Id, ApplicationManager.Instance.GetEsMember.Id);
            CreateWriteOffInvoice(stockTakeItems.Where(s => s.Balance < 0).Select(s => new Tuple<string, decimal>(s.CodeOrBarcode, -s.Balance ?? 0)).ToList(), stockTake.StockId, string.Format("Գույքագրման համար {0}, ամսաթիվ {1}", stockTake.StockTakeName, stockTake.CreateDate));
        }

        private void OnWriteInStockTaking(object o)
        {
            var stockTake = GetOpeningStockTake();
            if (stockTake == null) return;
            var stockTakeItems = StockTakeManager.GetStockTakeItems(stockTake.Id, ApplicationManager.Instance.GetEsMember.Id);
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
                    var ui = new DataReports(ApplicationManager.GetEsUser, ApplicationManager.Instance.GetEsMember);
                    ui.DataContext = new ReportsViewModel(ui);
                    ui.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("o", type, null);
            }
        }

        private void OnViewProductsByStock(object o)
        {
            var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Instance.GetEsMember.Id), true);
            AddTabControl(new ViewProductsUctrl(), new ProductItemsViewModel(stocks));
        }

        private bool CanGetProductHistory(object o)
        {
            return UserRoles.Any(s => s.RoleName == "Manager" || s.RoleName == "SaleManager");
        }

        private void OnGetProductsHistory(object o)
        {
            AddTabControl(new ProductsHistory(), new ProductHistoryViewModel());
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
                    OnNewMessage(new MessageModel("Ապրանքագիր չի հայտնաբերվել։", MessageModel.MessageTypeEnum.Information));
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

        private void OnSetProduct(ProductModel product)
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
            var server = SelectItemsManager.SelectServer();
            if (server == null)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Սերվերը բացակայում է", MessageModel.MessageTypeEnum.Information));
                return;
            }
            if (DatabaseManager.CheckConnection(DatabaseManager.CreateConnectionString(server)))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Սերվերի կապի ստաւոգումը հաջողվել է։", MessageModel.MessageTypeEnum.Information));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Սերվերի կապի ստաւոգումը ձախողվել է։", MessageModel.MessageTypeEnum.Warning));
            }
        }
#endregion Sttings

        #endregion

        #region ICommands

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

        #endregion Admin

        #region Documents

        public ICommand GetInvoicesCommand { get; private set; }
        public ICommand WriteOffProductsCommand { get; private set; }

        #endregion Documents

        #region CashDesk

        public ICommand GetChashDesksInfoCommand
        {
            get { return new RelayCommand(OnGetCashDeskInfo); }
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
        public ICommand ViewPackingListForSallerCommand { get; private set; }

        #endregion View

        #region Stock Take

        public ICommand WriteOffStockTakingCommand { get; private set; }
        public ICommand WriteInStockTakingCommand { get; private set; }

        #endregion Stock Take

        #endregion

        /// <summary>
        /// Managers
        /// </summary>
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

        public ICommand OpenCarculatorCommand
        {
            get { return new OpenCarculatorCommand(); }
        }

        public ICommand PrintSampleInvoiceCommand
        {
            get { return new PrintSampleInvoiceCommand(); }
        }

        #endregion Help

        #region Settings

        public ICommand ChangeSettingsCommand
        {
            get { return new ChangeSettingsCommand(this); }
        }

        public ICommand PrintPriceTicketCommand { get; private set; }

        public ICommand StockTakingCommand
        {
            get { return new RelayCommand(OnGetStockTaking, CanGetStockTaking); }
        }

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

        public ICommand EditUsersCommand { get; private set; }
        public ICommand CheckConnectionCommand { get; private set; }

        #endregion Settings

        #endregion Commands

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
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
