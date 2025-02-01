using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace ES.Common.Models
{
    [Serializable]
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
        private string ConnectionMetadata { get { return _connectionMetadata; } set { if (string.Equals(value, _connectionMetadata)) { return; } _connectionMetadata = value; } }
        public bool IntegratedSecurity { get { return _integratedSecurity; } set { if (string.Equals(value, _integratedSecurity)) { return; } _integratedSecurity = value; } }
        public bool PersistSecurityInfo { get { return _persistSecurityInfo; } set { _persistSecurityInfo = value; } }
        public bool MultipleActiveResultSets { get { return _multipleActiveResultSets; } set { _multipleActiveResultSets = value; } }
        public string DataSource
        {
            get
            {
                return string.Format("{0}{1}{2}",
                    Name,
                    !string.IsNullOrEmpty(Instance) ? string.Format(@"\{0}", Instance) : string.Empty,
                    Port != null && Port != 0 ? string.Format(",{0}", Port) : string.Empty);
            }
        }
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

        #region External properties
        public string GetConnectionString()
        {
            return GenerateConnectionString(Name, Port, Database, Login, Password);
        }
        public string GetLocalConnectionString()
        {
            return GenerateConnectionString("localhost", Port, Database, Login, Password);
        }
        #endregion External properties

        #region Internal methods
        private void Initialize()
        {
            _integratedSecurity = false;
            _persistSecurityInfo = true;
            _multipleActiveResultSets = true;
        }
        private string GenerateConnectionString(string userId, string password)
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Database)) { return null; }
            var sqlBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder();
            // Set the properties for the data source.
            sqlBuilder.DataSource = DataSource;
            sqlBuilder.InitialCatalog = Database;
            sqlBuilder.IntegratedSecurity = IntegratedSecurity;
            sqlBuilder.PersistSecurityInfo = PersistSecurityInfo;
            sqlBuilder.MultipleActiveResultSets = MultipleActiveResultSets;
            sqlBuilder.UserID = Login ?? string.Empty;
            sqlBuilder.Password = Password ?? string.Empty;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();
            string dataSource = sqlBuilder.ToString();
            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = ProviderName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = ConnectionMetadata;
            return entityBuilder.ConnectionString;
        }

        private string GenerateConnectionString(string server, int? port, string database, string userId, string password)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database)) { return null; }
            if (string.IsNullOrEmpty(userId)) return null;
            var sqlBuilder = new SqlConnectionStringBuilder();
            // Set the properties for the data source.
            sqlBuilder.DataSource = string.Format("{0}{1}{2}",
                server,
                !string.IsNullOrEmpty(Instance) ? string.Format(@"\{0}", Instance) : string.Empty,
                port != null && port != 0 ? string.Format(",{0}", port.Value) : string.Empty);
            sqlBuilder.InitialCatalog = database;
            sqlBuilder.IntegratedSecurity = IntegratedSecurity;
            sqlBuilder.PersistSecurityInfo = PersistSecurityInfo;
            sqlBuilder.MultipleActiveResultSets = MultipleActiveResultSets;
            sqlBuilder.UserID = userId ?? string.Empty;
            sqlBuilder.Password = password ?? string.Empty;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = ProviderName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = ConnectionMetadata;
            return entityBuilder.ConnectionString;
        }
        #endregion Internal methods

    }
}
