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
        public static string DataSource { get; private set; }
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
                string serverName = "ESServer";
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
        //public static string GitunikConnectionString
        //{
        //    get
        //    {
        //        string providerName = "System.Data.SqlClient";
        //        string serverName = "87.241.191.72";
        //        string databaseName = "EsStockDb";

        //        // Initialize the connection string builder for the
        //        // underlying provider.
        //        SqlConnectionStringBuilder sqlBuilder =
        //            new SqlConnectionStringBuilder();

        //        // Set the properties for the data source.
        //        sqlBuilder.DataSource = serverName;
        //        sqlBuilder.InitialCatalog = databaseName;
        //        sqlBuilder.IntegratedSecurity = false;
        //        sqlBuilder.PersistSecurityInfo = true;
        //        sqlBuilder.MultipleActiveResultSets = true;
        //        sqlBuilder.UserID = "sa";
        //        sqlBuilder.Password = "kinutigkirqop";

        //        // Build the SqlConnection connection string.
        //        string providerString = sqlBuilder.ToString();

        //        // Initialize the EntityConnectionStringBuilder.
        //        EntityConnectionStringBuilder entityBuilder =
        //            new EntityConnectionStringBuilder();

        //        //Set the provider name.
        //        entityBuilder.Provider = providerName;

        //        // Set the provider-specific connection string.
        //        entityBuilder.ProviderConnectionString = providerString;

        //        // Set the Metadata location.
        //        entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
        //        return entityBuilder.ConnectionString;
        //    }
        //}
        //public static string AramConnectionString
        //{
        //    get
        //    {
        //        string providerName = "System.Data.SqlClient";
        //        string serverName = @"87.241.164.170\esl";
        //        string databaseName = "EsStockDb";

        //        // Initialize the connection string builder for the
        //        // underlying provider.
        //        SqlConnectionStringBuilder sqlBuilder =
        //            new SqlConnectionStringBuilder();

        //        // Set the properties for the data source.
        //        sqlBuilder.DataSource = serverName;
        //        sqlBuilder.InitialCatalog = databaseName;
        //        sqlBuilder.IntegratedSecurity = false;
        //        sqlBuilder.PersistSecurityInfo = true;
        //        sqlBuilder.MultipleActiveResultSets = true;
        //        sqlBuilder.UserID = "sa";
        //        sqlBuilder.Password = "eslsqlserver@)!$";

        //        // Build the SqlConnection connection string.
        //        string providerString = sqlBuilder.ToString();

        //        // Initialize the EntityConnectionStringBuilder.
        //        EntityConnectionStringBuilder entityBuilder =
        //            new EntityConnectionStringBuilder();

        //        //Set the provider name.
        //        entityBuilder.Provider = providerName;

        //        // Set the provider-specific connection string.
        //        entityBuilder.ProviderConnectionString = providerString;

        //        // Set the Metadata location.
        //        entityBuilder.Metadata = @"res://*/Models.EsStockDbModel.csdl|res://*/Models.EsStockDbModel.ssdl|res://*/Models.EsStockDbModel.msl";
        //        return entityBuilder.ConnectionString;
        //    }
        //}
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
            _server = server;
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
                        DataSource =
                            "Data Source=localhost;Initial Catalog=EsStockDb;Integrated Security=True;Persist Security Info=True;User ID=;Password=;MultipleActiveResultSets=True";
                        IsEsServer = false;
                        break;
                    default:
                        _server = server;
                        IsEsServer = false;
                        if (string.IsNullOrEmpty(server.Name) || string.IsNullOrEmpty(server.Database)) { return false; }
                        var sqlBuilder = new SqlConnectionStringBuilder();
                        // Set the properties for the data source.
                        sqlBuilder.DataSource = server.DataSource;
                        sqlBuilder.InitialCatalog = server.Database;
                        sqlBuilder.IntegratedSecurity = server.IntegratedSecurity;
                        sqlBuilder.PersistSecurityInfo = server.PersistSecurityInfo;
                        sqlBuilder.MultipleActiveResultSets = server.MultipleActiveResultSets;
                        sqlBuilder.UserID = server.Login ?? string.Empty;
                        sqlBuilder.Password = server.Password ?? string.Empty;

                        // Build the SqlConnection connection string.
                        string providerString = sqlBuilder.ToString();
                        DataSource = sqlBuilder.ToString();
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

        #region Main Thread
        static int mainThreadId;

        // If called in the non main thread, will return false;
        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == mainThreadId; }
        }
        #endregion Main Thread
        #endregion External properties

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
            // In Main method:
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
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
            if (Instance.UserRoles == null) return false;
            var isInRole = false;
            switch (type)
            {
                case UserRoleEnum.Admin:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Admin);
                    break;
                case UserRoleEnum.Director:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Director) || IsInRole(UserRoleEnum.Admin);
                    break;
                case UserRoleEnum.Manager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Manager) || IsInRole(UserRoleEnum.Director);
                    break;
                case UserRoleEnum.StockKeeper:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.StockKeeper) || IsInRole(UserRoleEnum.Manager);
                    break;
                case UserRoleEnum.SaleManager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SaleManager) || IsInRole(UserRoleEnum.Manager);
                    break;
                case UserRoleEnum.SeniorSeller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SaleManager) || IsInRole(UserRoleEnum.SaleManager);
                    break;
                case UserRoleEnum.Seller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Seller) || IsInRole(UserRoleEnum.SeniorSeller);
                    break;
                case UserRoleEnum.Cashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Cashier) || IsInRole(UserRoleEnum.SeniorCashier);
                    break;
                case UserRoleEnum.SeniorCashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SeniorCashier) || IsInRole(UserRoleEnum.Manager);
                    break;
                case UserRoleEnum.JuniorCashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.JuniorCashier) || IsInRole(UserRoleEnum.Cashier);
                    break;
                case UserRoleEnum.JuniorSeller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.JuniorSeller) || IsInRole(UserRoleEnum.Seller);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return isInRole;
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
