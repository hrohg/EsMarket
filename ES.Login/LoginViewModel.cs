using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Common.Models;

namespace ES.Login
{
    public class LoginViewModel : ViewModelBase
    {
        #region Events
        public delegate void LoginingDelegate();
        public event LoginingDelegate LoginEvent;

        public delegate void LoginDelegate(EsUserModel esUser, string login);
        public event LoginDelegate OnLogin;

        public delegate void Closed(bool isTerminated);
        public event Closed OnClosed;

        #endregion Events

        #region Internal properties
        private List<DataServer> _servers;
        #endregion Internal properties

        #region External properties
        #region Logins
        private List<string> _logins;
        public List<string> Logins
        {
            get { return _logins; }
            set
            {
                if (_logins != value)
                {
                    _logins = value;
                    RaisePropertyChanged("Logins");
                }
            }
        }
        #endregion Logins

        #region Login
        private string _login;
        public string Login
        {
            get { return _login; }
            set
            {
                if (_login != value)
                {
                    _login = value;
                    RaisePropertyChanged("Login");
                }
            }
        }
        #endregion Login

        #region Password
        private SecureString _password;
        public SecureString Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    RaisePropertyChanged("Password");
                }
            }
        }

        private bool _isClearPassword;
        public bool IsClearPassword
        {
            get { return _isClearPassword; }
            set
            {
                if (_isClearPassword != value)
                {
                    _isClearPassword = value;
                    RaisePropertyChanged("IsClearPassword");
                }
            }
        }
        #endregion Password

        #region Login Message
        private string _loginMessage;
        public string LoginMessage
        {
            get { return _loginMessage; }
            set
            {
                if (_loginMessage != value)
                {
                    _loginMessage = value;
                    RaisePropertyChanged("LoginMessage");
                }
            }
        }
        #endregion Login Message

        #endregion External properties

        #region Constructors
        public LoginViewModel(string login, List<string> logins, List<DataServer> servers)
        {
            Initialize();
            Login = login;
            Logins = logins;
            _servers = servers;
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            
        }

        private void OnTryLogin()
        {
            var logiHandler = LoginEvent;
            if (logiHandler != null) logiHandler();
            LoginMessage = string.Empty;

            var servers = UserControls.Helpers.SelectItemsManager.SelectServers(_servers);
            if(servers==null) return;
            while (!ApplicationManager.CreateConnectionString(servers.FirstOrDefault()))
            {
                var handler = OnClosed;
                if (handler != null) handler(true);
                return;
            }
            var password = Password;
            IsClearPassword = true;
            var user = ApplicationManager.SetEsUser = UsersManager.VerifyLogin(Login, password);
            IsClearPassword = false;
            var loginedHandler = OnLogin;
            if (loginedHandler == null)
            {
                OnClose(false);
                return;
            }
            if (user == null)
            {
                LoginMessage = "Սխալ մուտքային տվյալներ:";
                loginedHandler(null, null);
            }
            else
            {
                loginedHandler(user, Login);
                //TxtPassword.Focus();
            }
        }
        #endregion Internal methods

        #region Commands
        private ICommand _loginCommand;
        public ICommand LoginCommnd
        {
            get
            {
                return _loginCommand ?? (_loginCommand = new RelayCommand(OnLoginCompleted, CanLogin));
            }
        }
        private bool CanLogin(object o)
        {
            return !string.IsNullOrEmpty(Login) && Password != null && Password.Length > 0;
        }
        private void OnLoginCompleted(object o)
        {
            //while (!ApplicationManager.CreateConnectionString(UserControls.Helpers.SelectItemsManager.SelectServer(ConfigSettings.GetDataServers())))
            //{
            //    //Breack
            //    OnBrowseServerSettings();
            //    var configServer = new ServerConfig(new ServerViewModel(new DataServer()));
            //    configServer.ShowDialog();
            //    //this.Hide(); 
            //    return;
            //}
            OnTryLogin();
        }

        private ICommand _closeCommand;
        public ICommand CloseCommnd
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new RelayCommand<bool?>(OnClose));
            }
        }
        private void OnClose(bool? o)
        {
            var handler = OnClosed;
            if (handler != null) handler(o ?? true);
        }
        #endregion Commands
    }
}
