using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using CashReg.Helper;
using CashReg.Interfaces;


namespace ES.Common.Models
{
    [Serializable]
    public class EcrServiceSettings
    {
        #region Internal properties
        private int _port;
        #endregion Internal proerpties

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
                if (_port == value) return;
                if (value < 0)
                {
                    _port = 0;
                }
                else
                {
                    _port = value;
                }
            }
        }
        public bool IsActive { get; set; }

        #endregion //External properties

        #region Constructors
        public EcrServiceSettings()
        {
            Initialize();
        }
        //public EcrServiceSettings(string ip, int port, bool isActive = false): this()
        //{
        //    Ip = ip;
        //    Port = port;
        //    IsActive = isActive;
        //}
        #endregion Constructors

        #region Internal methods
        private void Initialize() { }
        #endregion Internal methods
    }

    [Serializable]
    public class EcrConfig : IEcrSettings, INotifyPropertyChanged
    {
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #region Internal properties
        private string _ip;
        private int _port;
        private string _password;
        private string _crn;
        private EcrCashier _ecrCashier;
        private List<Department> _typeOfOperatorDeps;
        private Department _cashierDepartment;
        private bool _useExternalPrinter;
        private string _applicationIp;
        private bool _useExtPos;
        private bool _isActive;
        private bool _isDefault;

        #endregion Internal properties

        #region External properties
        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                RaisePropertyChanged("Ip");
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged("Port");
            }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string Crn
        {
            get { return _crn; }
            set { _crn = value; }
        }
        public EcrCashier EcrCashier
        {
            get { return _ecrCashier ?? (_ecrCashier = new EcrCashier()); }
            set { _ecrCashier = value; }
        }

        public List<Department> TypeOfOperatorDeps
        {
            get { return _typeOfOperatorDeps; }
            set { _typeOfOperatorDeps = value; RaisePropertyChanged("TypeOfOperatorDeps"); }
        }

        [XmlIgnore]
        public int SelectedDepartmentId
        {
            get { return CashierDepartment != null ? CashierDepartment.Id : -1; }
            set
            {
                _cashierDepartment = TypeOfOperatorDeps.Any()
                    ? TypeOfOperatorDeps.SingleOrDefault(s => s.Id == value)
                    : null;
                RaisePropertyChanged("SelectedDepartmentId");
            }
        }
        public Department CashierDepartment
        {
            get { return _cashierDepartment != null && TypeOfOperatorDeps.Any() ? TypeOfOperatorDeps.SingleOrDefault(s => s.Id == _cashierDepartment.Id) : null; }
            set
            {
                if (value == null) { return; }
                _cashierDepartment = value;
                RaisePropertyChanged("CashierDepartment");
            }
        }

        public bool UseExternalPrinter
        {
            get { return _useExternalPrinter; }
            set
            {
                _useExternalPrinter = value;
                RaisePropertyChanged("UseExternalPrinter");
            }
        }

        public string ApplicationIp
        {
            get { return _applicationIp; }
            set
            {
                _applicationIp = value;
                RaisePropertyChanged("ApplicationIp"); 
            }
        }

        public EcrServiceSettings EcrServiceSettings { get; set; }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                RaisePropertyChanged("IsActive");
                RaisePropertyChanged("ActivityDescription");
            }
        }

        public string ActivityDescription { get { return IsActive ? "Ակտիվ" : "Պասիվ"; } }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value;
                RaisePropertyChanged("IsDefault"); 
                RaisePropertyChanged("DefaultStageDescription"); 
            }
        }

        public string DefaultStageDescription { get { return IsDefault ? "Հիմնական" : "Լրացուցիչ"; } }

        public string ExcelFilePath { get; set; }

        public bool UseExtPos
        {
            get { return _useExtPos; }
            set { _useExtPos = value; }
        }
        [XmlIgnore]
        public string TypeOfTaxes { get { return CashReg.Helper.Enumerations.GetEcrDepartments().Where(s => s.Id == SelectedDepartmentId).Select(s => s.Name).SingleOrDefault(); } }
        #endregion External properties

        #region Constructors
        public EcrConfig()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            EcrServiceSettings = new EcrServiceSettings();
        }
        #endregion Internal methods

        #region External methods

        public static EcrSettings Convert(EcrConfig config)
        {
            return new EcrSettings
            {
                Ip = config.Ip,
                Password = config.Password,
                Port = config.Port,
                CashierDepartment = config.CashierDepartment,
                ApplicationIp = config.ApplicationIp,
                Crn = config.Crn,
                EcrCashier = config.EcrCashier,
                TypeOfOperatorDeps = config.TypeOfOperatorDeps,
                UseExternalPrinter = config.UseExternalPrinter
            };
        }
        public static EcrConfig Convert(EcrSettings config)
        {
            return new EcrConfig
            {
                Ip = config.Ip,
                Password = config.Password,
                Port = config.Port,
                CashierDepartment = config.CashierDepartment,
                ApplicationIp = config.ApplicationIp,
                Crn = config.Crn,
                EcrCashier = config.EcrCashier,
                TypeOfOperatorDeps = config.TypeOfOperatorDeps,
                UseExternalPrinter = config.UseExternalPrinter
            };
        }
        #endregion External methods


    }
}
