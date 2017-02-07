using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using CashReg;
using CashReg.Helper;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using UserControls.Commands;

namespace UserControls.ViewModels
{
    public class SettingsViewModel : DocumentViewModel
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Properties

        private const string SaleBySingleProperty = "SaleBySingle";
        private const string BuyBySingleProperty = "BuyBySingel";
        /// <summary>
        /// Other
        /// </summary>
        private const string OfflineModeProperty = "OfflineMode";
        #endregion

        #region Private properties
        private long _memberId;
        private List<ItemsForChoose> _stocks;
        private List<ItemsForChoose> _cashDesks;
        private bool _saleBySingle = false;
        private bool _buyBySingle = false;
        private bool _offlineMode = false;
        private bool _isInProgress;
        #endregion

        #region Public Properties
        public string Title { get; set; }
        public bool IsLoading { get { return _isInProgress; } set { if (_isInProgress == value) return; _isInProgress = value; OnPropertyChanged("IsLoading"); } }
        public string Description { get; set; }
        public bool IsModified { get; set; }
        public bool IsInProgress { get { return _isInProgress; } set { if (_isInProgress == value) return; _isInProgress = value; OnPropertyChanged("IsInProgress"); } }
        public List<ItemsForChoose> Stocks { get { return _stocks; } }
        public List<ItemsForChoose> CashDesks { get { return _cashDesks; } }
        public bool SaleBySingle { get { return _saleBySingle; } set { _saleBySingle = value; OnPropertyChanged(SaleBySingleProperty); } }
        public bool BuyBySingle { get { return _buyBySingle; } set { _buyBySingle = value; OnPropertyChanged(BuyBySingleProperty); } }
        public bool LocalMode { get { return _offlineMode; } set { _offlineMode = value; OnPropertyChanged(OfflineModeProperty); } }
        /// <summary>
        /// Printers
        /// </summary>
        /// <param name="memberId"></param>
        public string SalePrinter { get; set; }
        public ObservableCollection<ItemsForChoose> SalePrinters { get; set; }
        public string BarcodePrinter { get; set; }
        public ObservableCollection<ItemsForChoose> BarcodePrinters { get; set; }
        /// <summary>
        /// ECR
        /// </summary>
        #region ECR settings
        public EcrConfig EcrConfig { get; set; }
        public bool CanActivateEcr
        {
            get
            {
                var canActivateEcr = EcrConfig != null && !string.IsNullOrEmpty(EcrConfig.EcrSettings.Ip) && EcrConfig.EcrSettings.Port > 0 &&
                       !string.IsNullOrEmpty(EcrConfig.EcrSettings.Password) &&
                       EcrConfig.EcrSettings.EcrCashier != null && EcrConfig.EcrSettings.EcrCashier.Cashier > 0 && !string.IsNullOrEmpty(EcrConfig.EcrSettings.EcrCashier.Pin) &&
                       EcrConfig.EcrSettings.CashierDepartment != null;
                if (!canActivateEcr && EcrConfig != null)
                {
                    EcrConfig.IsActive = false;
                    OnPropertyChanged("IsEcrActivated");
                }
                return canActivateEcr;
            }
        }
        #endregion
        #endregion
        public SettingsViewModel(long memberId)
        {
            Title = "Կարգաբերումներ";
            _memberId = memberId;
            LoadProperties();
            SetCommands();
        }
        #region Private methods
        #region Settings
        private bool CanSetDefaultSettings(object o)
        {
            return true;
        }
        private void OnSetDefaultSettings(object o)
        {
            MessageModel message;
            var xml = new XmlManager();
            if (!xml.SetElementInnerText(SaleBySingle.ToString(), XmlTagItems.SaleBySingle))
            {
                message = new MessageModel("Վաճառքի կարգաբերումներ խմբագրումը ձախողվել է:", MessageModel.MessageTypeEnum.Warning);
            }
            else
            {
                ApplicationManager.SaleBySingle = SaleBySingle;
                message = new MessageModel("Վաճառքի կարգաբերումներ խմբագրումն իրականացվել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success);
            }
            ApplicationManager.MessageManager.OnNewMessage(message);
            if (!xml.SetElementInnerText(BuyBySingle.ToString(), XmlTagItems.BuyBySingle))
            {
                message = new MessageModel("Գնման կարգաբերումներ խմբագրումը ձախողվել է:", MessageModel.MessageTypeEnum.Warning);
            }
            else
            {
                ApplicationManager.BuyBySingle = BuyBySingle;
                message = new MessageModel("Գնման կարգաբերումներ խմբագրումն իրականացվել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success);
            }
            ApplicationManager.MessageManager.OnNewMessage(message);

        }
        #endregion
        private void LoadProperties()
        {
            var xml = new XmlManager();
            List<XmlSettingsItem> selected;
            selected = xml.GetItemsByControl(XmlTagItems.SaleStocks);
            _stocks = StockManager.GetStocks(_memberId).Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (selected.SingleOrDefault(t => t.Value.ToString() == s.Id.ToString()) != null) }).ToList();
            selected = xml.GetItemsByControl(XmlTagItems.SaleCashDesks);
            _cashDesks = CashDeskManager.TryGetCashDesk(_memberId, true).
            Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (selected.SingleOrDefault(t => t.Value.ToString() == s.Id.ToString()) != null) }).ToList();
            SaleBySingle = HgConvert.ToBoolean(xml.GetElementInnerText(XmlTagItems.SaleBySingle));
            BuyBySingle = HgConvert.ToBoolean(xml.GetElementInnerText(XmlTagItems.BuyBySingle));
            LocalMode = HgConvert.ToBoolean(xml.GetElementInnerText(XmlTagItems.LocalMode));
            //Ecr Settings
            var ecrConfig = ConfigSettings.GetEcrConfig();
            EcrConfig = ecrConfig ?? new EcrConfig();
            if (EcrConfig.EcrSettings != null && EcrConfig.EcrSettings.CashierDepartment != null)
            {
                EcrConfig.EcrSettings.CashierDepartment = EcrConfig.EcrSettings.TypeOfOperatorDeps.FirstOrDefault(s => s.Id == EcrConfig.EcrSettings.CashierDepartment.Id);
            }
            EcrConfig.IsActive = ecrConfig != null && ecrConfig.IsActive;
            //Printers
            SalePrinter = ApplicationManager.SalePrinter;
            BarcodePrinter = ApplicationManager.BarcodePrinter;

            var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
            SalePrinters = new ObservableCollection<ItemsForChoose>();
            BarcodePrinters = new ObservableCollection<ItemsForChoose>();
            foreach (var printer in printerQuery.Get())
            {
                var name = printer.GetPropertyValue("Name");
                var status = printer.GetPropertyValue("Status");
                var isDefault = printer.GetPropertyValue("Default");
                var isNetworkPrinter = printer.GetPropertyValue("Network");
                SalePrinters.Add(new ItemsForChoose()
                {
                    Data = string.Format("{0} Status:{1}{2}", name, status, isDefault is bool && (bool)isDefault ? " (Default)" : ""),
                    Value = name,
                    IsChecked = string.Equals(name, SalePrinter)
                });
                BarcodePrinters.Add(new ItemsForChoose()
                {
                    Data = string.Format("{0} Status:{1}{2}", name, status, isDefault is bool && (bool)isDefault ? " (Default)" : ""),
                    Value = name,
                    IsChecked = string.Equals(name, BarcodePrinter)
                });
            }

        }
        private void SetCommands()
        {
            SetSaleFromStockCommand = new SetSaleFromStock(this);
            SetCashDeskCommand = new SetCashDesk(this);
            SetApplicationSettingsCommand = new SetApplicationSettings(this);
        }

        private void ExecuteEcrAction(EcrExecuiteActions actionMode)
        {
            IsInProgress = true;
            var ecrserver = new EcrServer(EcrConfig.EcrSettings);
            MessageModel message = null;
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                    message = ecrserver.TryConnection() ? new MessageModel("Կապի ստուգումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageModel.MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.OperatorLogin:
                    message = ecrserver.TryOperatorLogin() ? new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success)
                        : new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    var operatorDeps = ecrserver.GetUsersDepsList();
                    if (operatorDeps == null)
                    {
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.ActionDescription, ecrserver.ActionCode), MessageModel.MessageTypeEnum.Warning);
                    }
                    else
                    {
                        EcrConfig.EcrSettings.TypeOfOperatorDeps = operatorDeps.d;
                        EcrConfig.EcrSettings.CashierDepartment = EcrConfig.EcrSettings.TypeOfOperatorDeps.FirstOrDefault();
                        OnPropertyChanged("EcrSettings");
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success);
                    }
                    break;
                case EcrExecuiteActions.CheckEcrConnection:
                    message = ecrserver.TryEcrConnection() ? new MessageModel("ՀԴՄ կապի ստուգումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageModel.MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.Zero:
                    break;
                case EcrExecuiteActions.LogoutOperator:
                    break;
                case EcrExecuiteActions.PrintReceiptTicket:
                    break;
                case EcrExecuiteActions.PrintLatestTicket:
                    break;
                case EcrExecuiteActions.PrintReturnTicket:
                    break;
                case EcrExecuiteActions.PrintEcrReport:
                    break;
                case EcrExecuiteActions.PrintReportX:
                    break;
                case EcrExecuiteActions.PrintReportZ:
                    break;
                case EcrExecuiteActions.ManageHeaderAndFooter:
                    break;
                case EcrExecuiteActions.ManageLogo:
                    break;
                default:
                    message = null;
                    break;
            }
            if (message != null)
            {
                ApplicationManager.MessageManager.OnNewMessage(message);
            }
            IsInProgress = false;
        }

        private void OnSaveEcrSettings(object o)
        {
            if (ConfigSettings.SetEcrConfig(XmlTagItems.Ecr, EcrConfig))
            {
                ApplicationManager.EcrSettings = EcrConfig.EcrSettings;
                ApplicationManager.IsEcrActivated = EcrConfig.IsActive;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("ՀԴՄ -ի գրանցումն իրականացել է հաջողությամբ։", MessageModel.MessageTypeEnum.Success));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("ՀԴՄ-ի գրանցումը ձախողվել է։", MessageModel.MessageTypeEnum.Warning));
            }
        }
        private bool CanExecuteEcrAction(EcrExecuiteActions actionMode)
        {
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                    return EcrConfig.EcrSettings != null && !string.IsNullOrEmpty(EcrConfig.EcrSettings.Ip) && EcrConfig.EcrSettings.Port != null;
                case EcrExecuiteActions.OperatorLogin:
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    return EcrConfig.EcrSettings != null && !string.IsNullOrEmpty(EcrConfig.EcrSettings.Ip) && EcrConfig.EcrSettings.Port != null && EcrConfig.EcrSettings.EcrCashier != null && !string.IsNullOrEmpty(EcrConfig.EcrSettings.EcrCashier.Pin);
                case EcrExecuiteActions.CheckEcrConnection:
                    return EcrConfig.IsActive && EcrConfig.EcrServiceSettings.IsActive && !string.IsNullOrEmpty(EcrConfig.EcrServiceSettings.Ip) && EcrConfig.EcrServiceSettings.Port > 0;
                case EcrExecuiteActions.Zero:
                    break;
                case EcrExecuiteActions.LogoutOperator:
                    break;
                case EcrExecuiteActions.PrintReceiptTicket:
                    break;
                case EcrExecuiteActions.PrintLatestTicket:
                    break;
                case EcrExecuiteActions.PrintReturnTicket:
                    break;
                case EcrExecuiteActions.PrintEcrReport:
                    break;
                case EcrExecuiteActions.PrintReportX:
                    break;
                case EcrExecuiteActions.PrintReportZ:
                    break;
                case EcrExecuiteActions.ManageHeaderAndFooter:
                    break;
                case EcrExecuiteActions.ManageLogo:
                    break;
                case EcrExecuiteActions.GetReceiptData:
                    break;
                case EcrExecuiteActions.CashWithdrawal:
                    break;
                default:
                    return false;
            }
            return false;
        }
        private void OnExecuteEcrAction(EcrExecuiteActions o)
        {
            var td = new Thread(() => ExecuteEcrAction(o));
            td.Start();
        }
        #endregion
        #region Public methods
        public void OnClose(object o)
        {
            ApplicationManager.OnTabItemClose(o);
        }
        public bool CanSetSaleFromStock()
        {
            return (Stocks != null && Stocks.Count > 0);
        }
        public void SetSaleFromStock()
        {
            var xml = new XmlManager();
            if (xml.SetStockItemsByControl(Stocks.Where(s => s.IsChecked)
                        .Select(s => new XmlSettingsItem { Key = XmlTagItems.Store, Data = s.Data, Value = s.Value, Member = _memberId })
                        .ToList(), XmlTagItems.SaleStocks))
            {
                MessageBox.Show("Գրանցումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Գրանցման ժամանակ տեղի է ունեցել սխալ։", "Գրանցման սխալ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public bool CanSetChashDesk()
        {
            return (CashDesks != null && CashDesks.Count > 0);
        }
        public void SetCashDesk()
        {
            var xml = new XmlManager();
            if (xml.SetStockItemsByControl(
                    CashDesks.Where(s => s.IsChecked)
                        .Select(s => new XmlSettingsItem { Key = XmlTagItems.CashDesk, Data = s.Data, Value = s.Value, Member = _memberId })
                        .ToList(), XmlTagItems.SaleCashDesks))
            {
                MessageBox.Show("Գրանցումն իրականացվել է հաջողությամբ։");
            }
            else
            {
                MessageBox.Show("Գրանցման ժամանակ տեղի է ունեցել սխալ։", "Գրանցման սխալ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public bool CanSetApplicationSettings()
        {
            return true;
        }

        public void SetApplicationSettiongs()
        {
            var xml = new XmlManager();
            if (!xml.SetElementInnerText(LocalMode.ToString(), XmlTagItems.LocalMode))
            {
                MessageBox.Show("Գրանցման ժամանակ տեղի է ունեցել սխալ։", "Գրանցման սխալ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ApplicationManager.LocalMode = LocalMode;
        }
        public void OnSetPrinters()
        {
            var xml = new XmlManager();
            var salePrinter = SalePrinters.FirstOrDefault(s => s.IsChecked);
            var barcodePrinter = BarcodePrinters.FirstOrDefault(s => s.IsChecked);

            if (xml.SetElementInnerText(salePrinter != null ? salePrinter.Value.ToString() : "", XmlTagItems.SalePrinter))
            {
                ApplicationManager.SalePrinter = salePrinter != null ? (string)salePrinter.Value : string.Empty;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Վաճառքի տպիչի գրանցում:", MessageModel.MessageTypeEnum.Success));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Վաճառքի տպիչի գրանցման ձախողում:", MessageModel.MessageTypeEnum.Warning));
            }

            if (xml.SetElementInnerText(barcodePrinter != null ? barcodePrinter.Value.ToString() : "", XmlTagItems.BarcodePrinter))
            {
                ApplicationManager.BarcodePrinter = barcodePrinter != null ? (string)barcodePrinter.Value : string.Empty;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Գնապիտակի տպիչի գրանցում:", MessageModel.MessageTypeEnum.Success));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Գնապիտակի տպիչի գրանցման ձախողում:", MessageModel.MessageTypeEnum.Warning));
            }
        }
        #endregion

        #region ICommands
        public ICommand CloseCommand { get { return new RelayCommand(OnClose); } }
        public ICommand SetSaleFromStockCommand { get; private set; }
        public ICommand SetCashDeskCommand { get; private set; }
        public ICommand SetDefaultSettingsCommand { get { return new RelayCommand(OnSetDefaultSettings, CanSetDefaultSettings); } }
        public ICommand SetApplicationSettingsCommand { get; private set; }
        public ICommand SetPrintersCommand { get { return new SetPrintersCommand(this); } }
        public ICommand SetEcrSettingsCommand { get { return new RelayCommand(OnSaveEcrSettings); } }
        public ICommand ExecuteEcrActionCommand { get { return new RelayCommand<EcrExecuiteActions>(OnExecuteEcrAction, CanExecuteEcrAction); } }

        #endregion
    }
}
