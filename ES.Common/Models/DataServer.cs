namespace ES.Common.Models
{
    public class DataServer
    {
        #region Internal properties

        private string _providerName = "System.Data.SqlClient";
        private string _connectionMetadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
        private bool _integratedSecurity;
        private bool _persistSecurityInfo;
        private bool _multipleActiveResultSets;
        #endregion

        #region External properties
        public string Description { get; set; }
        public string Name { get; set; }
        public string Instance { get; set; }
        public int? Port { get; set; }
        public string Database { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string ProviderName { get { return _providerName; } set { if (string.Equals(value, _providerName)) { return; } _providerName = value; } }
        public string ConnectionMetadata { get { return _connectionMetadata; } set { if (string.Equals(value, _connectionMetadata)) { return; } _connectionMetadata = value; } }
        public bool IntegratedSecurity { get { return _integratedSecurity; } set { if (string.Equals(value, _integratedSecurity)) { return; } _integratedSecurity = value; } }
        public bool PersistSecurityInfo { get { return _persistSecurityInfo; } set { _persistSecurityInfo = value; } }
        public bool MultipleActiveResultSets { get { return _multipleActiveResultSets; } set { _multipleActiveResultSets = value; } }
        #endregion External properties

        #region Constructors
        public DataServer()
        {
            Initialize();
        }
        public DataServer(string description, string serverName)
            : this()
        {
            Description = description;
            Name = serverName;
        }
        #endregion Constructors

        #region Internal methods
        private void Initialize()
        {
            _integratedSecurity = false;
            _persistSecurityInfo = true;
            _multipleActiveResultSets = true;
        }
        #endregion Internal methods

    }

}
