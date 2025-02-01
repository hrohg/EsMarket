using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using ES.Business.Helpers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Models;
using ES.DataAccess.Models;

namespace ES.Business.Managers
{
    public class ApplicationManager
    {
        private readonly object _lockDataServer = new object();
        private readonly object _lockLocalDataServer = new object();
        public delegate void OnReceivedLog(MessageModel log);
        public delegate void OnProgressChanged();

        public event OnReceivedLog ReveiveLogEvent;
        public event OnProgressChanged ProgressChangedEvent;

        #region Internal fields
        private DataServer _server;
        private readonly MessageManager _messageManager;
        private string _connectionString;

        private bool _isServerValid;
        private bool _isServerLocal;
        #endregion Internal fields

        #region Internal properties
        private readonly object _lockProgress = new object();
        private static EsMemberModel _member = new EsMemberModel();
        private static EsMembersAccountsModel _membersAccounts;
        private EsUserModel _esUser = new EsUserModel(0);
        private static List<MembersRoles> _userRoles;
        private static CashManager _cashProvider;
        private readonly Dictionary<string, int> ProgressValues = new Dictionary<string, int>();
        public bool IsProcessing { get { lock (_lockProgress) return ProgressValues.Any(); } }
        public int ProgressValue { get { lock (_lockProgress) return ProgressValues.Any() ? ProgressValues.Sum(s => s.Value) / ProgressValues.Count : -1; } }
        public void SetProgressValue(string key, int value = 0)
        {
            lock (_lockProgress)
            {
                if (!ProgressValues.ContainsKey(key)) ProgressValues.Add(key, value);
                else { ProgressValues[key] = value; }
                if (ProgressValues[key] >= 100) RemoveProgressValue(key);
            }
            DispatcherWrapper.Instance.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, () =>
            {
                var handler = ProgressChangedEvent;
                if (handler != null) handler();
            });
        }
        public void RemoveProgressValue(string key)
        {
            lock (_lockProgress)
            {
                ProgressValues.Remove(key);
            }
            DispatcherWrapper.Instance.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, () =>
            {
                var handler = ProgressChangedEvent;
                if (handler != null) handler();
            });
        }
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
        private ApplicationSettingsViewModel _settings;
        public static ApplicationSettingsViewModel Settings
        {
            get { return _insatance._settings ?? (_insatance._settings = new ApplicationSettingsViewModel()); }
        }
        #endregion Settings
        public string GetServerName()
        {
            return _server != null ? _server.Description : string.Empty;
        }
        public string GetDbName()
        {
            return _server != null ? _server.Database : string.Empty;
        }
        public bool IsServerValid { get { lock (_lockDataServer) return _isServerValid; } private set { lock (_lockDataServer) _isServerValid = value; } }
        public bool IsServerLocal { get { lock (_lockLocalDataServer) return _isServerLocal; } private set { lock (_lockLocalDataServer) _isServerLocal = value; } }
        public EsMemberModel GetMember { get { return _member; } }
        public EsUserModel GetUser { get { return _esUser; } }

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
        public static int MemberId { get { return Member != null ? Member.Id : -1; } }
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

        public List<MembersRoles> UserRoles
        {
            get { return _userRoles; }
            private set
            {
                _userRoles = value;
            }
        }
        public static EsUserModel GetEsUser { get { return Instance.GetUser; } }
        public static int ActiveUserId { get { return Instance.GetUser.UserId; } }
        public EsUserModel SetEsUser { set { _esUser = value; } }
        public static LicensePlansEnum License { get { return CashManager.Instance.License; } }
        public string GetDefaultConnectionString()
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
        public string GetServerConnectionString()
        {
            string providerName = "System.Data.SqlClient";
            string serverName = "bamboo.arvixe.com"; // "93.187.163.33,14033";
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
            sqlBuilder.UserID = "esstockdb_user"; // "sa";
            sqlBuilder.Password = "esstockdb@)!$"; //"academypbx569280";

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
        public void SetDataServer(DataServer server)
        {
            lock (_lockDataServer) _server = server;
            CreateConnectionString();
            //new Thread(() => { lock (_lockDataServer) IsServerValid = _server != null ? DatabaseManager.CheckConnection(_server.GetConnectionString()) : false; }).Start();
            new Thread(() => { lock (_lockDataServer) IsServerValid = _server != null ? DatabaseManager.CheckConnection(_connectionString) : false; }).Start();
            new Thread(() => { lock (_lockLocalDataServer) IsServerLocal = _server != null ? DatabaseManager.CheckConnection(_server.GetLocalConnectionString()) : false; }).Start();
        }
        private bool CreateConnectionString()
        {
            DataServer server = _server;
            if (server != null)
            {
                switch (server.Description.ToLower())
                {
                    case "esserver":
                        _connectionString = GetServerConnectionString();
                        IsEsServer = true;
                        break;
                    case "default":
                        _connectionString = GetDefaultConnectionString();
                        IsEsServer = false;
                        break;
                    case "local":
                        _connectionString = LocalhostConnectionString;
                        IsEsServer = false;
                        break;
                    default:                        
                        IsEsServer = false;
                        _connectionString = _server.GetConnectionString();
                        if (string.IsNullOrEmpty(_connectionString)) return false;                       
                        break;
                }
            }
            else
            {
                _connectionString = GetServerConnectionString();
                IsEsServer = true;
            }
            return true;            
        }
        public string GetConnectionString() { return _connectionString; }
        public CashManager CashProvider
        {
            get { return CashManager.Instance; }
        }
        public static CashManager CashManager { get { return Instance.CashProvider; } }
        public static MessageManager MessageManager { get { return _insatance._messageManager; } }

        #region Main Thread
        static int _mainThreadId;

        // If called in the non main thread, will return false;
        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId; }
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

        private static void Initialize()
        {
            // In Main method:
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        //private void OnMessageReceived(string message, MessageTypeEnum type)
        //{
        //    MessageManager.OnMessage(new MessageModel(message, type));
        //}

        private void ResetMemberData()
        {
            Settings.LoadMemberSettings();
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

                case UserRoleEnum.Moderator:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Moderator) || IsInRole(UserRoleEnum.Admin); ;
                    break;

                case UserRoleEnum.Director:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Director);
                    break;

                case UserRoleEnum.StockKeeper:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.StockKeeper);
                    break;

                //Sellers
                case UserRoleEnum.SaleManager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SaleManager);
                    break;
                case UserRoleEnum.SeniorSeller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SeniorSeller) || IsInRole(UserRoleEnum.SaleManager);
                    break;
                case UserRoleEnum.Seller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Seller) || IsInRole(UserRoleEnum.SeniorSeller);
                    break;
                case UserRoleEnum.JuniorSeller:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.JuniorSeller);
                    break;

                //Cashiers
                case UserRoleEnum.SeniorCashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SeniorCashier);
                    break;
                case UserRoleEnum.Cashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Cashier) || IsInRole(UserRoleEnum.SeniorCashier);
                    break;
                case UserRoleEnum.JuniorCashier:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.JuniorCashier);
                    break;

                //Managers
                case UserRoleEnum.SeniorManager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.SeniorManager);
                    break;
                case UserRoleEnum.Manager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.Manager) || IsInRole(UserRoleEnum.SeniorManager);
                    break;
                case UserRoleEnum.JuniorManager:
                    isInRole = Instance.UserRoles.Any(r => (UserRoleEnum)r.Id == UserRoleEnum.JuniorManager);
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
        #endregion
    }


}
