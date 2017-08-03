using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using CashReg.Helper;
using CashReg.Interfaces;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;

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
            var exServer = DataServers.SingleOrDefault(s => s.Name == dataServer.Name && s.Description == dataServer.Description);
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

        #endregion Internal properties

        #region External properties
        public long MemberId { get; set; }

        #region General
        public string LastSelectedLanguage { get; set; }
        public string ActiveLablePrinter { get; set; }
        public bool IsOfflineMode { get; set; }
        public bool NotifyAboutIncomingInvoices { get; set; }
        public string ImportingFilePath { get; set; }
        #endregion General

        #region Sale
        private List<long> _activeSaleStocks;
        public List<long> ActiveSaleStocks
        {
            get { return _activeSaleStocks ?? (_activeSaleStocks = new List<long>()); }
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
        public bool IsEcrActivated { get; set; }

        #endregion Sale

        #region Purchase

        private List<long> _activePurchaseStocks;
        public List<long> ActivePurchaseStocks
        {
            get { return _activePurchaseStocks ?? (_activePurchaseStocks = new List<long>()); }
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

        #endregion Purchase

        #region Ecr settings

        private EcrConfig _ecrModel;
        public EcrConfig EcrConfig
        {
            get { return _ecrModel ?? (_ecrModel = new EcrConfig()); }
            set { _ecrModel = value; }
        }
        #endregion Ecr settings

        #endregion External properties

        #region Constructors

        public MemberSettings()
        {
            Initialize();
        }

        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {

        }
        private bool Save(long memberId)
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
        public static MemberSettings GetSettings(long memberId)
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

        public static bool Save(MemberSettings memberSettings, long memberId)
        {
            return memberSettings.Save(memberId);
        }
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
        public static bool ConvertConfigFile(long memberId)
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
            var logins = xmlLogins!=null? xmlLogins.Select(s => s.Value).ToList():new List<string>();
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

            memberSettings.EcrConfig = EcrConfig.Convert(conf);
            memberSettings.EcrConfig.Password = conf.Password;


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
