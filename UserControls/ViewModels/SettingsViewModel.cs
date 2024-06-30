using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CashReg.Helper;
using CashReg.Interfaces;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;

namespace UserControls.ViewModels
{
    public class SettingsViewModel : DocumentViewModel
    {
        #region Internal properties

        #region Settings
        private SettingsContainer _settings;
        private SettingsContainer Settings
        {
            get
            {
                return _settings ?? (_settings = new SettingsContainer());
            }
        }

        #endregion Settings

        #endregion Internal properties

        #region External properties

        #region IsInProgress
        private bool _isInProgress;
        public bool IsInProgress
        {
            get { return _isInProgress; }
            set { if (_isInProgress == value) return; _isInProgress = value; RaisePropertyChanged("IsInProgress"); }
        }
        #endregion IsIngrogress

        public bool HasBankAccounts { get { return ApplicationManager.CashManager.GetBankAccounts.Any(); } }

        #region Member settings

        #region General

        #region Barcode printer
        private List<ItemsForChoose> _lablePrinters;
        public List<ItemsForChoose> LablePrinters
        {
            get { return _lablePrinters ?? new List<ItemsForChoose>(); }
            set
            {
                _lablePrinters = value;
            }
        }
        #endregion Barcode printers

        #region Is work in offline mode
        public bool LocalMode { get { return _settings.MemberSettings.IsOfflineMode; } set { _settings.MemberSettings.IsOfflineMode = value; RaisePropertyChanged("OfflineMode"); } }
        #endregion Is work in offline mode

        #region Serial ports

        private List<ItemsForChoose> _cashDeskPrinters;
        public List<ItemsForChoose> CashDeskPrinters
        {
            get
            {
                return _cashDeskPrinters ?? (_cashDeskPrinters = new List<ItemsForChoose>());
            }
            set { _cashDeskPrinters = value; }
        }

        public List<string> ActiveSerialPorts
        {
            get
            {
                var serialPorts = new List<string>() { "" };
                serialPorts.AddRange(SerialPort.GetPortNames().ToList());
                return serialPorts;
            }
        }
        public string ScannerPort
        {
            get { return _settings.MemberSettings.ScannerPort; }
            set
            {
                _settings.MemberSettings.ScannerPort = value;
                RaisePropertyChanged(() => ScannerPort);
            }
        }
        public string CashDeskPort
        {
            get { return _settings.MemberSettings.CashDeskPort; }
            set
            {
                _settings.MemberSettings.CashDeskPort = value;
                RaisePropertyChanged(() => CashDeskPort);
            }
        }
        #endregion Serial ports

        public EcrServiceSettings EcrServiceSettings { get { return _settings.MemberSettings.EcrServiceSettings; } }

        public bool EnableAI
        {
            get
            {
                return false;
            }
            set
            {
                if (value == true)
                    MessageBox.Show("Ողջու՛յն, Ես Էմին եմ: \nԻնձ ակտիվացնելու համար հարկավոր է բաժանորդագրվել:", "Հաղորդագրություն Էմիից", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        public bool NotifyAboutIncomingInvoices { get { return _settings.MemberSettings.NotifyAboutIncomingInvoices; } set { _settings.MemberSettings.NotifyAboutIncomingInvoices = value; RaisePropertyChanged("NotifyAboutIncomingInvoices"); } }
        public bool UseUnicCode { get { return _settings.MemberSettings.UseUnicCode; } set { _settings.MemberSettings.UseUnicCode = value; RaisePropertyChanged("UseShortCode"); } }

        #region Brunch settings
        public ObservableCollection<BranchModel> Branches { get; set; }

        public BranchModel SelectedBranch
        {
            get { return _selectedBranch ?? (_selectedBranch = new BranchModel(ApplicationManager.Member.Id)); }
            set { _selectedBranch = value; RaisePropertyChanged("SelectedBranch"); }
        }
        public BranchModel BranchSettings { get { return Settings.MemberSettings.BranchSettings; } }
        #endregion Brunch settings
        #endregion General

        #region Sale

        #region Sale stocks
        private List<ItemsForChoose> _saleStocks;
        public List<ItemsForChoose> SaleStocks
        {
            get
            {
                return _saleStocks ?? (_saleStocks = ApplicationManager.CashManager.GetStocks.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.ActiveSaleStocks.Contains(s.Id)) }).ToList());
            }
        }
        #endregion Sale stocks

