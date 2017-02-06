using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CashReg.Helper;
using ES.Common.Helpers;

namespace ES.Common
{
    public static class ConfigSettings
    {
        #region Private methods

        #endregion
        public static bool SetDataServer(DataServer dataServer)
        {
            var resoult = new XmlManager().SetXmlElement(XmlTagItems.EsServers, XmlManager.Convert(XmlManager.SerializeToXmlElement(dataServer)), dataServer.Name);
            return resoult;
        }
        public static List<DataServer> GetDataServers()
        {
            var xmlServers = new XmlManager().GetXmlElements(XmlTagItems.EsServers);
            return xmlServers != null ? xmlServers.Select(s => XmlManager.DeserializeFromXmlElement<DataServer>(XmlManager.Convert(s))).ToList() : new List<DataServer>();
        }
        public static bool RemoveDataServer(DataServer dataServer)
        {
            if (dataServer == null) return false;
            return new XmlManager().RemoveXmlElement(XmlTagItems.EsServers, XmlManager.Convert(XmlManager.SerializeToXmlElement(dataServer)), dataServer.Name);
        }
        public static EcrConfig GetEcrConfig()
        {
            var xElement = new XmlManager().GetXmlElement(XmlTagItems.EcrConfig);
            return XmlManager.DeserializeFromXmlElement<EcrConfig>(XmlManager.Convert(xElement));
        }
        public static bool SetEcrConfig(string toElement, object o)
        {
            bool resoult = false;
            if (!string.IsNullOrEmpty(toElement) && o != null)
            {
                var xmlElement = XmlManager.SerializeToXmlElement(o);
                var xml = new XmlManager();
                resoult = xml.SetXmlElement(toElement, XmlManager.Convert(xmlElement));
            }
            return resoult;
        }

        public static bool SetConfig(string key, string value, string settings = "TempSettings")
        {
            var xml = new XmlManager();
           return xml.SetXmlElement(settings, new XElement(key, value));
        }
        public static string GetConfig(string key, string settings = "TempSettings")
        {
            var xml = new XmlManager();
            var firstOrDefault = xml.GetXmlElements(settings).FirstOrDefault(s => s.Name==key);
            if (firstOrDefault != null)
            {
                return firstOrDefault.Value;
            }
            return string.Empty;
        }
    }

    public class ServiceSettings : INotifyPropertyChanged
    {
        #region Internal properties

        private int _port;
        #endregion //Internal proerpties

        #region External properties
        public string Ip { get; set; }

        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                if(_port == value) return;
                if (value < 0)
                {
                    _port = 0;
                }
                else
                {
                    _port = value;
                }
                OnPropertyChanged("Port");
            }
        }

        public bool IsActive { get; set; }

        #endregion //External properties

        #region Constructors
        public ServiceSettings()
        {

        }
        public ServiceSettings(string ip, int port, bool isActive = false)
            : this()
        {
            Ip = ip;
            Port = port;
            IsActive = isActive;
        }
        #endregion //Constructors

        #region NotificationChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //NotificationChanged
    }
    public class EcrConfig : INotifyPropertyChanged
    {
        #region Internal properties
        private EcrSettings _ecrSettings;
        private ServiceSettings _ecrServiceSettings;
        private bool _isActive;
        #endregion //Internal properties

        #region External properties

        public EcrSettings EcrSettings
        {
            get { return _ecrSettings ?? (_ecrSettings = new EcrSettings()); }
            set { _ecrSettings = value; }
        }

        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public ServiceSettings EcrServiceSettings
        {
            get { return _ecrServiceSettings ?? (_ecrServiceSettings = new ServiceSettings()); }
            set { _ecrServiceSettings = value; }
        }

        #endregion //External properties

        #region Constructors
        public EcrConfig() { }

        public EcrConfig(EcrSettings settings, bool isActive)
            : base()
        {
            EcrSettings = settings;
            IsActive = isActive;
        }
        public EcrConfig(EcrSettings settings, bool isActive, ServiceSettings ecrServiceSettings)
            : base()
        {
            EcrSettings = settings;
            IsActive = isActive;
            EcrServiceSettings = ecrServiceSettings;
        }
        #endregion //Constructors

        #region NotificationChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion //NotificationChanged
    }

    public class ConfigItem
    {
        public string Key { get; set; }
        public string Data { get; set; }
        public object Value { get; set; }
    }

    public class DataServer
    {
        #region internal properties

        private string _providerName = "System.Data.EntityClient";
        private string _connectionMetadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
        private bool _integratedSecurity = false;
        private bool _persistSecurityInfo = true;
        private bool _multipleActiveResultSets = true;
        #endregion
        public string Description { get; set; }
        public string Name { get; set; }
        public string Instance { get; set; }
        public int? Port { get; set; }
        public string Database { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string ProviderName { get { return _providerName; } set { if (string.Equals(value, _providerName)) { return; } _providerName = value; } }
        public string ConnectionMetadata { get { return _connectionMetadata; } set { if (string.Equals(value, _connectionMetadata)) { return; } _connectionMetadata = value; } }
        public bool IntegratedSecurity { get { return _integratedSecurity; } set { _integratedSecurity = value; } }
        public bool PersistSecurityInfo { get { return _persistSecurityInfo; } set { _persistSecurityInfo = value; } }
        public bool MultipleActiveResultSets { get { return _multipleActiveResultSets; } set { _multipleActiveResultSets = value; } }

        public DataServer()
        {

        }
        public DataServer(string description, string serverName)
        {
            Description = description;
            Name = serverName;
        }
    }
}
