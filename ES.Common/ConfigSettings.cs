using System.Collections.Generic;
using System.Linq;
using CashReg.Helper;
using CashReg.Interfaces;
using ES.Common.Managers;
using ES.Common.Models;

namespace ES.Common
{
    public static class ConfigSettings
    {
        public static List<DataServer> GetDataServers(string filePath = null)
        {
            var xmlServers = new XmlManager(filePath).GetXmlElements(XmlTagItems.EsServers);
            return xmlServers != null ? xmlServers.Select(s => XmlManager.DeserializeFromXmlElement<DataServer>(XmlManager.Convert(s))).ToList() : new List<DataServer>();
        }
        public static EcrSettings GetEcrConfig(string filePath)
        {
            var xElement = new XmlManager(filePath).GetXmlElement(XmlTagItems.EcrConfig);
            var settings = XmlManager.DeserializeFromXmlElement<EcrSettings>(XmlManager.Convert(xElement.Element("EcrSettings"))) ?? new EcrSettings();
            return settings;
        }
    }

    //public class EcrConfig : INotifyPropertyChanged
    //{
    //    #region Internal properties
    //    private EcrSettings _ecrSettings;
    //    private EcrServiceSettings _ecrServiceSettings;
    //    private bool _isActive;
    //    #endregion Internal properties

    //    #region External properties

    //    public EcrSettings EcrSettings
    //    {
    //        get { return _ecrSettings ?? (_ecrSettings = new EcrSettings()); }
    //        set { _ecrSettings = value; }
    //    }

    //    public bool IsActive
    //    {
    //        get
    //        {
    //            return _isActive;
    //        }
    //        set
    //        {
    //            if (_isActive == value) return;
    //            _isActive = value;
    //            OnPropertyChanged("IsActive");
    //        }
    //    }

    //    public EcrServiceSettings EcrServiceSettings
    //    {
    //        get { return _ecrServiceSettings ?? (_ecrServiceSettings = new EcrServiceSettings()); }
    //        set { _ecrServiceSettings = value; }
    //    }

    //    #endregion External properties

    //    #region Constructors

    //    #endregion Constructors

    //    #region NotificationChanged
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected virtual void OnPropertyChanged(string propertyName)
    //    {
    //        var handler = PropertyChanged;
    //        if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion //NotificationChanged
    //}
}
