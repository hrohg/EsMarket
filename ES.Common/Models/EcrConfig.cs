using System;
using System.Collections.Generic;
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
        private string _ip;
        private int? _port;
        private string _password;
        private string _crn;
        private EcrCashier _ecrCashier;
        private List<Department> _typeOfOperatorDeps;
        private Department _cashierDepartment;
        private bool _useExternalPrinter;
        private string _applicationIp;

        #region External properties
        public EcrServiceSettings EcrServiceSettings { get; set; }
        public bool IsActive { get; set; }
        public string ExcelFilePath { get; set; }

        #endregion External properties

        //     #region Private properties
        //    private string _ecrServerIp = "217.113.7.68";
        //    private int _ecrConnectionPort = 9999;
        //    private string _ecrPk = "HS0FBZZZ";
        //    private int _cashierId = 3;
        //    private string _cashierPas = "3";
        //    #endregion
        //    #region Public properties
        //    public string ECRServerIP { get { return _ecrServerIp; } set { _ecrServerIp = value; } }
        //    public int ECRConnectionPort { get { return _ecrConnectionPort; } set { _ecrConnectionPort = value; } }
        //    public string ECRPrimaiyKey { get { return _ecrPk; } set { _ecrPk = value; } }
        //    public int CashierId { get { return _cashierId; } set { _cashierId = value; } }
        //    public string CashierPas { get { return _cashierPas; } set { _cashierPas = value; } }
        //    #endregion

        public EcrConfig()
        {
            Initialize();
        }

        #region Internal methods

        private void Initialize()
        {
            EcrServiceSettings = new EcrServiceSettings();
        }
        #endregion Internal methods

        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        public int? Port
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
            get { return _ecrCashier; }
            set { _ecrCashier = value; }
        }

        public List<Department> TypeOfOperatorDeps
        {
            get { return _typeOfOperatorDeps; }
            set { _typeOfOperatorDeps = value; }
        }

        public Department CashierDepartment
        {
            get { return _cashierDepartment; }
            set { _cashierDepartment = value; }
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
    }
}
