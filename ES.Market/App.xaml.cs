using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Model;
using ES.Login;
using ES.Market.Config;
using ES.Market.ViewModels;
using UserControls.Controls;
using UserControls.ViewModels;

namespace ES.Market
{
    public partial class App : Application
    {

        #region Internal fields

        private ObservableMruCollection<string> _logins;
        #endregion Internal fields

        #region Internal properties
        private SplashScreen _splash;

        #region LoginWindow
        private LoginWindow _loginWindow;
        private LoginWindow LoginWindow
        {
            get
            {
                if (_loginWindow != null)
                {
                    return _loginWindow;
                }
                _logins = new ObservableMruCollection<string>(GeneralSettings.LoadGeneralSettings().LastLogins, 10);
                var dataServers = DataServerSettings.GetDataServers();
                if (!dataServers.Any())
                {
                    new ServerConfig(new ServerViewModel(new DataServer())).ShowDialog();
                }
                var loginVm = new LoginViewModel(_logins.FirstOrDefault(), _logins.ToList(), DataServerSettings.GetDataServers());

                loginVm.LoginEvent += OnLogining;
                loginVm.OnLogin += OnLogin;
                loginVm.OnClosed += OnClosed;
                return _loginWindow = new LoginWindow(loginVm);
            }
        }
        #endregion LoginWindow

        #region Mrket window

        private ShellViewModel _shellVm;
        private ShellViewModel ShellVm
        {
            get
            {
                if (_shellVm == null)
                {
                    _shellVm = new ShellViewModel();
                    _shellVm.OnLogOut += OnLogOut;
                }
                return _shellVm;
            }
        }

        private MarketShell _market;
        private MarketShell Market
        {
            get
            {
                if (_market == null)
                {
                    _market = new MarketShell(ShellVm);
                    Market.Closing += OnClosing;
                    Market.Closed += OnMarketClosed;
                }
                return _market;
            }
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            var dataContext = ((Window)sender).DataContext as ShellViewModel;
            if (dataContext == null) return;
            if (!dataContext.Close())
            {
                e.Cancel = true;
                return;
            }
        }

        #endregion Market window

        #endregion Internal properties

        #region Constructos
        public App()
        {

        }
        #endregion Constructors

        #region Internal methods
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                //if(BeginTest()) return;
                base.OnStartup(e);
                this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            }
            catch (ApplicationException ex)
            {
                MessageManager.ShowMessage(ex.Message, "Application error", MessageBoxImage.Error);
                if (Application.Current != null) Current.Shutdown();
            }
            catch
            {
                MessageManager.ShowMessage("Unknown exception.", "Application error", MessageBoxImage.Error);
                if (Application.Current != null) Current.Shutdown();
            }
        }

        private bool BeginTest()
        {

            return InvoicesManager.BeginTest();
            return false;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            /*Clearing Resources and adding resource directly (without merging), needed for keeping the custom styles and for not inherit windows style across themes (theme independence)*/
            Application.Current.Resources.MergedDictionaries.Clear();
            var dictionary = new ResourceDictionary { Source = new Uri("/ResourceLibrary;component/Resources/Resources.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            dictionary = new ResourceDictionary { Source = new Uri("/UserControls;component/Views/Generic.xaml", UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);

            try
            {
                _splash = new SplashScreen("Application.png");
                ApplicationManager.CreateConnectionString(null);
                OnTryLogin();
                if (Application.Current != null) Application.Current.Shutdown();

            }
            catch (Exception ex)
            {
                new EsExceptionBox { DataContext = new ReportExceptionViewModel(ex) }.ShowDialog();
            }
        }

        private void OnTryLogin()
        {
            if (LoginWindow.ShowDialog() != null)
            {

            }
        }
        private void OnClosed(bool isterminated)
        {
            if (isterminated)
            {
                LoginWindow.Close();
                if (Application.Current != null) Current.Shutdown();
            }
        }

        private void OnLogin(EsUserModel esuser, string login)
        {
            if (esuser == null)
            {
                LoginWindow.ShowDialog();
            }
            else
            {
                _splash.Show(true);

                var generalSettings = GeneralSettings.LoadGeneralSettings();
                _logins.Add(login);
                generalSettings.LastLogins = _logins.ToList();
                GeneralSettings.SaveGeneralSettings(generalSettings);

                var members = MembersManager.GetMembersByUser(esuser.UserId);
                if (members == null || members.Count == 0)
                {
                    MessageManager.ShowMessage("Դուք ոչ մի համակարգում ընդգրկված չեք։ Ընդգրկվելու համար խնդրում ենք դիմել տնօրինություն կամ ծրագրային համակարգի ադմինիստրացիա։");
                    LoginWindow.ShowDialog();
                }
                else
                {
                    members = UserControls.Helpers.SelectItemsManager.SelectEsMembers(members, false);
                    if (members.Any())
                    {
                        ApplicationManager.Instance.SetEsMember = members.Single();
                        Market.ShowDialog();
                    }
                    else
                    {
                        Shutdown();
                    }
                }
            }
        }

        private void OnMarketClosed(object sender, EventArgs e)
        {
            Market.Closed -= OnMarketClosed;
            ShellVm.OnLogOut -= OnLogOut;
            ShellVm.Dispose();

            if (Application.Current != null) Application.Current.Shutdown();
        }

        private void OnLogOut()
        {
            Market.Closed -= OnMarketClosed;
            Market.Close();
            ShellVm.OnLogOut -= OnLogOut;
            ShellVm.Dispose();
            _market = null;
            _shellVm = null;
            LoginWindow.ShowDialog();
        }

        private void OnLogining()
        {
            LoginWindow.Hide();
        }

        private bool IsServer()
        {
            return ConfigurationManager.AppSettings["ServerID"] == "127.0.0.1";
        }

        private static void DeleteDatFile()
        {
            try
            {
                File.Delete(Constants.BusinessDatTempFilePath);
            }
            catch { }

        }

        private void ShowError(string message)
        {

        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(new Action(() =>
            new EsExceptionBox { DataContext = new ReportExceptionViewModel(e.Exception) }.ShowDialog()));
            e.Handled = true;
        }
        private static void ShutdownApplication()
        {
            DeleteDatFile();
            if (Application.Current != null) Application.Current.Shutdown();
        }
        #endregion Internal methods

        #region External methods

        #endregion External methods
    }
}
