using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using AccountingTools.Enums;
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
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.Data.Models.Products;
using ES.Market.Commands;
using ES.Market.Config;
using ES.Market.Users;
using ES.Market.Views.Reports.View;
using ES.Market.Views.Reports.ViewModels;
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
using UserControls.ViewModels.Reports;
using UserControls.ViewModels.Reports.Orders;
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
using ProductModel = ES.Data.Models.Products.ProductModel;
using ProductOrderViewModel = UserControls.ViewModels.Reports.Orders.ProductOrderViewModel;
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
        private readonly object _sync = new object();
        private const string IsLocalModeProperty = "IsLocalMode";
        private const string MessagesProperty = "Messages";
        private const string MessageProperty = "Message";
        private Thread _listenSerialPortsThread;
        private SerialPortReader serialPortReader;
        #endregion

        #region Internal properties
        private MarketShell _parentTabControl;
        private bool _isLocalMode;

        //Barcode
        private DateTime _lastKeystroke = new DateTime(0);
        private List<char> _barcode = new List<char>(10);
        private System.Text.StringBuilder _barcodeText = new System.Text.StringBuilder();

        static bool _listenSerialPort;
        static SerialPort _serialPort;

        private ObservableCollection<MessageModel> _messages = new ObservableCollection<MessageModel>();
        //private UctrlLibraryBrowser _libraryBrowser;
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

        public ApplicationSettingsViewModel ApplicationSettings { get { return ApplicationManager.Settings; } }
        public string ProcessDescription { get; set; }
        public bool IsCashUpdateing
        {
            get { return _isCashUpdateing; }
            set
            {
                _isCashUpdateing = value;
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
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }
        public bool IsLocalMode { get { return _isLocalMode; } set { _isLocalMode = value; RaisePropertyChanged(IsLocalModeProperty); } }
        public ObservableCollection<InvoiceViewModel> Invoices = new ObservableCollection<InvoiceViewModel>();
        public ObservableCollection<MessageModel> Messages { get { return new ObservableCollection<MessageModel>(_messages.OrderByDescending(s => s.Date)); } set { _messages = value; RaisePropertyChanged(MessagesProperty); RaisePropertyChanged(MessageProperty); } }
        public MessageModel Message { get { return Messages.LastOrDefault(); } }
        public string UserDescription { get { return ApplicationManager.GetEsUser.FullName; } }
        public string ServerDescription { get { return ApplicationManager.IsEsServer ? string.Format("{0}: {1}", "Cloud", ApplicationManager.Member.Name) : string.Format("{0}: {1}", "Local server", ApplicationManager.Member.Name); } }
        public string ServerPath
        {
            get
            {
                return ApplicationManager.GetServerPath().OriginalString;
            }
        }

        #region Toolbar
        public Visibility AddSingleVisibility
        {
            get
            {
                return ActiveTab is InvoiceViewModel ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsSingle { get; set; }

        #endregion Toolbar

        #endregion

        #region Constructors
        public ShellViewModel()
        {
            Initialize();
        }
        public void Dispose()
        {
            ApplicationManager.CashManager.BeginCashUpdateing -= BeginCashUpdateing;
            ApplicationManager.CashManager.CashUpdated -= CashUpdated;
            _listenSerialPort = false;
            if (serialPortReader != null) serialPortReader.Dispose();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            IsLocalMode = ApplicationManager.Settings.IsOfflineMode;

            ApplicationManager.CashManager.BeginCashUpdateing += BeginCashUpdateing;
            ApplicationManager.CashManager.CashUpdated += CashUpdated;
            IsLoading = ApplicationManager.CashManager.IsUpdateing;
            Documents = new ObservableCollection<DocumentViewModel>();
            Tools = new ObservableCollection<ToolsViewModel>();
            ApplicationManager.MessageManager.MessageReceived += OnNewMessage;
            InitializeCommands();

            Tools.Add(LogViewModel);
            AddDocument(new StartPageViewModel(this) { IsActive = false }); ;

            LoadAutosavedInvoices();
            serialPortReader = new SerialPortReader(ApplicationSettings.SettingsContainer.MemberSettings.ScannerPort, SerialPort_DataReceived);

            //_listenSerialPortsThread = new Thread(ListenSerialPorts);
            //_listenSerialPortsThread.Start();
        }
        private void SerialPort_DataReceived(string data)
        {
            //var vm = Documents.SingleOrDefault(s => s.IsActive);
            var vm = Documents.SingleOrDefault(s => s.IsSelected);
            if (vm != null) DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => { vm.SetExternalText(new ExternalTextImputEventArgs(data)); });
        }
        private void CashUpdated()
        {
            IsLoading = IsCashUpdateing = false;
        }

        private void BeginCashUpdateing()
        {
            IsLoading = IsCashUpdateing = true;
        }

        private void InitializeCommands()
        {
            RefreshCashCommand = new RelayCommand(UpdateCash, CanUpdateCash);
            //Base
            KeyPressedCommand = new RelayCommand<KeyEventArgs>(OnKeyPressed);

            //Admin
            ImportProductsCommand = new RelayCommand(OnImportProducts);
            ViewInternalWayBillCommands = new RelayCommand<ViewInvoicesEnum>(OnViewViewInternalWayBillCommands, CanViewViewInternalWayBillCommands);

            LogOutCommand = new RelayCommand(OnLogoff);
            ChangePasswordCommand = new RelayCommand(OnChangePassword);
            //Data
            WriteOffProductsCommand = new RelayCommand(OnWriteOffProducts);
            GetReportCommand = new RelayCommand<ReportTypes>(OnGetReport);

            #region Help
            PrintPriceTicketCommand = new RelayCommand<PrintPriceTicketEnum?>(OnPrintPriceTicket, CanPrintPriceTicket);
            #endregion Help

            #region Settings

            CheckConnectionCommand = new RelayCommand(OnCheckConnection);
            #endregion Settings

        }

        private void LoadAutosavedInvoices()
        {
            var invoices = InvoicesManager.LoadAutosavedInvoices();
            InvoiceViewModel invoiceVm = null;
            foreach (var invoice in invoices)
            {
                if (invoice == null || invoice.Invoice == null || invoice.InvoiceItems == null) continue;
                switch ((InvoiceType)invoice.Invoice.InvoiceTypeId)
                {
                    case InvoiceType.PurchaseInvoice:
                        invoiceVm = new PurchaseInvoiceViewModel();
                        break;
                    case InvoiceType.SaleInvoice:
                        invoiceVm = new SaleInvoiceViewModel();
                        break;
                    case InvoiceType.ProductOrder:
                        invoiceVm = new PurchaseInvoiceViewModel();
                        break;
                    case InvoiceType.MoveInvoice:
                        invoiceVm = new InternalWaybillViewModel();
                        break;
                    case InvoiceType.InventoryWriteOff:
                        invoiceVm = new InventoryWriteOffViewModel();
                        break;
                    case InvoiceType.ReturnFrom:
                        invoiceVm = new ReturnFromInvoiceViewModel();
                        break;
                    case InvoiceType.ReturnTo:
                        invoiceVm = new ReturnToInvoiceViewModel();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                invoiceVm.Invoice.Reload(invoice.Invoice, ApplicationManager.Member.Id);
                invoiceVm.Title = "AutoSave";
                foreach (var invoiceItemsModel in invoice.InvoiceItems)
                {
                    var code = invoiceItemsModel.Code;
                    var quantity = invoiceItemsModel.Quantity;
                    invoiceVm.SetInvoiceItem(code);
                    if (invoiceVm.InvoiceItem.Product == null)
                    {
                        continue;
                    }
                    invoiceVm.InvoiceItem.Quantity = quantity;
                    invoiceVm.InvoiceItem.Price = invoiceItemsModel.Price;
                    invoiceVm.InvoiceItem.DisplayOrder = (short)(invoiceVm.InvoiceItems.Count + 1);
                    invoiceVm.AddInvoiceItem();
                    invoiceVm.InvoiceItem = new InvoiceItemsModel();
                }
                AddInvoiceDocument(invoiceVm);
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
            if (!document.IsActive) return;
            ActiveTab = document;
            foreach (var doc in Documents)
            {
                if (doc != document && document.IsActive) doc.IsActive = false;
            }
            if (ActiveTab is InvoiceViewModel || ActiveTab is ProductManagerViewModel)
            {
                ProductItemsToolsViewModel.IsActive = true;
            }
        }

        private void AddInvoiceDocument(InvoiceViewModel vm)
        {
            if (vm == null) return;
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
            foreach (var doc in Documents)
            {
                doc.IsSelected = false;
                doc.IsActive = false;
            }
            vm.IsActive = true;
            vm.IsSelected = true;
            lock (_sync)
            {
                Documents.Add(vm);
            }
        }

        private void OnRemoveDocument(PaneViewModel vm)
        {
            if (vm == null) return;
            vm.OnClosed -= OnRemoveDocument;
            if (vm is DocumentViewModel)
            {
                ((DocumentViewModel)vm).ActiveTabChangedEvent -= OnActiveTabChanged;
            }

            lock (_sync)
            {
                Documents.Remove((DocumentViewModel)vm);
            }
            if (vm is InvoiceViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((InvoiceViewModel)vm).OnSetProductItem;
            }
            if (vm is StockTakeBaseViewModel)
            {
                ((StockTakeBaseViewModel)vm).CreateWriteInInvoiceEvent -= OnCreateWriteInInvoice;
                ((StockTakeBaseViewModel)vm).CreateWriteOffInvoiceEvent -= OnCreateWriteOffInvoice;
            }
            if (vm is StockTakeManagerViewModel)
            {
                ProductItemsToolsViewModel.OnProductItemSelected -= ((StockTakeManagerViewModel)vm).OnSetProductItem;
            }
            if (vm is ProductManagerViewModelBase)
            {
                ((ProductManagerViewModelBase)vm).ProductEditedEvent -= OnProductChanged;
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
                        return;
                }
            }
            switch (e.Key)
            {
                case Key.F1:
                    //handle X key reate new sale invoice
                    //
                    break;
                case Key.F2:
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.Seller) || ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller))
                    {
                        OnGetInvoices(new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>(InvoiceTypeEnum.SaleInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    return;
                case Key.F3:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.SaleManager))
                    {
                        OnGetInvoices(new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>(InvoiceTypeEnum.PurchaseInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    return;
                case Key.F4:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.StockKeeper)) return;
                    //handle X key reate new sale invoice
                    if (ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper))
                    {
                        OnGetInvoices(new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>(InvoiceTypeEnum.MoveInvoice, InvoiceState.New, MaxInvocieCount.All));
                    }
                    return;
                case Key.F5:
                //Used
                case Key.F6:
                    //Used
                    break;
                case Key.F7:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key reate new sale invoice
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Seller)) break;
                    OnGetInvoices(new Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>(InvoiceTypeEnum.ProductOrder, InvoiceState.New, MaxInvocieCount.All));
                    return;
                case Key.F8:
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) return;
                    //handle X key logoff
                    if (!ApplicationManager.IsInRole(UserRoleEnum.Manager)) break;
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


            // process barcode
            if (e.Key == Key.Enter)
            {
                string barcode = new string(_barcode.ToArray());
                var vm = Documents.SingleOrDefault(s => s.IsActive);
                if (vm != null) vm.SetExternalText(new ExternalTextImputEventArgs(barcode));
                _barcode.Clear();
                _barcodeText.Clear();
                return;
            }

            // check timing (keystrokes within 100 ms)
            TimeSpan elapsed = (DateTime.Now - _lastKeystroke);
            if (elapsed.TotalMilliseconds > 200)
            {
                _barcode.Clear();
                _barcodeText.Clear();
            }

            // record keystroke & timestamp
            var key = KeyboardHelper.KeyToChar(e.Key == Key.System ? e.SystemKey : e.Key);
            if (e.Key == Key.System)
            {
                if (Keyboard.IsKeyDown(Key.NumPad0)) key = '0';
                if (Keyboard.IsKeyDown(Key.NumPad1)) key = '1';
                if (Keyboard.IsKeyDown(Key.NumPad2)) key = '2';
                if (Keyboard.IsKeyDown(Key.NumPad3)) key = '3';
                if (Keyboard.IsKeyDown(Key.NumPad4)) key = '4';
                if (Keyboard.IsKeyDown(Key.NumPad5)) key = '5';
                if (Keyboard.IsKeyDown(Key.NumPad6)) key = '6';
                if (Keyboard.IsKeyDown(Key.NumPad7)) key = '7';
                if (Keyboard.IsKeyDown(Key.NumPad8)) key = '8';
                if (Keyboard.IsKeyDown(Key.NumPad9)) key = '9';
            }


            if (key == '\x00')
            {
                //if (Keyboard.IsKeyDown(Key.NumPad1)) { MessageManager.OnMessage("1" + " " + (int)e.Key); } else { MessageManager.OnMessage((int)e.SystemKey + " " + (int)e.Key); }
                return;
            }
            _barcode.Add(key);
            _barcodeText.Append(key);
            _lastKeystroke = DateTime.Now;
        }

        #endregion Base

        private void OnNewMessage(MessageModel message)
        {
            if (ApplicationManager.IsMainThread)
            {
                LogViewModel.AddLog(message);
            }
            else
            {
                if (System.Windows.Application.Current != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => LogViewModel.AddLog(message)));
                }
            }
            if (_messages != null) _messages.Add(message);
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
                    Content = new ProductOrderUctrl(),
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
            var stocks = ApplicationManager.CashManager.GetStocks;
            if (isClosed)
            {
                var dateIntermediate = UIHelper.Managers.SelectManager.GetDateIntermediate();
                if (dateIntermediate == null)
                {
                    MessageManager.ShowMessage("Գործողությունն ընդհատված է։", "Թերի տվյալներ");
                    return null;
                }
                var startDate = dateIntermediate.Item1;
                var endDate = dateIntermediate.Item2;
                stockTakes = StockTakeManager.GetStockTakeByCreateDate(startDate, endDate);
                if (stockTakes == null || !stockTakes.Any())
                {
                    MessageManager.ShowMessage("Տվյալ ժամանակահատվածում հաշվառում չի իրականացվել։", "Թերի տվյալներ");
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

            var items = new List<ItemsToSelect>();
            foreach (var item in stockTakes.OrderByDescending(s => s.CreateDate))
            {
                var stock = stocks.SingleOrDefault(t => t.Id == item.StockId);
                if (stock == null) continue;
                items.Add(new ItemsToSelect
                {
                    DisplayName = string.Format("{0} {1} ({2})", item.StockTakeName, item.CreateDate, stock.FullName),
                    SelectedValue = item.Id
                });
            }

            var selectItemId = SelectManager.GetSelectedItem(items).Select(s => (Guid)s.SelectedValue).FirstOrDefault();
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
                    return (ApplicationManager.IsInRole(UserRoleEnum.Moderator) || ApplicationManager.IsInRole(UserRoleEnum.Director)) && ApplicationManager.IsEsServer;
                case SyncronizeServersMode.DownloadBaseData:
                    return (ApplicationManager.IsInRole(UserRoleEnum.Moderator) || ApplicationManager.IsInRole(UserRoleEnum.Director)) && !ApplicationManager.IsEsServer;
                case SyncronizeServersMode.SyncronizeBaseData:
                    return (ApplicationManager.IsInRole(UserRoleEnum.Moderator) || ApplicationManager.IsInRole(UserRoleEnum.SeniorManager)) && !ApplicationManager.IsEsServer;
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
            var isResult = DatabaseManager.SyncronizeServers(syncronizeMode);
            MessageManager.OnMessage(isResult
                ? new MessageModel("Տվյալների համաժամանակեցումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success)
                : new MessageModel("Տվյալների համաժամանակեցումը ձախողվել է։", MessageTypeEnum.Error));
            if (isResult) MessageManager.ShowMessage("Փոփոխությունները թարմացնելու համար վերբեռնեք ծրագիրը:", "Բազայի թարմացում", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OnSyncronizeProducts(object o)
        {
            Thread myThread = new Thread(DatabaseManager.SyncronizeProducts);
            myThread.Start();
        }
        #region Data
        private void OnViewParkingLists(InvoiceTypeEnum type)
        {
            _dataIntermidiate = new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Now);
            switch (type)
            {
                case InvoiceTypeEnum.None:
                    return;
                    break;
                case InvoiceTypeEnum.PurchaseInvoice:
                case InvoiceTypeEnum.SaleInvoice:
                case InvoiceTypeEnum.MoveInvoice:
                    _dataIntermidiate = UIHelper.Managers.SelectManager.GetDateIntermediate();
                    break;
                case InvoiceTypeEnum.ProductOrder:
                    break;
                case InvoiceTypeEnum.InventoryWriteOff:
                    break;
                case InvoiceTypeEnum.ReturnFrom:
                    break;
                case InvoiceTypeEnum.ReturnTo:
                    break;
                case InvoiceTypeEnum.Statements:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("invoiceType", type, null);
            }
            var invoices = SelectItemsManager.SelectInvoice(type, _dataIntermidiate, OrderInvoiceBy.ApprovedDate, true);
            OpenPackingLists(invoices);
        }

        #endregion Data

        private void OpenPackingLists(List<InvoiceModel> invoices)
        {
            if (invoices == null) return;
            InvoiceViewModelBase vm = null;
            foreach (var invoiceModel in invoices)
            {
                switch ((InvoiceType)invoiceModel.InvoiceTypeId)
                {
                    case InvoiceType.PurchaseInvoice:
                        vm = new PackingListForSallerViewModel(invoiceModel.Id);
                        vm.TotalAmount = (double)vm.InvoiceItems.Sum(s => (s.ProductPrice ?? 0) * (s.Quantity ?? 0));
                        break;
                    case InvoiceType.SaleInvoice:
                        vm = new PackingListForSallerViewModel(invoiceModel.Id);
                        break;
                    case InvoiceType.ProductOrder:
                        break;
                    case InvoiceType.MoveInvoice:
                        vm = new ViewMoveInvoiceViewModel(invoiceModel.Id);
                        break;
                    case InvoiceType.InventoryWriteOff:

                    case InvoiceType.ReturnFrom:

                    case InvoiceType.ReturnTo:
                        vm = new PackingListForSallerViewModel(invoiceModel.Id);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (vm == null) continue;
                AddDocument(vm);
            }
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
                    ExportManager.ExportPriceForScaleToXml(new ProductsManager().GetProductsBy(ProductViewType.WeigthsOnly));
                    break;
                case ExportForScale.ShtrixK:
                    ExportManager.ExportPriceForShtrikhK(new ProductsManager().GetProductsBy(ProductViewType.All));
                    break;
                case ExportForScale.All:
                    break;
                default:
                    ExportManager.ExportPriceForScaleToXml(ApplicationManager.Instance.CashProvider.GetProducts().ToList());
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

        private bool CanUpdateCash(object o)
        {
            return !IsCashUpdateing; // && IsLocalMode
        }

        private void UpdateCash(object o)
        {
            if (!CanUpdateCash(o))
            {
                return;
            }
            ApplicationManager.Instance.CashProvider.UpdateCashAsync();
        }

        private bool CanEditUserCommand(object o)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.Moderator) || ApplicationManager.IsInRole(UserRoleEnum.Admin);
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
            var result = MessageManager.ShowMessage(string.Format("Իրականացվել է {0} անվանում ապրանքի բեռնում: Ցանկանու՞մ եք ավելացնել ամբողջությամբ։", products.Count), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
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
                result = MessageManager.ShowMessage(string.Format("Ցանականու՞մ եք ավելացնել {0} ({1}) ապրանքը։", product.Description, product.Code), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
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
            MessageManager.ShowMessage(string.Format("Իրականացվել է {0} անվանում ապրանքի ավելացում:", insertedProuctCount), "Ապրանքների ավելացում");
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
            var result = MessageManager.ShowMessage(string.Format("Իրականացվել է {0} անվանում ապրանքի բեռնում: Ցանկանու՞մ եք ավելացնել ամբողջությամբ։", products.Count), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
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
                    //Mu = item.unit,
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
                result = MessageManager.ShowMessage(string.Format("Ցանականու՞մ եք ավելացնել {0} ({1}) ապրանքը։", product.Description, product.Code), "Ապրանքների ավելացում", MessageBoxButton.YesNoCancel);
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
            MessageManager.ShowMessage(string.Format("Իրականացվել է {0} անվանում ապրանքի ավելացում:", insertedProuctCount), "Ապրանքների ավելացում");
        }

        private ProductModel Convert(EsProductModel item)
        {
            return new ProductModel
            {
                Code = item.Code,
                Barcode = item.Barcode,
                HcdCs = item.HcdCs,
                Description = item.Description,
                //MeasureUnit = CashManager.Instance.MeasureOfUnits.SingleOrDefault(s=>s.Id==item.),
                CostPrice = item.CostPrice,
                Price = item.Price,
                ExpiryDays = item.ExpiryDays,
                EsMemberId = item.EsMemberId,
                LastModifierId = item.LastModifierId,
                IsEnabled = item.IsEnabled
            };
        }

        private ICommand _sendMessageCommand;
        public ICommand SendMessageCommand
        {
            get
            {
                return _sendMessageCommand ?? (_sendMessageCommand = new RelayCommand(() =>
                {
                    MailSender.SendMessageToSupport("support@ess.am", "Test",
                        string.Format("Test message from administator {0}.",
                            ApplicationManager.GetEsUser.FullName));
                }));
            }
        }
        #endregion

        #region Users

        public void OnLogoff(object o)
        {
            if (MessageManager.ShowMessage("Դուք իսկապե՞ս ցանկանում եք դուրս գալ համակարգից:", "Աշխատանքի ավարտ", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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

        private List<InvoiceModel> GetInvoices(InvoiceState state, InvoiceTypeEnum type, int count)
        {
            switch (state)
            {
                case InvoiceState.All:
                    return InvoicesManager.GetInvoices(type, null, count);
                case InvoiceState.New:
                    return null;

                case InvoiceState.Accepted:
                    return InvoicesManager.GetInvoices(type, false, count);
                    return InvoicesManager.GetInvoicesDescriptions(type, count);

                case InvoiceState.Draft:
                    return InvoicesManager.GetInvoices(type, true, count);

                    return InvoicesManager.GetUnacceptedInvoicesDescriptions(type);

                case InvoiceState.Approved:
                    return InvoicesManager.GetInvoices(type, false, count);
                    //return InvoicesManager.GetInvoicesDescriptions(type);
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
            try
            {
                var actionMode = o as EcrExecuiteActions?;
                var ecServer = EcrManager.EcrServer;
                IsLoading = true;
                MessageModel message = null;
                switch (actionMode)
                {
                    case EcrExecuiteActions.CheckConnection:
                        message = ecServer.TryConnection() ? new MessageModel("ՀԴՄ կապի ստուգումն իրականացել է հաջողությամբ: " + ecServer.ActionDescription, MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageTypeEnum.Warning);
                        IsLoading = false;
                        break;
                    case EcrExecuiteActions.OperatorLogin:
                        message = ecServer.TryOperatorLogin() ? new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
                        IsLoading = false;
                        break;
                    case EcrExecuiteActions.PrintReturnTicket:
                        var resoult = MessageManager.ShowMessage("Դուք ցանկանու՞մ եք վերադարձնել ՀԴՄ կտրոնն ամբողջությամբ:", "ՀԴՄ կտրոնի վերադարձ", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (resoult == MessageBoxResult.Yes)
                        {
                            message = ecServer.PrintReceiptReturnTicket(true) ? new MessageModel("ՀԴՄ կտրոնի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Error);
                        }
                        else if (resoult == MessageBoxResult.No)
                        {
                            message = ecServer.PrintReceiptReturnTicket(false) ? new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի մասնակի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Error);
                        }
                        else
                        {
                            message = new MessageModel("ՀԴՄ վերադարձի կտրոնի տպումն ընդհատվել է:", MessageTypeEnum.Warning);
                        }
                        IsLoading = false;
                        break;
                    case EcrExecuiteActions.PrintLatestTicket:
                        message = ecServer.PrintReceiptLatestCopy() ? new MessageModel("ՀԴՄ վերջին կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ վերջին կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
                        IsLoading = false;
                        break;
                    case EcrExecuiteActions.PrintReportX:
                        message = ecServer.GetReport(ReportType.X) ? new MessageModel("ՀԴՄ X հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ X հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
                        IsLoading = false;
                        break;
                    case EcrExecuiteActions.PrintReportZ:
                        message = ecServer.GetReport(ReportType.Z) ? new MessageModel("ՀԴՄ Z հաշվետվության տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ Z հաշվետվության տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
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
                        var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.ExcelFilePath);
                        if (string.IsNullOrEmpty(filePath))
                        {
                            IsLoading = false;
                            return;
                        }
                        try
                        {
                            if (ecServer.PrintReceiptFromExcelFile(filePath))
                            {
                                IsLoading = false;
                                MessageManager.OnMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success));
                            }
                            else
                            {
                                IsLoading = false;
                                MessageManager.OnMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումն ընդհատվել է: {1} ({0})", ecServer.ActionCode, ecServer.ActionDescription), MessageTypeEnum.Warning));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageManager.OnMessage(ex.ToString());
                        }
                        var memberSettings = ApplicationManager.Settings.SettingsContainer.MemberSettings;
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
                        if (ecServer.SetCashWithdrawal(HgConvert.ToDecimal(amountForm.TicketValue), partner.FullName))
                            MessageManager.OnMessage("Կանխավճարի վերադարձն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                        else
                            MessageManager.OnMessage("Կանխավճարի վերադարձը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
                        break;
                    case EcrExecuiteActions.PrintCash:
                        break;
                    case EcrExecuiteActions.CashIn:
                        var cashInPartner = SelectItemsManager.SelectPartner();
                        if (cashInPartner == null) break;
                        var cashInAmountForm = new InputForm("Կանխավճար");
                        if (cashInAmountForm.ShowDialog() != DialogResult.OK) break;
                        if (ecServer.SetCashWithdrawal(HgConvert.ToDecimal(cashInAmountForm.TicketValue), cashInPartner.FullName))
                            MessageManager.OnMessage("Կանխավճարի կտրոնի տպումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                        else
                            MessageManager.OnMessage("Կանխավճարի կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecServer.ActionDescription, ecServer.ActionCode), MessageTypeEnum.Warning);
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
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.ToString());
            }
        }

        private void OnGetCashDeskInfo(object o)
        {
            var cashDesks = CashDeskManager.GetCashDesks();
            var partnersDebit = PartnersManager.GetPartnersAmount(true);
            var partnersCredit = PartnersManager.GetPartnersAmount(false);

            if (cashDesks == null || cashDesks.Count == 0)
            {
                return;
            }
            string content, title;
            title = "Դրամարկղի մնացորդի դիտում";
            var cashDeskContent = string.Empty;
            if (!ApplicationManager.IsInRole(UserRoleEnum.Manager))
            {
                cashDesks = CashDeskManager.GetCashDesks(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks);
                cashDesks.AddRange(CashDeskManager.GetCashDesks(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts));
                cashDeskContent = cashDesks.Where(s => s.IsCash).Aggregate(cashDeskContent, (current, item) => current + string.Format("Դրամարկղ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N")));
                cashDeskContent = cashDesks.Where(s => !s.IsCash).Aggregate(cashDeskContent, (current, item) => current + string.Format("Անկանխիկ {0}  - {1} դր․ \n", item.Name, item.Total.ToString("N")));
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
                cashDeskContent += string.Format("Դեբիտորական - {0} դր․ \n", partnersDebit.ToString("N"));
                cashDeskContent += string.Format("Կրեդիտորական - {0} դր․ \n\n", partnersCredit.ToString("N"));
                cashDeskContent += string.Format("Ընդամենը դրամական միջոցներ - {0} դր․ \n\n", (cashDesks.Sum(s => s.Total) + partnersDebit + partnersCredit).ToString("N"));
            }
            MessageManager.ShowMessage(cashDeskContent, title, MessageBoxImage.Information);
        }

        private void OnExecuteEcrAction(object o)
        {
            var td = new Thread(() => { ExecuteEcrAction(o); });
            td.SetApartmentState(ApartmentState.STA);
            td.Start();
        }

        private void OnAboutEcr(object o)
        {
            MessageBox.Show("ՀԴՄ ծրագրային ապահովում \nԹողարկում: 1.06 \nՊրոտոկոլ: v0.6 \nՏեխնիկական հարցերով դիմել: support@ess.am", "ՀԴՄ տեխնիական օգնություն", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Data

        private bool CanGetProductOrder(ProductOrderTypeEnum productOrderType)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Admin) ||
                ApplicationManager.IsInRole(UserRoleEnum.Director) ||
                ApplicationManager.IsInRole(UserRoleEnum.Manager) ||
                ApplicationManager.IsInRole(UserRoleEnum.SaleManager) ||
                   ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
        }

        private void OnGetProductOrder(ProductOrderTypeEnum productOrderType)
        {
            TableViewModel<ProductOrderModel> vm;
            if (productOrderType == ProductOrderTypeEnum.Dynamic)
            {
                var vm1 = new ProductOrderViewModel(productOrderType);
                AddDocument(vm1);
            }
            else
            {
                vm = new ProductOrderBySaleViewModel(productOrderType);
                vm.Update();
                AddDocument(vm);
            }



        }

        private void OnGetSaleProducts(object o)
        {
            var vm = new SaleProductsViewModel(null);
            AddDocument(vm);
        }

        private void OnWriteOffProducts(object o)
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks()).FirstOrDefault();
            if (stock == null) return;
            var existingProducts = ProductsManager.GetProductItemsByStock(stock.Id);
            OnCreateWriteOffInvoice(existingProducts.Select(s => new InvoiceItemsModel { Code = s.Product.Code, Quantity = s.Quantity }).ToList(), stock.Id);
        }

        private void OnCreateWriteOffInvoice(List<InvoiceItemsModel> items, short? stockId, string notes = null)
        {
            if (items == null || !items.Any())
            {
                OnNewMessage(new MessageModel(DateTime.Now, "Դուրսգրման ենթակա ապրանք գոյություն չունի:", MessageTypeEnum.Information));
                return;
            }
            var isTrimInvoice = false;
            if (items.Count > 500)
            {
                var result = MessageManager.ShowMessage(string.Format("Առկա է {0} անվանում ապրանք: Ցանակու՞մ եք տրոհել 500 ական տողերի:", items.Count), "Հարցում", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        return;
                    case MessageBoxResult.Yes:
                        isTrimInvoice = true;
                        break;
                    default:
                        break;
                }
            }
            var vm = new InventoryWriteOffViewModel();
            vm.Invoice.FromStockId = stockId;
            vm.Invoice.Notes = notes;
            vm.FromStock = ApplicationManager.CashManager.GetStocks.SingleOrDefault(s => s.Id == stockId);
            foreach (var item in items)
            {
                var productId = item.ProductId;
                var code = item.Code;
                var quantity = item.Quantity;
                if (productId != Guid.Empty)
                {
                    vm.SetInvoiceItem(productId);
                }
                else
                {
                    vm.SetInvoiceItem(code);
                }
                if (vm.InvoiceItem.Product == null)
                {
                    continue;
                }
                vm.InvoiceItem.Quantity = quantity;
                vm.InvoiceItem.DisplayOrder = (short)(vm.InvoiceItems.Count + 1);
                vm.InvoiceItems.Add(vm.InvoiceItem);
                vm.InvoiceItem = new InvoiceItemsModel();

                if (vm.InvoiceItems.Count > 500 && isTrimInvoice)
                {
                    AddInvoiceDocument(vm);
                    vm = new InventoryWriteOffViewModel();
                    vm.Invoice.FromStockId = stockId;
                    vm.Invoice.Notes = notes;
                }
            }
            if (vm.InvoiceItems.Any()) AddInvoiceDocument(vm);
        }

        private void OnCreateWriteInInvoice(List<InvoiceItemsModel> items, short? stockId, string notes)
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
                var code = item.Code;
                var quantity = item.Quantity;
                vm.SetInvoiceItem(code);
                if (vm.InvoiceItem.Product == null)
                {
                    continue;
                }
                vm.InvoiceItem.Price = 0;
                vm.InvoiceItem.Quantity = quantity;
                vm.InvoiceItem.DisplayOrder = (short)(vm.InvoiceItems.Count + 1);
                vm.InvoiceItems.Add(vm.InvoiceItem);
                vm.InvoiceItem = new InvoiceItemsModel();
            }
            AddInvoiceDocument(vm);
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

        private void OnViewProducts(ProductsViewEnum viewBy)
        {
            DocumentViewModel vm = null;
            switch (viewBy)
            {
                case ProductsViewEnum.ByStocks:
                    var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks(), true);
                    vm = new ProductItemsViewModel(stocks);
                    break;
                case ProductsViewEnum.ByDetile:
                    vm = new ProductItemsViewByDetileViewModel();
                    break;
                case ProductsViewEnum.ByProducts:
                    break;
                case ProductsViewEnum.ByProductItems:
                    break;
                case ProductsViewEnum.ByCategories:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("viewBy", viewBy, null);
            }
            AddDocument(vm);
        }

        private bool CanGetProductHistory(object o)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
        }

        private void OnGetProductsHistory(object o)
        {
            AddDocument(new ProductHistoryViewModel());
        }

        private void OnViewPackingList(object o)
        {
            var tuple = o as Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount, OrderInvoiceBy>;
            if (tuple == null) return;

            var type = tuple.Item1;
            var state = tuple.Item2;
            var count = (int)tuple.Item3;
            var orderBy = tuple.Item4;
            List<InvoiceModel> invoices;
            if (tuple.Item3 == MaxInvocieCount.All)
            {
                if (state == InvoiceState.Approved)
                {
                    var dates = UIHelper.Managers.SelectManager.GetDateIntermediate();
                    if (dates == null) return;
                    invoices = InvoicesManager.GetApprovedInvoices(type, dates);
                }
                else { invoices = InvoicesManager.GetDraftInvoices(type); }
            }
            else
            {
                invoices = GetInvoices(state, type, count);
            }
            if (invoices == null)
            {
                OnNewMessage(new MessageModel("Ապրանքագիր չի հայտնաբերվել։", MessageTypeEnum.Information));
                return;
            }
            invoices = invoices.OrderByDescending(s => s.ApproveDate).ToList();
            OpenPackingLists(SelectItemsManager.SelectInvoice(invoices, orderBy, true));

        }

        #endregion

        #region Help

        #endregion

        #region Manager

        private bool CanManageProducts(object o)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        private void OnManageProducts(object o)
        {
            var vm = new ProductManagerViewModel();
            ApplicationManager.Instance.CashProvider.ProductsChanged += vm.OnUpdatedProducts;
            ApplicationManager.Instance.CashProvider.ProductUpdated += vm.OnUpdate;
            vm.ProductEditedEvent += OnProductChanged;
            AddDocument(vm);
        }

        private void OnProductChanged(ProductModel product)
        {
            foreach (InvoiceViewModel vm in Documents.Where(d => d is InvoiceViewModel))
            {
                vm.OnProductChanged(product);
            }
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
            return ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        private void OnManagePartners(object o)
        {
            var partVm = new PartnerViewModel();
            //ApplicationManager.CashManager.
            AddDocument(partVm);
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

        private ICommand _manageStockesCommand;

        public ICommand ManageStockesCommand
        {
            get { return _manageStockesCommand ?? (_manageStockesCommand = new RelayCommand(OnManageStockes, CanManageStockes)); }
        }

        private bool CanManageStockes(object obj)
        {
            return true;
        }

        private void OnManageStockes(object obj)
        {
            AddDocument(new ManageStockesViewModel());
        }

        private ICommand _manageCashDesksCommand;

        public ICommand ManageCashDesksCommand
        {
            get { return _manageCashDesksCommand ?? (_manageCashDesksCommand = new RelayCommand(OnManageCashDesks, CanManageCashDesks)); }
        }

        private bool CanManageCashDesks(object obj)
        {
            return true;
        }

        private void OnManageCashDesks(object obj)
        {
            AddDocument(new ManageCashDesksViewModel());
        }

        #endregion Admin

        #region Documents

        #region Get invoices command

        private ICommand _getInvoicesCommand;

        public ICommand GetInvoicesCommand
        {
            get { return _getInvoicesCommand ?? (_getInvoicesCommand = new RelayCommand<Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount>>(OnGetInvoices, CanGetInvoices)); }
        }

        public bool CanGetInvoices(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> tuple)
        {
            if (tuple == null) return false;

            var type = tuple.Item1;
            var state = tuple.Item2;
            var count = (int)tuple.Item3;
            switch (type)
            {
                case InvoiceTypeEnum.PurchaseInvoice:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);

                case InvoiceTypeEnum.SaleInvoice:
                    switch (state)
                    {
                        case InvoiceState.Approved:
                        case InvoiceState.Accepted:
                        case InvoiceState.All:
                            return ApplicationManager.IsInRole(UserRoleEnum.Seller);

                        case InvoiceState.Draft:
                        case InvoiceState.New:
                            return ApplicationManager.IsInRole(UserRoleEnum.Seller) || ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller);

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case InvoiceTypeEnum.ProductOrder:
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
                case InvoiceTypeEnum.MoveInvoice:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
                case InvoiceTypeEnum.InventoryWriteOff:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) && !ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
                case InvoiceTypeEnum.ReturnFrom:
                    return ApplicationManager.IsInRole(UserRoleEnum.Seller);
                case InvoiceTypeEnum.ReturnTo:
                case InvoiceTypeEnum.Statements:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
                case InvoiceTypeEnum.None:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnGetInvoices(Tuple<InvoiceTypeEnum, InvoiceState, MaxInvocieCount> tuple)
        {
            //var tuple = o as Tuple<InvoiceType, InvoiceState, MaxInvocieCount>;
            if (tuple != null)
            {
                var type = tuple.Item1;
                var state = tuple.Item2;
                var count = (int)tuple.Item3;
                List<InvoiceModel> invoices = null;
                switch (state)
                {
                    case InvoiceState.New:
                        InvoiceViewModel newInvocieViewmodel = null;
                        switch (type)
                        {
                            case InvoiceTypeEnum.SaleInvoice:
                                newInvocieViewmodel = new SaleInvoiceViewModel();
                                break;
                            case InvoiceTypeEnum.MoveInvoice:
                                newInvocieViewmodel = new InternalWaybillViewModel();
                                break;
                            case InvoiceTypeEnum.PurchaseInvoice:
                                newInvocieViewmodel = new PurchaseInvoiceViewModel();
                                break;
                            case InvoiceTypeEnum.InventoryWriteOff:
                                newInvocieViewmodel = new InventoryWriteOffViewModel();
                                break;
                            case InvoiceTypeEnum.ProductOrder:
                                break;
                            case InvoiceTypeEnum.ReturnFrom:
                                newInvocieViewmodel = new ReturnFromInvoiceViewModel();
                                break;
                            case InvoiceTypeEnum.ReturnTo:
                                newInvocieViewmodel = new ReturnToInvoiceViewModel();
                                break;
                            default:
                                break;
                        }
                        AddInvoiceDocument(newInvocieViewmodel);
                        return;
                    case InvoiceState.Draft:
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
                invoices = SelectItemsManager.SelectInvoice(invoices, OrderInvoiceBy.CreatedDate, true);
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
                        case InvoiceType.ReturnFrom:
                            vm = new ReturnFromInvoiceViewModel(invoiceModel.Id);
                            break;
                        case InvoiceType.ReturnTo:
                            vm = new ReturnToInvoiceViewModel(invoiceModel.Id);
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

        #region View
        public ICommand GetChashDesksInfoCommand
        {
            get { return new RelayCommand(OnGetCashDeskInfo); }
        }
        private ICommand _viewDebitByPartnerCommand;

        public ICommand ViewDebitByPartnerCommand
        {
            get { return _viewDebitByPartnerCommand ?? (_viewDebitByPartnerCommand = new RelayCommand<DebitEnum>(OnViewDebitByPartner, CanViewDebitByPartner)); }
        }

        private ICommand _checkDebitByPartnerCommand;
        public ICommand CheckDebitByPartnerCommand
        {
            get { return _checkDebitByPartnerCommand ?? (_checkDebitByPartnerCommand = new RelayCommand<DebitEnum>(OnCheckCashDeskInfo, CanCheckCashDeskInfo)); }
        }
        private bool CanViewDebitByPartner(DebitEnum obj)
        {
            return true;
        }

        private void OnViewDebitByPartner(DebitEnum value)
        {
            UserControls.Managers.CashDeskManager.ViewDebitByPartner(value);
        }
        private bool CanCheckCashDeskInfo(DebitEnum obj)
        {
            return true;
        }

        private void OnCheckCashDeskInfo(DebitEnum value)
        {
            UserControls.Managers.CashDeskManager.CheckDebitByPartner(value);
        }
        #endregion View

        #region View report

        private ICommand _viewAccountantTableCommand;

        public ICommand ViewAccountantTableCommand
        {
            get { return _viewAccountantTableCommand ?? (_viewAccountantTableCommand = new RelayCommand<AccountingActionsEnum?>(OnViewAccountantTable)); }
        }

        private void OnViewAccountantTable(AccountingActionsEnum? accountingPlanEnum)
        {
            var dates = UIHelper.Managers.SelectManager.GetDateIntermediate();
            if (dates == null) return;
            var vm = new ViewAccountantTableViewModel(dates.Item1, dates.Item2);
            AddInvoiceDocument(vm);
            vm.UpdateAccountingRecords(accountingPlanEnum ?? AccountingActionsEnum.None);
        }

        public ICommand ViewPartnersBalanceDetailedCommand { get { return new RelayCommand(OnViewPartnersBalanceDetailed); } }

        private void OnViewPartnersBalanceDetailed(object obj)
        {
            var vm = new ViewPartnersBalanceViewModel();
            AddInvoiceDocument(vm);
            vm.Update();
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
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorCashier) || ApplicationManager.IsInRole(UserRoleEnum.Cashier);

                case AccountingPlanEnum.Prepayments:
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorCashier) || ApplicationManager.IsInRole(UserRoleEnum.Cashier);

                case AccountingPlanEnum.CashDesk:
                    break;

                case AccountingPlanEnum.Accounts:
                    break;

                case AccountingPlanEnum.EquityBase:
                    break;

                case AccountingPlanEnum.PurchasePayables:
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorCashier) || ApplicationManager.IsInRole(UserRoleEnum.Cashier); ;

                case AccountingPlanEnum.ReceivedInAdvance:
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorCashier) || ApplicationManager.IsInRole(UserRoleEnum.Cashier); ;

                case AccountingPlanEnum.DebitForSalary:
                    break;
                case AccountingPlanEnum.Proceeds:
                    break;
                case AccountingPlanEnum.CostPrice:
                    break;
                case AccountingPlanEnum.SalesCosts:
                    break;
                case AccountingPlanEnum.OtherOperationalExpenses:
                    break;

                case AccountingPlanEnum.None:
                    break;
                case AccountingPlanEnum.BalanceDebetCredit:
                    return ApplicationManager.IsInRole(UserRoleEnum.SeniorCashier);
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
            var dates = UIHelper.Managers.SelectManager.GetDateIntermediate();
            if (dates == null) return;
            var repayment = AccountingRecordsManager.GetAccountingRecords(dates.Item1, dates.Item2, (short)AccountingPlanEnum.CashDesk, (short)AccountingPlanEnum.AccountingReceivable);
            var ui = new UIListView(repayment.Where(s => (s.CreditGuidId != null && partners.Contains(s.CreditGuidId.Value))).Select(s => new { Ամսաթիվ = s.RegisterDate, Վճարված = s.Amount, Նշումներ = s.Description }).ToList()) { Title = "Դեբիտորական պարտքի մարում ըստ պատվիրատուների" };
            ui.Show();
        }
        #region Ecr commands
        public ICommand AboutEcrCommand
        {
            get { return new RelayCommand(OnAboutEcr); }
        }
        #endregion Ecr commands

        #endregion CashDesk

        #region Data

        //View
        private ICommand _changedProductsCommand;

        public ICommand ChangedProductsCommand
        {
            get { return _changedProductsCommand ?? (_changedProductsCommand = new RelayCommand(OnViewChangedProducts)); }
        }

        private ICommand _viewProductsCommand;
        public ICommand ViewProductDetilesCommand
        {
            get { return _viewProductsCommand ?? (_viewProductsCommand = new RelayCommand(OnViewProducts)); }
        }

        private ICommand _viewProductsModificationLogCommand;
        public ICommand ViewProductsModificationLogCommand
        {
            get { return _viewProductsModificationLogCommand ?? (_viewProductsModificationLogCommand = new RelayCommand(OnViewProductsModificationLog)); }
        }
        public ICommand ViewProductsPriceModificationLogCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var vm = new ViewModifiedProductsViewModel(true);
                    AddDocument(vm);
                });
            }
        }
        private void OnViewProducts()
        {
            var vm = new ViewProductDetilesViewModel();
            AddDocument(vm);
        }
        private void OnViewChangedProducts()
        {
            var vm = new ViewProductsViewModel(true);
            AddDocument(vm);
        }
        private void OnViewProductsModificationLog()
        {
            var vm = new ViewModifiedProductsViewModel();
            AddDocument(vm);
        }
        public ICommand GetReportCommand { get; private set; }

        public ICommand GetProductsHistoryCommand
        {
            get { return new RelayCommand(OnGetProductsHistory, CanGetProductHistory); }
        }

        private ICommand _getFallowProductsCommand;

        public ICommand GetFallowProductsCommand
        {
            get { return _getFallowProductsCommand ?? (_getFallowProductsCommand = new RelayCommand(OnGetFalowProducts)); }
        }

        private void OnGetFalowProducts(object obj)
        {
            var vm = new FallowProductsViewModel();
            AddDocument(vm);
        }

        private ICommand _checkProductsRemainderCommand;

        public ICommand CheckProductsRemainderCommand
        {
            get { return _checkProductsRemainderCommand ?? (_checkProductsRemainderCommand = new RelayCommand(OnCheckProductsRemainder)); }
        }

        private void OnCheckProductsRemainder(object obj)
        {
            var vm = new CheckProductsRemainderViewModel();
            AddDocument(vm);
        }

        #region View

        public ICommand ViewInternalWayBillCommands { get; private set; }

        #region View Packing List For Saller Command

        private ICommand _viewPackingListCommand;

        public ICommand ViewPackingListCommand
        {
            get { return _viewPackingListCommand ?? (_viewPackingListCommand = new RelayCommand(OnViewPackingList)); }
        }

        #endregion View Packing List For Saller Command

        private ICommand _viewPackingListsCommand;

        public ICommand ViewPackingListsCommand
        {
            get { return _viewPackingListsCommand ?? (_viewPackingListsCommand = new RelayCommand<InvoiceTypeEnum>(OnViewParkingLists)); }
        }

        #endregion View

        #region Stock Take

        #endregion Stock Take

        #endregion

        #region Help

        private ICommand _aboutCommand;

        public ICommand AboutCommand
        {
            get { return _aboutCommand ?? (_aboutCommand = new RelayCommand(OnAbout)); }
        }

        private void OnAbout(object obj)
        {
            AssemblyTitleAttribute _Title = null;
            AssemblyCompanyAttribute _Company = null;
            AssemblyCopyrightAttribute _Copyright = null;
            AssemblyProductAttribute _Product = null;
            Version _Version = null;
            var Title = String.Empty;
            var CompanyName = String.Empty;
            var Copyright = String.Empty;
            var ProductName = String.Empty;
            var Version = String.Empty;
            var linkTimeLocal = GetLinkerTime(Assembly.GetExecutingAssembly()).ToString("d");
            try
            {
                var assembly = Assembly.GetEntryAssembly();

                if (assembly != null)
                {
                    object[] attributes = assembly.GetCustomAttributes(false);

                    foreach (object attribute in attributes)
                    {
                        Type type = attribute.GetType();

                        if (type == typeof(AssemblyTitleAttribute)) _Title = (AssemblyTitleAttribute)attribute;
                        if (type == typeof(AssemblyCompanyAttribute))
                            _Company = (AssemblyCompanyAttribute)attribute;
                        if (type == typeof(AssemblyCopyrightAttribute))
                            _Copyright = (AssemblyCopyrightAttribute)attribute;
                        if (type == typeof(AssemblyProductAttribute))
                            _Product = (AssemblyProductAttribute)attribute;
                    }



                    _Version = assembly.GetName().Version;
                }

                if (_Title != null) Title = _Title.Title;
                if (_Company != null) CompanyName = _Company.Company;
                if (_Copyright != null) Copyright = _Copyright.Copyright;
                if (_Product != null) ProductName = _Product.Product;
                if (_Version != null) Version = _Version.ToString();
            }
            catch
            {
            }

            var info = string.Format("Application: {0}\n" +
                                     "Product: {1}\n" +
                                     "Company: {2}\n" +
                                     "Version: {3}\n" +
                                     "Date: {4}\n\n" +
                                     "Copyright: {5}", Title, ProductName, CompanyName, Version, linkTimeLocal, Copyright);
            MessageManager.ShowMessage(info, "About Es Market");
        }
        public static DateTime GetLinkerTime(Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }
        #endregion Help

        #region Toolbar buttons

        private ICommand _openCashdeskCommand;

        public ICommand OpenCashdeskCommand
        {
            get { return _openCashdeskCommand ?? (_openCashdeskCommand = new RelayCommand(OnOpenCashDesk, CanOpenCashDesk)); }
        }

        private bool CanOpenCashDesk(object obj)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Cashier) &&
                   (!string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort) || !string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveCashDeskPrinter));
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
            return ApplicationManager.IsInRole(UserRoleEnum.SeniorSeller);
        }

        private void OnCorrectProducts(object obj)
        {
            var vm = new CorrectProductsViewModel();
            AddDocument(vm);
        }

        private ICommand _rReEditProductsCommand;
        public ICommand ReEditProductsCommand
        {
            get { return _rReEditProductsCommand ?? (_rReEditProductsCommand = new RelayCommand(OnReEditProducts, CanReeditProducts)); }
        }
        private bool CanReeditProducts(object obj)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.SeniorSeller);
        }

        private void OnReEditProducts(object obj)
        {
            var vm = new ReeditProductsViewModel();
            AddDocument(vm);
        }

        public ICommand ManageProductsCommand
        {
            get { return new RelayCommand(OnManageProducts, CanManageProducts); }
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
            get { return new RelayCommand<ProductOrderTypeEnum>(OnGetProductOrder, CanGetProductOrder); }
        }

        public ICommand SaleProductsCommand
        {
            get { return new RelayCommand(OnGetSaleProducts); }
        }

        public ICommand ViewProductsCommand
        {
            get { return new RelayCommand<ProductsViewEnum>(OnViewProducts); }
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

        public bool CanOpenTools(ToolsEnum toolEnum)
        {
            switch (toolEnum)
            {
                case ToolsEnum.Log:
                    return true;

                case ToolsEnum.ProductItems:
                    return ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller) || ApplicationManager.IsInRole(UserRoleEnum.Seller) || ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);

                case ToolsEnum.Categories:
                    return ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);

                default:
                    throw new ArgumentOutOfRangeException("toolEnum", toolEnum, null);
            }
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
                case ProjectCreationEnum.EditLast:
                case ProjectCreationEnum.Edit:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper) || ApplicationManager.IsInRole(UserRoleEnum.SeniorSeller) || ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
                    break;
                case ProjectCreationEnum.View:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper) || ApplicationManager.IsInRole(UserRoleEnum.Manager);
                    break;
                case ProjectCreationEnum.ViewLast:
                    return ApplicationManager.IsInRole(UserRoleEnum.Director) || ApplicationManager.IsInRole(UserRoleEnum.SaleManager) || ApplicationManager.IsInRole(UserRoleEnum.StockKeeper) || ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorManager);
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
                    var stocks = SelectItemsManager.SelectStocks(StockManager.GetStocks());
                    if (stocks.Count == 0)
                    {
                        MessageManager.OnMessage(new MessageModel("Պահեստի բացակայում", MessageTypeEnum.Warning));
                        return;
                    }
                    stockTake = StockTakeManager.CreateStockTaking(stocks.First().Id);
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
            vm.CreateWriteInInvoiceEvent += OnCreateWriteInInvoice;
            vm.CreateWriteOffInvoiceEvent += OnCreateWriteOffInvoice;
            AddInvoiceDocument(vm);
        }


        private StockTakeModel GetLastStockTake()
        {
            var stockTaking = StockTakeManager.GetLastStockTake();
            if (stockTaking == null)
            {
                MessageManager.ShowMessage("Որևէ հաշվառում չի հայտնաբերվել։", "Հաշվառում", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        private Tuple<DateTime, DateTime> _dataIntermidiate;


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
