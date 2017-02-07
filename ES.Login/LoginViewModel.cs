using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Common;

namespace ES.Login
{
    public class LoginViewModel : ViewModelBase
    {
        #region Events

        public delegate void Logining();
        public event Logining OnLogining;

        public delegate void Logined(EsUserModel esUser);
        public event Logined OnLogined;

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
            LoginCommnd = new RelayCommand(OnLogin, CanLogin);
            CloseCommnd = new RelayCommand<bool?>(OnClose);
            SetLogins();
        }

        private bool CanLogin(object o)
        {
            return !string.IsNullOrEmpty(Login) && Password != null && Password.Length > 0;
        }

        private void OnLogin(object o)
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

        private void OnClose(bool? o)
        {
            var handler = OnClosed;
            if (handler != null) handler(o ?? true);
        }

        private void OnTryLogin()
        {
            var loginingHandler = OnLogining;
            if (loginingHandler != null) loginingHandler();
            LoginMessage = string.Empty;

            while (!ApplicationManager.CreateConnectionString(UserControls.Helpers.SelectItemsManager.SelectServer(_servers)))
            {
                var handler = OnClosed;
                if (handler != null) handler(true);
                return;
            }
            var password = Password;
            Password = new SecureString();
            var user = ApplicationManager.SetEsUser = UsersManager.VerifyLogin(Login, password);
            var loginedHandler = OnLogined;
            if (loginedHandler == null)
            {
                OnClose(false);
                return;
            }
            if (user == null)
            {

                loginedHandler(null);
                LoginMessage = "Սխալ մուտքային տվյալներ:";
            }
            else
            {
                var xml = new XmlManager();
                var login = new XElement(XmlTagItems.Login);
                login.Value = Login;
                xml.SetXmlElement(XmlTagItems.Users, login);
                var loginUser = new XElement(XmlTagItems.Login) { Value = Login };
                xml.AddXmlElement(XmlTagItems.Logins, loginUser);
                SetLogins();

                loginedHandler(user);
                //TxtPassword.Focus();
            }
        }

        private void SetLogins()
        {
            var logins = new XmlManager().GetXmlElements(XmlTagItems.Logins).ToList();
            Logins = logins.Select(s => s.Value).ToList();
        }
        #endregion Internal methods

        #region Commands
        public ICommand LoginCommnd { get; private set; }
        public ICommand CloseCommnd { get; private set; }
        #endregion Commands
    }
}
