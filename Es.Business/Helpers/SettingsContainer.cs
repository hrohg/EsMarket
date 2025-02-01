using CashReg.Helper;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace ES.Business.Helpers
{
    [Serializable]
    public class DataServerSettings
    {
        #region Internal fields
        private static readonly string Path;
        #endregion Internal fields

        #region External properties
        public List<DataServer> DataServers { get; set; }
        #endregion External properties

        #region Contructors
        static DataServerSettings()
        {
            Path = PathHelper.GetDataServerSettingsFilePath();
        }
        public DataServerSettings()
        {
            DataServers = GetDataServers();
        }
        #endregion Constructors

        #region External methods

        public bool Save()
        {
            if (string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(Path))) return false;
            FileStream fs = new FileStream(Path, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }
        public bool SetDataServer(DataServer dataServer)
        {
            var exServer = DataServers.SingleOrDefault(s => s.Description == dataServer.Description);
            if (exServer != null)
            {
                exServer.Instance = dataServer.Instance;
                exServer.Database = dataServer.Database;
                exServer.Port = dataServer.Port;
                exServer.Login = dataServer.Login;
                exServer.Password = dataServer.Password;
            }
            else
            {
                DataServers.Add(dataServer);
            }
            return Save();
        }
        public static List<DataServer> GetDataServers()
        {
            if (File.Exists(Path))
            {
                FileStream fileStream = new FileStream(Path, FileMode.Open);
                try
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    return ((DataServerSettings)serializer.Deserialize(fileStream)).DataServers;
                }
                catch (Exception)
                {
                    return new List<DataServer>();
                }
                finally
                {
                    fileStream.Close();
                }
            }
            return new List<DataServer>();
        }
        public static bool SetDataServer(List<DataServer> dataServers)
        {
            var dataServerSettings = new DataServerSettings();
            return dataServers.Aggregate(true, (current, dataServer) => (current && dataServerSettings.SetDataServer(dataServer)));
        }

        public static bool RemoveDataServer(List<DataServer> dataServers)
        {
            var result = true;
            var dataServerSettings = new DataServerSettings();
            foreach (var dataServer in dataServers)
            {
                var exDataServer = dataServerSettings.DataServers.SingleOrDefault(s => s.Description == dataServer.Description && s.Name == dataServer.Name);
                if (exDataServer == null)
                {
                    result = false;
                }
                else
                {
                    dataServerSettings.DataServers.Remove(exDataServer);
                }
            }
            return result && dataServerSettings.Save();
        }
        #endregion External methods
    }

    [Serializable]
    public class MemberSettings
    {
        #region Internal properties
        private string _scannerPort;
        private string _cashDeskPort;
        #endregion Internal properties

        #region External properties
        public int MemberId { get; set; }
        
        #region General

        public string LastSelectedLanguage { get; set; }
        public string ActiveLablePrinter { get; set; }
        public bool IsOfflineMode { get; set; }
        public bool NotifyAboutIncomingInvoices { get; set; }
        public string ImportingFilePath { get; set; }
        public bool IsEcrActivated { get; set; }
        public string ActiveCashDeskPrinter { get; set; }
        public string ScannerPort
        {
            get { return _scannerPort; }
            set { _scannerPort = value; }
        }
        public string CashDeskPort
        {
            get { return _cashDeskPort; }
            set { _cashDeskPort = value; }
        }
        public bool UseUnicCode
        {
            get { return _useUnicCode; }
            set { _useUnicCode = value; }
        }
        #endregion General

        #region Sale
        private List<short> _activeSaleStocks;
        public List<short> ActiveSaleStocks
        {
            get { return _activeSaleStocks ?? (_activeSaleStocks = new List<short>()); }
            set
            {
                _activeSaleStocks = value;
            }
        }
        private List<Guid> _saleCashDesk;
        public List<Guid> SaleCashDesks
        {
            get { return _saleCashDesk ?? (_saleCashDesk = new List<Guid>()); }
            set
            {
                _saleCashDesk = value;
            }
        }
        private List<Guid> _saleBankAccounts;
        public List<Guid> SaleBankAccounts
        {
            get { return _saleBankAccounts ?? (_saleBankAccounts = new List<Guid>()); }
            set
            {
                _saleBankAccounts = value;
            }
        }
        public string ActiveSalePrinter { get; set; }
        public bool SaleBySingle { get; set; }
        public bool IsPrintSaleTicket { get; set; }
        public string SecondaryScreenName { get; set; }
        #endregion Sale

        #region Purchase

        private List<short> _activePurchaseStocks;
        public List<short> ActivePurchaseStocks
        {
            get { return _activePurchaseStocks ?? (_activePurchaseStocks = new List<short>()); }
            set
            {
                _activePurchaseStocks = value;
            }
        }
        private List<Guid> _purchaseCashDesk;
        public List<Guid> PurchaseCashDesks
        {
            get { return _purchaseCashDesk ?? (_purchaseCashDesk = new List<Guid>()); }
            set
            {
                _purchaseCashDesk = value;
            }
        }
        private List<Guid> _purchaseBankAccounts;
        public List<Guid> PurchaseBankAccounts
        {
            get { return _purchaseBankAccounts ?? (_purchaseBankAccounts = new List<Guid>()); }
            set
            {
                _purchaseBankAccounts = value;
            }
        }
        public bool PurchaseBySingle { get; set; }
        public bool IsPrintPurchaseInvoice { get; set; }
        #endregion Purchase

        #region Ecr settings

        private EcrConfig _ecrConfig;
        private List<EcrConfig> _ecrModels;
        private bool _useUnicCode;

        private EcrServiceSettings _ecrServiceSettings;

        [XmlIgnore]
        public EcrConfig EcrConfig
        {
            get { return _ecrConfig ?? (EcrConfigs.Any(s => s.IsActive) ? (_ecrConfig = EcrConfigs.Any(s => s.IsActive && s.IsDefault) ? EcrConfigs.SingleOrDefault(s => s.IsActive && s.IsDefault) : EcrConfigs.FirstOrDefault(s => s.IsActive)) : new EcrConfig()); }
        }
        public List<EcrConfig> EcrConfigs
        {
            get { return _ecrModels ?? (_ecrModels = new List<EcrConfig>()); }
            set { _ecrModels = value; }
        }

        public EcrServiceSettings EcrServiceSettings
        {
            get { return _ecrServiceSettings ?? (_ecrServiceSettings = new EcrServiceSettings()); }
            set { _ecrServiceSettings = value; }
        }

        #endregion Ecr settings        

        #region Branch settings
        public BranchModel BranchSettings { get; set; }

        #endregion Branch settings
                
        #region Server settings
        public ServerSettings ServerSettings { get; set; }
        #endregion Server settings

        #endregion External properties

        #region Constructors

        public MemberSettings()
        {
        }

        #endregion Constructors

        #region Internal methods        
        private bool Save(int memberId)
        {
            MemberId = memberId;
            string settingsFilePath = PathHelper.GetMemberSettingsFilePath(MemberId);
            if (string.IsNullOrEmpty(settingsFilePath)) return false;
            FileStream fs = new FileStream(settingsFilePath, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, this);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }

        #endregion Internal methods

        #region External methods
        public static MemberSettings GetSettings(int memberId)
        {
            try
            {
                string settingsFilePath = PathHelper.GetMemberSettingsFilePath(memberId);
                if (File.Exists(settingsFilePath))
                {
                    FileStream fileStream = new FileStream(settingsFilePath, FileMode.Open);
                    try
                    {
                        BinaryFormatter serializer = new BinaryFormatter();
                        return (MemberSettings)serializer.Deserialize(fileStream);
                    }
                    catch (Exception)
                    {
                        return new MemberSettings();
                    }
                    finally
                    {
                        fileStream.Close();
                    }
                }
                return new MemberSettings();
            }
            catch (Exception ex)
            {
                ApplicationManager.Instance.AddMessageToLog(new MessageModel(ex.Message, MessageTypeEnum.Warning));
                return new MemberSettings();
            }
        }

        public static bool Save(MemberSettings memberSettings, int memberId)
        {
            return memberSettings.Save(memberId);
        }

        public static bool SaveEcrService(EcrConfig config, int memberId)
        {
            string settingsFilePath = PathHelper.GetMemberEcrServiceFilePath(memberId);
            if (string.IsNullOrEmpty(settingsFilePath)) return false;
            FileStream fs = new FileStream(settingsFilePath, FileMode.OpenOrCreate);
            XmlSerializer serializer = new XmlSerializer(typeof(EcrConfig));
            try
            {
                serializer.Serialize(fs, config);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
            }

        }

        //public List<ItemsForChoose> GetSecondaryScreens()
        //{
        //    if (_secondaryScreens == null || _secondaryScreens.Count == 0)
        //    {
        //        _secondaryScreens = new List<ItemsForChoose>();
        //        Screen[] secondaryScreens = Screen.AllScreens.Where(s => !s.Primary).ToArray();
        //        _secondaryScreens = new List<ItemsForChoose>();
        //        foreach (var screen in secondaryScreens)
        //        {
        //            _secondaryScreens.Add(new ItemsForChoose((e) =>
        //            {
        //                SecondaryScreenName = _secondaryScreens.Where(s => s.IsChecked).Select(s => (string)s.Value).SingleOrDefault();
        //            })
        //            {
        //                Data = screen,
        //                Value = screen.DeviceName,
        //                IsChecked = screen.DeviceName == SecondaryScreenName
        //            });
        //        }
        //    }
        //    return _secondaryScreens;
        //}
        #endregion External methods
    }

    [Serializable]
    public class GeneralSettings
    {
        #region External properties
        #region Login settings

        private List<string> _lastLogins;
        public List<string> LastLogins
        {
            get { return _lastLogins ?? (_lastLogins = new List<string>()); }
            set { _lastLogins = value; }
        }
        #endregion Login settings

        #endregion External properties

        #region Constructors

        #endregion Constructors

        #region Internal methods

        #endregion Internal methods

        #region External methods

        public static GeneralSettings LoadGeneralSettings()
        {
            string settingsFilePath = PathHelper.GetGeneralSettingsFilePath();
            if (File.Exists(settingsFilePath))
            {
                FileStream fileStream = new FileStream(settingsFilePath, FileMode.Open);
                try
                {
                    BinaryFormatter serializer = new BinaryFormatter();
                    return (GeneralSettings)serializer.Deserialize(fileStream);
                }
                catch (Exception)
                {
                    return new GeneralSettings();
                }
                finally
                {
                    fileStream.Close();
                }
            }
            return new GeneralSettings();
        }
        public static bool SaveGeneralSettings(GeneralSettings generalSettings)
        {
            string settingsFilePath = PathHelper.GetGeneralSettingsFilePath();
            if (string.IsNullOrEmpty(settingsFilePath)) return false;
            FileStream fs = new FileStream(settingsFilePath, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, generalSettings);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                fs.Close();
            }
        }
        #endregion External methods
    }

    public class SettingsContainer
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties

        #region General
        private GeneralSettings _generalSettings;
        public GeneralSettings GeneralSettings
        {
            get
            {
                return _generalSettings ?? (_generalSettings = new GeneralSettings());
            }
            set { _generalSettings = value; }
        }
        #endregion General

        #region Member settings
        private MemberSettings _memberSettings;
        public MemberSettings MemberSettings
        {
            get
            {
                return _memberSettings ?? (_memberSettings = new MemberSettings());
            }
            set { _memberSettings = value; }
        }
        #endregion Member settings

        #endregion External properties

        #region Constructors
        public SettingsContainer()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            LoadGeneralSettings();
        }

        private void LoadGeneralSettings()
        {
            _generalSettings = new GeneralSettings();
        }

        #endregion Internal methods

        #region External methods

        public void LoadMemberSettings()
        {
            MemberSettings = MemberSettings.GetSettings(ApplicationManager.Member.Id);
        }
        public bool Save()
        {
            return MemberSettings.Save(MemberSettings, ApplicationManager.Member.Id);
        }

        //public static SettingsContainer LoadSettings()
        //{

        //    if (Settings != null) return Settings;
        //    string settingsFilePath = string.Format(@"{0}\{1}", PathHelper.GetLocalAppDataPath(), Constants.SettingsContainerFilePath);
        //    if (File.Exists(settingsFilePath))
        //    {
        //        FileStream fileStream = new FileStream(settingsFilePath, FileMode.Open);
        //        try
        //        {
        //            BinaryFormatter serializer = new BinaryFormatter();
        //            return (SettingsContainer)serializer.Deserialize(fileStream);
        //        }
        //        catch (Exception)
        //        {
        //            return new SettingsContainer();
        //        }
        //        finally
        //        {
        //            fileStream.Close();
        //        }
        //    }
        //    return new SettingsContainer();
        //}
        public static bool ConvertConfigFile(int memberId)
        {
            var title = "Config file";
            var filter = "Xml file | *.xml";
            var filePath = new OpenFileDialog { Title = title, InitialDirectory = AppDomain.CurrentDomain.BaseDirectory, Filter = filter };
            filePath.ShowDialog();
            if (string.IsNullOrEmpty(filePath.FileName) || !File.Exists(filePath.FileName)) return false;

            //ServerSettings
            var dataServers = ConfigSettings.GetDataServers(filePath.FileName);
            if (DataServerSettings.SetDataServer(dataServers))
            {
                MessageManager.OnMessage("Սերվերների տվյալների խմբագրումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Սերվերների տվյալների խմբագրումը ձախողվել է։", MessageTypeEnum.Warning);
            }
            //General settings
            var xmlLogins = new XmlManager(filePath.FileName).GetXmlElements(XmlTagItems.Logins);
            var logins = xmlLogins != null ? xmlLogins.Select(s => s.Value).ToList() : new List<string>();
            var generalSettings = GeneralSettings.LoadGeneralSettings();
            generalSettings.LastLogins = logins.ToList();
            if (GeneralSettings.SaveGeneralSettings(generalSettings))
            {
                MessageManager.OnMessage("Հիմանակն տվյալների խմբագրումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Հիմանակն տվյալների խմբագրումը ձախողվել է։", MessageTypeEnum.Warning);
            }

            //MemberSettings
            var memberSettings = MemberSettings.GetSettings(memberId);
            EcrSettings conf = ConfigSettings.GetEcrConfig(filePath.FileName);
            var ecrConfig = EcrConfig.Convert(conf);
            ecrConfig.Password = conf.Password;
            memberSettings.EcrConfigs.Add(ecrConfig);


            if (MemberSettings.Save(memberSettings, ApplicationManager.Member.Id))
            {
                MessageManager.OnMessage("Տվյալների խմբագրումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Տվյալների խմբագրումը ձախողվել է։", MessageTypeEnum.Warning);
            }
            return true;
        }
        #endregion

        #region Events

        public delegate void SettingsChangedDelegate();

        public event SettingsChangedDelegate SettingsChanged;

        private void OnSettingsChanged()
        {
            var handler = SettingsChanged;
            if (handler != null) handler();
        }

        #endregion Events
    }
}
