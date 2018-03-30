using System;
using System.Collections.Generic;
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
    public class EcrConfig : IEcrSettings
    {
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

        #endregion Internal properties

        #region External properties
        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
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
            get { return _ecrCashier??(_ecrCashier= new EcrCashier()); }
            set { _ecrCashier = value; }
        }

        public List<Department> TypeOfOperatorDeps
        {
            get { return _typeOfOperatorDeps; }
            set { _typeOfOperatorDeps = value; }
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
            }
        }
        public Department CashierDepartment
        {
            get { return _cashierDepartment != null && TypeOfOperatorDeps.Any()? TypeOfOperatorDeps.SingleOrDefault(s => s.Id == _cashierDepartment.Id) : null; }
            set
            {
                if (value == null) { return; }
                _cashierDepartment = value; 
            }
        }

        public bool UseExternalPrinter
        {
            get { return _useExternalPrinter; }
            set { _useExternalPrinter = value; }
        }

        public string ApplicationIp
        {
            get { return _applicationIp; }
            set { _applicationIp = value; }
        }

        public EcrServiceSettings EcrServiceSettings { get; set; }
        public bool IsActive { get; set; }
        public string ExcelFilePath { get; set; }

        public bool UseExtPos
        {
            get { return _useExtPos; }
            set { _useExtPos = value; }
        }

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