        #region Sale cash desks
        private List<ItemsForChoose> _saleCashDesks;
        public List<ItemsForChoose> SaleCashDesks
        {
            get { return _saleCashDesks ?? (_saleCashDesks = ApplicationManager.CashManager.GetCashDesk.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.SaleCashDesks.Contains(s.Id)) }).ToList()); }
        }
        #endregion Sale cash desks

        #region Sale bank accounts
        private List<ItemsForChoose> _saleBankAccounts;
        public List<ItemsForChoose> SaleBankAccounts
        {
            get { return _saleBankAccounts ?? (_saleBankAccounts = ApplicationManager.CashManager.GetBankAccounts.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.SaleBankAccounts.Contains(s.Id)) }).ToList()); }
        }
        #endregion Sale bank accounts

        #region Sale printer
        private List<ItemsForChoose> _salePrinters;
        public List<ItemsForChoose> SalePrinters
        {
            get
            {
                return _salePrinters ?? (_salePrinters = new List<ItemsForChoose>());
            }
            set { _salePrinters = value; }
        }
        #endregion Sale printer

        public bool SaleBySingle { get { return Settings.MemberSettings.SaleBySingle; } set { Settings.MemberSettings.SaleBySingle = value; RaisePropertyChanged("SaleBySingle"); } }
        public bool IsPrintSaleTicket { get { return Settings.MemberSettings.IsPrintSaleTicket; } set { Settings.MemberSettings.IsPrintSaleTicket = value; RaisePropertyChanged("IsPrintSaleTicket"); } }
        public bool IsEcrActivated { get { return Settings.MemberSettings.IsEcrActivated; } set { Settings.MemberSettings.IsEcrActivated = value; RaisePropertyChanged("IsEcrActivated"); } }

        #endregion Sale

        #region Purchase

        #region Purchase strocks
        private List<ItemsForChoose> _purchaseStocks;
        public List<ItemsForChoose> PurchaseStocks
        {
            get
            {
                return _purchaseStocks ?? (_purchaseStocks = ApplicationManager.CashManager.GetStocks.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.ActivePurchaseStocks.Contains(s.Id)) }).ToList());
            }
        }
        #endregion Purchase stocks

        #region Purchase cash desks
        private List<ItemsForChoose> _purchaseCashDesks;
        public List<ItemsForChoose> PurchaseCashDesks
        {
            get { return _purchaseCashDesks ?? (_purchaseCashDesks = ApplicationManager.CashManager.GetCashDesk.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.PurchaseCashDesks.Contains(s.Id)) }).ToList()); }
        }
        #endregion Purchase cash desks

        #region Purchase bank accounts
        private List<ItemsForChoose> _purchaseBankAccounts;
        public List<ItemsForChoose> PurchaseBankAccounts
        {
            get { return _purchaseBankAccounts ?? (_purchaseBankAccounts = ApplicationManager.CashManager.GetBankAccounts.Select(s => new ItemsForChoose { Data = s.Name, Value = s.Id, IsChecked = (Settings.MemberSettings.PurchaseBankAccounts.Contains(s.Id)) }).ToList()); }
        }
        #endregion Purchase bank accounts

        public bool PurchaseBySingle { get { return Settings.MemberSettings.PurchaseBySingle; } set { Settings.MemberSettings.PurchaseBySingle = value; RaisePropertyChanged("PurchaseBySingle"); } }
        public bool IsPrintPurchaseInvoice
        {
            get { return Settings.MemberSettings.IsPrintPurchaseInvoice; }
            set
            {
                Settings.MemberSettings.IsPrintPurchaseInvoice = value; RaisePropertyChanged("IsPrintPurchaseInvoice");
            }
        }
        #endregion Purchase

        #region ECR settings
        public List<IEcrDepartment> EcrDepartments { get; private set; }

        public ObservableCollection<EcrConfig> EcrSettings
        {
            get
            {
                return new ObservableCollection<EcrConfig>(Settings.MemberSettings.EcrConfigs);
            }
        }

        public EcrConfig SelectedEcrSettings
        {
            get { return _selectedEcrSettings ?? (_selectedEcrSettings = new EcrConfig()); }
            set
            {
                _selectedEcrSettings = value; RaisePropertyChanged("SelectedEcrSettings");
                RaisePropertyChanged("OnExecuteEcrAction");
                RaisePropertyChanged("ManageEcrSettingsCommand");
                RaisePropertyChanged("NewEcrSettingsCommand");
                RaisePropertyChanged("ManageButtonContent");
            }
        }

        public bool IsSelectedBranchDefault
        {
            get
            {
                return SelectedEcrSettings.IsDefault;
            }
            set
            {
                if (value && EcrSettings.Any(s => s == SelectedEcrSettings))
                {
                    foreach (var ecrSetting in EcrSettings)
                    {
                        ecrSetting.IsDefault = false;
                    }
                }
                SelectedEcrSettings.IsDefault = value;
            }
        }

        //public string EcrSettingsIp
        //{
        //    get { return SelectedEcrSettings.Ip; }
        //    set
        //    {
        //        SelectedEcrSettings.Ip = value;
        //        RaisePropertyChanged("EcrSettingsIp");
        //        RaisePropertyChanged("ExecuteEcrActionCommand");
        //    }
        //}
        //public int? EcrSettingsPort
        //{
        //    get { return EcrSettings.Port>0?EcrSettings.Port:(int?)null; }
        //    set
        //    {
        //        EcrSettings.Port = value??0;
        //        RaisePropertyChanged("EcrSettingsPort");
        //        RaisePropertyChanged("ExecuteEcrActionCommand");
        //    }
        //}
        //public int EcrCashier
        //{
        //    get { return EcrSettings.EcrCashier.Cashier; }
        //    set
        //    {
        //        EcrSettings.EcrCashier.Cashier = value;
        //        RaisePropertyChanged("EcrCashier");
        //        RaisePropertyChanged("ExecuteEcrActionCommand");
        //    }
        //}
        //public string EcrCashierPin
        //{
        //    get { return EcrSettings.EcrCashier.Pin; }
        //    set
        //    {
        //        EcrSettings.EcrCashier.Pin = value;
        //        RaisePropertyChanged("EcrCashierPin");
        //        RaisePropertyChanged("ExecuteEcrActionCommand");
        //    }
        //}
        public bool CanActivateEcr
        {
            get
            {
                var canActivateEcr = EcrSettings != null && !string.IsNullOrEmpty(SelectedEcrSettings.Ip) && SelectedEcrSettings.Port > 0 &&
                       !string.IsNullOrEmpty(SelectedEcrSettings.Password) &&
                       SelectedEcrSettings.EcrCashier != null && SelectedEcrSettings.EcrCashier.Cashier > 0 && !string.IsNullOrEmpty(SelectedEcrSettings.EcrCashier.Pin) &&
                       SelectedEcrSettings.CashierDepartment != null;
                if (!canActivateEcr && EcrSettings != null)
                {
                    SelectedEcrSettings.IsActive = false;
                    RaisePropertyChanged("IsEcrActive");

                }
                return canActivateEcr;
            }
        }
        #endregion ECR settings

        #endregion Member Settings

        #region General settings


        #endregion Destop settings

        #region Ecr settings

        public string ManageButtonContent { get { return EcrSettings.Any(s => s == SelectedEcrSettings) ? "Հեռացնել" : "Ավելացնել"; } }
        #endregion Ecr settings

        #region Member settings

        #endregion MemberSettings

        #region Branch Properties
        public bool IsLocalBranch { get { return BranchSettings != null && BranchSettings.Id == SelectedBranch.Id; } }
        public string SetAsLocalBranchButtonText { get { return IsLocalBranch ? "Ակտիվ" : "Պասիվ"; } }
        #endregion Branch properties

        #endregion External properties

        #region Constructors
        public SettingsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Կարգաբերումներ";
            EcrDepartments = CashReg.Helper.Enumerations.GetEcrDepartments();
            LoadProperties();
        }
        #region Settings
        #endregion
        private void LoadProperties()
        {
            Settings.MemberSettings = MemberSettings.GetSettings(ApplicationManager.Member.Id);
            Branches = new ObservableCollection<BranchModel>(BranchManager.GetBranches());
            //Branches.SingleOrDefault(s=>s.Id == Settings.MemberSettings.BranchId);

            //Ecr Settings
            //var ecrConfig = ConfigSettings.GetEcrConfig();
            //EcrModel = ApplicationManager.Settings.MemberSettings.EcrModel; //ecrConfig ?? new EcrConfig();
            //EcrSettings.IsActive = ecrConfig != null && ecrConfig.IsActive ApplicationManager.Settings.MemberSettings.IsEcrActivated;
            //Printers

            LablePrinters = new List<ItemsForChoose>();
            SalePrinters = new List<ItemsForChoose>();
            CashDeskPrinters = new List<ItemsForChoose>();
            try
            {
                var printerQuery = new ManagementObjectSearcher("SELECT * from Win32_Printer");
                foreach (var printer in printerQuery.Get())
                {
                    var name = printer.GetPropertyValue("Name");
                    var status = printer.GetPropertyValue("Status");
                    var isDefault = printer.GetPropertyValue("Default");
                    //var isNetworkPrinter = printer.GetPropertyValue("Network");
                    SalePrinters.Add(new ItemsForChoose
                    {
                        Data = string.Format("{0} Status:{1}{2}", name, status, isDefault is bool && (bool)isDefault ? " (Default)" : ""),
                        Value = name,
                        IsChecked = string.Equals(name, Settings.MemberSettings.ActiveSalePrinter)
                    });
                    CashDeskPrinters.Add(new ItemsForChoose
                    {
                        Data = string.Format("{0} Status:{1}{2}", name, status, isDefault is bool && (bool)isDefault ? " (Default)" : ""),
                        Value = name,
                        IsChecked = string.Equals(name, Settings.MemberSettings.ActiveCashDeskPrinter)
                    });
                    LablePrinters.Add(new ItemsForChoose
                    {
                        Data = string.Format("{0} Status:{1}{2}", name, status, isDefault is bool && (bool)isDefault ? " (Default)" : ""),
                        Value = name,
                        IsChecked = string.Equals(name, Settings.MemberSettings.ActiveLablePrinter)
                    });
                }
            }
            catch (Exception ex)
            {
                ApplicationManager.Instance.AddMessageToLog(new MessageModel(ex.Message, MessageTypeEnum.Warning));
            }
            SelectedBranch = BranchSettings;
        }

        #endregion Internal methods

        #region External methods

        #endregion External methods

        #region Commands

        #region Save command
        private ICommand _saveCommand;
        private EcrConfig _selectedEcrSettings;
        public ICommand SaveCommand { get { return _saveCommand ?? (_saveCommand = new RelayCommand(OnSave)); } }
        private void OnSave(object obj)
        {
            //General
            _settings.MemberSettings.ActiveLablePrinter = LablePrinters.Where(s => s.IsChecked).Select(s => (string)s.Value).SingleOrDefault();
            _settings.MemberSettings.ActiveCashDeskPrinter = CashDeskPrinters.Where(s => s.IsChecked).Select(s => (string)s.Value).SingleOrDefault();
            //Sale
            _settings.MemberSettings.ActiveSaleStocks = SaleStocks.Where(s => s.IsChecked).Select(s => (short)s.Value).ToList();
            _settings.MemberSettings.SaleCashDesks = SaleCashDesks.Where(s => s.IsChecked).Select(s => (Guid)s.Value).ToList();
            _settings.MemberSettings.SaleBankAccounts = SaleBankAccounts.Where(s => s.IsChecked).Select(s => (Guid)s.Value).ToList();
            _settings.MemberSettings.ActiveSalePrinter = SalePrinters.Where(s => s.IsChecked).Select(s => (string)s.Value).SingleOrDefault();

            //Purchase
            _settings.MemberSettings.ActivePurchaseStocks = PurchaseStocks.Where(s => s.IsChecked).Select(s => (short)s.Value).ToList();
            _settings.MemberSettings.PurchaseCashDesks = PurchaseCashDesks.Where(s => s.IsChecked).Select(s => (Guid)s.Value).ToList();
            _settings.MemberSettings.PurchaseBankAccounts = PurchaseBankAccounts.Where(s => s.IsChecked).Select(s => (Guid)s.Value).ToList();

            //Logins

            //Ecr


            //Branch
            _settings.MemberSettings.BranchSettings = SelectedBranch;

            if (Settings.Save())
            {
                Settings.LoadMemberSettings();
                ApplicationManager.Settings.Reload();
                RaisePropertyChanged("EcrSettings");
                SelectedEcrSettings = new EcrConfig();
                MessageManager.OnMessage("Կարգավորումների գրանցումն իրականացել է հաջողությամբ:");
            }
            else
            {
                MessageManager.OnMessage("Կարգավորումների գրանցումը ձախողվել է:", MessageTypeEnum.Warning);
            }

        }
        #endregion Save command

        #region Ecr commands
        public ICommand ExecuteEcrActionCommand { get { return new RelayCommand<EcrExecuiteActions>(OnExecuteEcrAction, CanExecuteEcrAction); } }
        private bool CanExecuteEcrAction(EcrExecuiteActions actionMode)
        {
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckConnection:
                    return EcrSettings != null && !string.IsNullOrEmpty(SelectedEcrSettings.Ip) && SelectedEcrSettings.Port > 0;
                case EcrExecuiteActions.OperatorLogin:
                    return EcrSettings != null && !string.IsNullOrEmpty(SelectedEcrSettings.Ip) && SelectedEcrSettings.Port > 0 && SelectedEcrSettings.EcrCashier != null && !string.IsNullOrEmpty(SelectedEcrSettings.EcrCashier.Pin);
                    break;
                case EcrExecuiteActions.GetOperatorsList:
                case EcrExecuiteActions.GetDepsList:
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    return EcrSettings != null && !string.IsNullOrEmpty(SelectedEcrSettings.Ip) && SelectedEcrSettings.Port > 0;
                case EcrExecuiteActions.CheckEcrConnection:
                    return SelectedEcrSettings.IsActive && SelectedEcrSettings.EcrServiceSettings.IsActive && !string.IsNullOrEmpty(SelectedEcrSettings.EcrServiceSettings.Ip) && SelectedEcrSettings.EcrServiceSettings.Port > 0;
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
                case EcrExecuiteActions.PrintCash:
                    break;
                case EcrExecuiteActions.CashIn:
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
        private void ExecuteEcrAction(EcrExecuiteActions actionMode)
        {
            IsInProgress = true;
            var ecrserver = EcrManager.GetEcrServer(SelectedEcrSettings);
            MessageModel message = null;
            UsersAndDepartments operatorDeps;
            switch (actionMode)
            {
                case EcrExecuiteActions.CheckEcrConnection:
                    message = ecrserver.TryEcrConnection() ? new MessageModel("ՀԴՄ կապի ստուգումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.CheckConnection:
                    message = ecrserver.TryConnection() ? new MessageModel("Կապի ստուգումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կապի ստուգումը ձախողվել է:", MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.Zero:
                    break;
                case EcrExecuiteActions.GetOperatorsAndDepList:
                    operatorDeps = ecrserver.GetUsersDepsList();
                    break;
                case EcrExecuiteActions.GetOperatorsList:
                    operatorDeps = ecrserver.GetUsersDepsList();
                    if (operatorDeps == null)
                    {
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.Result.Description, ecrserver.Result.Code), MessageTypeEnum.Warning);
                    }
                    else
                    {
                        SelectedEcrSettings.TypeOfOperatorDeps = operatorDeps.d;
                        SelectedEcrSettings.CashierDepartment = SelectedEcrSettings.TypeOfOperatorDeps.FirstOrDefault();
                        RaisePropertyChanged("SelectedEcrSettings");
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                        var operators = operatorDeps.c;
                        string operatorsList = "";
                        for (int i = 0; i < operators.Count; i++)
                        {
                            operatorsList += operators[i].Id + " : " + operators[i].Name + "\n";
                        }

                        MessageBox.Show(operatorsList, "Operators list");
                    }
                    break;
                case EcrExecuiteActions.GetDepsList:
                    operatorDeps = ecrserver.GetUsersDepsList();
                    if (operatorDeps == null)
                    {
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.Result.Description, ecrserver.Result.Code), MessageTypeEnum.Warning);
                    }
                    else
                    {
                        SelectedEcrSettings.TypeOfOperatorDeps = operatorDeps.d;
                        SelectedEcrSettings.CashierDepartment = SelectedEcrSettings.TypeOfOperatorDeps.FirstOrDefault();
                        RaisePropertyChanged("SelectedEcrSettings");
                        message = new MessageModel("ՀԴՄ օպերատորի բաժինների ստացումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                    }
                    break;
                case EcrExecuiteActions.OperatorLogin:
                    message = ecrserver.TryOperatorLogin() ? new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success) : new MessageModel("ՀԴՄ օպերատորի մուտքի ստուգումը ձախողվել է:" + string.Format(" {0} ({1})", ecrserver.Result.Description, ecrserver.Result.Code), MessageTypeEnum.Warning);
                    break;
                case EcrExecuiteActions.LogoutOperator:
                    break;
                case EcrExecuiteActions.PrintReceiptTicket:
                    break;
                case EcrExecuiteActions.PrintLatestTicket:
                    break;
                case EcrExecuiteActions.PrintReturnTicket:
                    break;
                case EcrExecuiteActions.ManageHeaderAndFooter:
                    break;
                case EcrExecuiteActions.ManageLogo:
                    break;
                case EcrExecuiteActions.PrintEcrReport:
                    break;
                case EcrExecuiteActions.PrintReportX:
                    break;
                case EcrExecuiteActions.PrintReportZ:
                    break;
                case EcrExecuiteActions.GetReceiptData:
                    break;
                case EcrExecuiteActions.PrintCash:
                    break;
                case EcrExecuiteActions.CashIn:
                    break;
                case EcrExecuiteActions.CashWithdrawal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("actionMode", actionMode, null);
            }
            if (message != null)
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => MessageManager.OnMessage(message)));
                }
            }
            IsInProgress = false;
        }

        private ICommand _manageEcrSettingsCommand;

        public ICommand ManageEcrSettingsCommand
        {
            get { return _manageEcrSettingsCommand ?? (_manageEcrSettingsCommand = new RelayCommand<EcrExecuiteActions>(OnManageEcrSettings, CanManageEcrSettings)); }
        }

        private bool CanManageEcrSettings(EcrExecuiteActions obj)
        {
            return EcrSettings.Any(s => s == SelectedEcrSettings) || !string.IsNullOrEmpty(SelectedEcrSettings.Ip);
        }

        private void OnManageEcrSettings(EcrExecuiteActions obj)
        {
            if (EcrSettings.Any(s => s == SelectedEcrSettings))
            {
                Settings.MemberSettings.EcrConfigs.Remove(SelectedEcrSettings);
            }
            else
            {
                if (SelectedEcrSettings.IsDefault)
                    foreach (var ecrSetting in EcrSettings)
                    {
                        ecrSetting.IsDefault = false;
                    }
                Settings.MemberSettings.EcrConfigs.Add(SelectedEcrSettings);
            }
            RaisePropertyChanged("EcrSettings");
            OnNewEcrSettings(obj);
        }

        private ICommand _newEcrSettingsCommand;

        public ICommand NewEcrSettingsCommand
        {
            get { return _newEcrSettingsCommand ?? (_newEcrSettingsCommand = new RelayCommand<EcrExecuiteActions>(OnNewEcrSettings, CanNewEcrSettings)); }
        }

        private bool CanNewEcrSettings(EcrExecuiteActions obj)
        {
            return EcrSettings.Any(s => s == SelectedEcrSettings);
        }

        private void OnNewEcrSettings(EcrExecuiteActions obj)
        {
            SelectedEcrSettings = new EcrConfig();
            RaisePropertyChanged("SelectedEcrSettings");
        }

        private ICommand _saveEcrServiceSettingsCommand;
        private BranchModel _selectedBranch;

        public ICommand SaveEcrServiceSettingsCommand
        {
            get { return _saveEcrServiceSettingsCommand ?? (_saveEcrServiceSettingsCommand = new RelayCommand<EcrExecuiteActions>(OnSaveEcrSettings, CanSaveEcrSettings)); }
        }

        private bool CanSaveEcrSettings(EcrExecuiteActions obj)
        {
            return EcrSettings.Any(s => s == SelectedEcrSettings) && EcrServiceSettings.IsActive;
        }

        private void OnSaveEcrSettings(EcrExecuiteActions obj)
        {
            var ecrSettings = SelectedEcrSettings;
            ecrSettings.EcrServiceSettings = EcrServiceSettings;
            MemberSettings.SaveEcrService(SelectedEcrSettings, ApplicationManager.Member.Id);
        }
        #endregion Ecr commands

        #region Branch commands

        private ICommand _createNewBranchCmmand;
        public ICommand CreateNewBranchCommand { get { return _createNewBranchCmmand ?? (_createNewBranchCmmand = new RelayCommand(OnCreateNewBranch)); } }

        private void OnCreateNewBranch()
        {
            SelectedBranch = new BranchModel(ApplicationManager.Member.Id);
        }

        private ICommand _editBranchCommand;
        public ICommand EditBranchCommand { get { return _editBranchCommand ?? (_editBranchCommand = new RelayCommand(OnEditBranch)); } }

        private void OnEditBranch()
        {
            Branches.Add(SelectedBranch);
        }

        private ICommand _sendTestEmailCommand;
        public ICommand SendTestMailCommand { get { return _sendTestEmailCommand ?? (_sendTestEmailCommand = new RelayCommand(OnSendTestEmail, CanSendTestMail)); } }

        private bool CanSendTestMail(object obj)
        {
            return SelectedBranch.NoReplayMailSettings != null && !string.IsNullOrEmpty(SelectedBranch.Name) && !string.IsNullOrEmpty(SelectedBranch.NoReplayMailSettings.Email) && !string.IsNullOrEmpty(SelectedBranch.NoReplayMailSettings.SmtpServer) && SelectedBranch.NoReplayMailSettings.SmtpPort > 0;
        }

        private void OnSendTestEmail(object obj)
        {
            if (MailSender.SendMessageFromBrunch(SelectedBranch.NoReplayMailSettings.Email, "Թեսթային հաղորդագրություն", string.Format("Ուղարկվել է թեսթային հաղորդագրություն -ի կողմից ({1}): <br/>Եթե ստացել եք այս հաղոդագրությունը նշանակում է տվյալները ճիշտ են մուտքագրված: {0}", ApplicationManager.GetEsUser.FullName, SelectedBranch.Name), SelectedBranch))
            {
                MessageBox.Show("Փորձնական հաղորդագորությունն ուղարկվել է հաջողությամբ:");
            }
        }

        private ICommand _setLocalBranchCommand;
        public ICommand SetLocalBranchCommand { get { return _setLocalBranchCommand ?? (_setLocalBranchCommand = new RelayCommand(OnSetLocalBranch)); } }

        private void OnSetLocalBranch()
        {
            if (!IsLocalBranch)
            {
                Settings.MemberSettings.BranchSettings = SelectedBranch;
            }
            else
            {
                Settings.MemberSettings.BranchSettings = null;
            }
            RaisePropertyChanged("IsLocalBranch");
            RaisePropertyChanged("SendTestMailCommand");
            RaisePropertyChanged("SetAsLocalBranchButtonText");
        }

        #endregion Branch commands

        #endregion Commands
    }
}
