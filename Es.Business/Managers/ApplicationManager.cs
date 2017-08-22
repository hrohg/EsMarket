using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Controls;
using ES.Business.Helpers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Model;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class ApplicationManager
    {
        public delegate void OnReceivedLog(MessageModel log);
        public event OnReceivedLog ReveiveLogEvent;

        #region Internal fields
        private static DataServer _server;
        private readonly MessageManager _messageManager;
        #endregion Internal fields

        #region Internal properties
        private static EsMemberModel _member = new EsMemberModel();
        private static EsMembersAccountsModel _membersAccounts;
        private static EsUserModel _esUser = new EsUserModel();
        private static List<MembersRoles> _userRoles;
        private static LocalManager _cashProvider;
        private static Common.Managers.MessageManager _mesManager;

        #endregion

        //private string CreateConnectionString(string server, string databaseName, string userName, string password)
        //{
        //    var builder = new SqlConnectionStringBuilder
        //    {
        //        DataSource = server, // server address
        //        InitialCatalog = databaseName, // database name
        //        IntegratedSecurity = false, // server auth(false)/win auth(true)
        //        MultipleActiveResultSets = false, // activate/deactivate MARS
        //        PersistSecurityInfo = true, // hide login credentials
        //        UserID = userName, // user name
        //        Password = password // password
        //    };
        //    return builder.ConnectionString;
        //}
        private static string CreateConnectionString(string host, string catalog)
        {
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = host,
                InitialCatalog = catalog,
                PersistSecurityInfo = true,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true,

                UserID = "",
                Password = "",
            };

            // assumes a connectionString name in .config of MyDbEntities
            var entityConnectionStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = sqlBuilder.ConnectionString,
                Metadata = "res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl",
            };

            return entityConnectionStringBuilder.ConnectionString;
        }

        #region External properties
        public static bool IsEsServer { get; private set; }

        #region Settings
        private SettingsContainer _settings;
        public static SettingsContainer Settings
        {
            get { return _insatance._settings ?? (_insatance._settings = new SettingsContainer()); }
        }
        #endregion Settings

        public static string DbName { get { return _server != null ? _server.Database : string.Empty; } }
        public EsMemberModel GetMember { get { return _member; } }

        public EsMembersAccountsModel GetEsMembersAccounts
        {
            get
            {
                if (_member == null)
                {
                    _membersAccounts = MembersManager.GetMembersAccounts(GetMember.Id);
                }
                return _membersAccounts;
            }
        }

        public static EsMemberModel Member
        {
            get { return Instance.GetMember; }
        }
        public EsMemberModel SetEsMember
        {
            set
            {
                _member = value;
                if (_member != null)
                {
                    UserRoles = UsersManager.GetUserRoles(GetEsUser.UserId, GetMember.Id);
                    ResetMemberData();
                }
            }
        }

        public List<MembersRoles> UserRoles { get { return _userRoles; } private set { _userRoles = value; } }
        public static EsUserModel GetEsUser { get { return _esUser; } }
        public static EsUserModel SetEsUser { set { _esUser = value; } }
        public static string DefaultConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = "ESLServer";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "sa";
                sqlBuilder.Password = "eslsqlserver@)!$";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        public static string ServerConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = "93.187.163.33,14033";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "sa";
                sqlBuilder.Password = "academypbx569280";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        public static string GitunikConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = "87.241.191.72";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "sa";
                sqlBuilder.Password = "kinutigkirqop";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        public static string AramConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = @"87.241.164.170\esl";
                string databaseName = "EsStockDb";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = false;
                sqlBuilder.PersistSecurityInfo = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.UserID = "sa";
                sqlBuilder.Password = "eslsqlserver@)!$";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
                return entityBuilder.ConnectionString;
            }
        }
        public static string LocalhostConnectionString
        {
            get
            {
                string providerName = "System.Data.SqlClient";
                string serverName = @"localhost";
                string databaseName = "EsStockDb";
                string user = "sa";
                string pass = "hhgpas";

                // Initialize the connection string builder for the
                // underlying provider.
                SqlConnectionStringBuilder sqlBuilder =
                    new SqlConnectionStringBuilder();

                // Set the properties for the data source.
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = databaseName;
                sqlBuilder.IntegratedSecurity = true;
                sqlBuilder.MultipleActiveResultSets = true;
                sqlBuilder.ApplicationName = "EntityFramework";
                //sqlBuilder.UserID = "sa";
                //sqlBuilder.Password = "eslsqlserver@)!$";

                // Build the SqlConnection connection string.
                string providerString = sqlBuilder.ToString();

                // Initialize the EntityConnectionStringBuilder.
                EntityConnectionStringBuilder entityBuilder =
                    new EntityConnectionStringBuilder();

                //Set the provider name.
                entityBuilder.Provider = providerName;

                // Set the provider-specific connection string.
                entityBuilder.ProviderConnectionString = providerString;

                // Set the Metadata location.
                entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";


                //return @"provider=System.Data.SqlClient;provider connection string=&quot;data source=localhost\ESL;initial catalog=ESStockDB;Integrated Security=True; MultipleActiveResultSets=True;App=EntityFramework&quot;";
                return CreateConnectionString(serverName, databaseName);
                //return entityBuilder.ConnectionString;
            }
        }
        public static bool CreateConnectionString(DataServer server)
        {
            if (server != null)
            {
                switch (server.Description.ToLower())
                {
                    case "esserver":
                        ConnectionString = ServerConnectionString;
                        IsEsServer = true;
                        break;
                    case "default":
                        ConnectionString = DefaultConnectionString;
                        IsEsServer = false;
                        break;
                    case "local":
                        ConnectionString = LocalhostConnectionString;
                        IsEsServer = false;
                        break;
                    default:
                        _server = server;
                        IsEsServer = false;
                        if (string.IsNullOrEmpty(server.Name) || string.IsNullOrEmpty(server.Database)) { return false; }
                        var sqlBuilder = new SqlConnectionStringBuilder();
                        // Set the properties for the data source.
                        sqlBuilder.DataSource = string.Format("{0}{1}{2}",
                            server.Name,
                            !string.IsNullOrEmpty(server.Instance) ? string.Format(@"\{0}", server.Instance) : string.Empty,
                            server.Port != null && server.Port != 0 ? string.Format(",{0}", server.Port) : string.Empty);
                        sqlBuilder.InitialCatalog = server.Database;
                        sqlBuilder.IntegratedSecurity = server.IntegratedSecurity;
                        sqlBuilder.PersistSecurityInfo = server.PersistSecurityInfo;
                        sqlBuilder.MultipleActiveResultSets = server.MultipleActiveResultSets;
                        sqlBuilder.UserID = server.Login ?? string.Empty;
                        sqlBuilder.Password = server.Password ?? string.Empty;

                        // Build the SqlConnection connection string.
                        string providerString = sqlBuilder.ToString();

                        // Initialize the EntityConnectionStringBuilder.
                        EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

                        //Set the provider name.
                        entityBuilder.Provider = server.ProviderName;

                        // Set the provider-specific connection string.
                        entityBuilder.ProviderConnectionString = providerString;

                        // Set the Metadata location.
                        entityBuilder.Metadata = server.ConnectionMetadata;
                        ConnectionString = entityBuilder.ConnectionString;
                        break;
                }
            }
            else
            {
                ConnectionString = ServerConnectionString;
                IsEsServer = true;
            }
            return true;
            //string serverName = "localhost", 
            //int port = 1433, 
            //string databaseName = "EsStockDb", 
            //string userName = "sa", 
            //string pass = "", 
            //string providerName = "System.Data.SqlClient", 
            //string connectionMetadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl"
            // Initialize the connection string builder for the
            // underlying provider.
        }
        public static string ConnectionString { get; set; }
        public LocalManager CashProvider
        {
            get { return _cashProvider ?? (_cashProvider = new LocalManager()); }
            set { _cashProvider = value; }
        }
        public static LocalManager CashManager { get { return Instance.CashProvider; } }
        public static MessageManager MessageManager { get { return _insatance._messageManager; } }
        #endregion

        #region Constructors
        private static ApplicationManager _insatance;
        public static ApplicationManager Instance
        {
            get { return _insatance ?? (_insatance = new ApplicationManager()); }
        }
        public ApplicationManager()
        {
            _messageManager = MessageManager.Manager;
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            
        }

        private void OnMessageReceived(string message, MessageTypeEnum type)
        {
            MessageManager.OnMessage(new MessageModel(message, type));
        }

        private void ResetMemberData()
        {
            Settings.LoadMemberSettings();
            CashProvider = new LocalManager();
        }
        #endregion

        #region External methods
        public static Uri GetServerPath()
        {
            return new Uri(string.Format("{0}{1}/{2}", "pack://application:,,,/Shared;", "component/Images/Server", IsEsServer ? "CloudServer.ico" : "LocalServer.ico"));
        }

        public static void OnTabItemClose(object o)
        {
            var tabControl = o as TabControl;
            var tabitem = tabControl != null ? tabControl.SelectedItem as TabItem : o as TabItem;
            if (tabControl == null && tabitem != null)
            {
                tabControl = (TabControl)tabitem.Parent;
            }
            if (tabitem != null)
            {
                tabControl.Items.Remove(tabitem);
            }
        }

        public void AddMessageToLog(MessageModel log)
        {
            MessageManager.OnMessage(log);
            var handler = ReveiveLogEvent;
            if (handler != null) handler(log);
        }

        public static bool IsInRole(UserRoleEnum? type)
        {
            switch (type)
            {
                case UserRoleEnum.Admin:
                    return Instance.UserRoles.Any(r => r.Id == 1);
                    break;
                case UserRoleEnum.Director:
                    return Instance.UserRoles.Any(r => r.Id == 2) || IsInRole(UserRoleEnum.Admin);
                    break;
                case UserRoleEnum.Manager:
                    return Instance.UserRoles.Any(r => r.Id == 3) || IsInRole(UserRoleEnum.Director);
                    break;
                case UserRoleEnum.StockKeeper:
                    return Instance.UserRoles.Any(r => r.Id == 4) || IsInRole(UserRoleEnum.Manager);
                    break;
                case UserRoleEnum.SaleManager:
                    return Instance.UserRoles.Any(r => r.Id == 5) || IsInRole(UserRoleEnum.Manager);
                    break;
                case UserRoleEnum.SeniorSeller:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.SaleManager) || IsInRole(UserRoleEnum.SaleManager);
                    break;
                case UserRoleEnum.Seller:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.Seller) || IsInRole(UserRoleEnum.SeniorSeller);
                    break;
                case UserRoleEnum.Cashier:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.Cashier) || IsInRole(UserRoleEnum.SeniorCashier);
                    break;
                case UserRoleEnum.SeniorCashier:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.SeniorCashier) || IsInRole(UserRoleEnum.JuniorCashier);
                    break;
                case UserRoleEnum.JuniorCashier:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.JuniorCashier) || IsInRole(UserRoleEnum.Director);
                    break;
                case UserRoleEnum.JuniorSeller:
                    return Instance.UserRoles.Any(r => r.Id == (int)UserRoleEnum.JuniorSeller) || IsInRole(UserRoleEnum.Director);
                    break;
                case null:
                    return false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static bool IsInRole(List<UserRoleEnum> types)
        {
            return types.Any(s => IsInRole(s));
        }

        public static void ReloadSettings()
        {
            _insatance._settings = new SettingsContainer();
            _insatance._settings.LoadMemberSettings();
        }
        #endregion
    }


}
