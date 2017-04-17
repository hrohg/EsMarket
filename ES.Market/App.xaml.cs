using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Data.Model;
using ES.Login;
using ES.Market.ViewModels;
using UserControls.Controls;
using UserControls.ViewModels;

namespace ES.Market
{
   public partial class App : Application
    {
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
                var xml = new XmlManager();
                var xmlLogin = xml.GetXmlElement(XmlTagItems.Users);
                var login = xmlLogin != null ? xmlLogin.Value : string.Empty;
                var logins = new XmlManager().GetXmlElements(XmlTagItems.Logins).ToList();
                var loginVm = new LoginViewModel(login, logins.Select(s => s.Value).ToList(), ConfigSettings.GetDataServers());
                loginVm.OnLogining += OnLogining;
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
                    Market.Closed += OnMarketClosed;
                }
                return _market;
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
                base.OnStartup(e);
                this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Application.Current != null) Current.Shutdown();
            }
            catch
            {
                MessageBox.Show("Unknown exception.", "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Application.Current != null) Current.Shutdown();
            }
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
                //_splash.Show(true);

                //SettingsContainer settings = LoadSettings();
                //if (settings != null && !string.IsNullOrEmpty(settings.LastSelectedLanguage))
                //{
                //    CultureResources.ChangeCulture(new CustomCultureInfo(settings.LastSelectedLanguage));
                //}
                //else
                //{
                //    CultureResources.ChangeCulture(new CustomCultureInfo("hy-AM"));
                //}
                //CultureResources.ChangeCulture(new CustomCultureInfo("hy-AM"));


                //if (DatabaseManager.IsCanCreateLocalDb())
                //{
                //    var membersFromServer = MembersManager.GetEsMembersFromServer();
                //    membersFromServer = SelectItemsManager.SelectEsMembers(membersFromServer, false);
                //    if (membersFromServer == null || membersFromServer.Count == 0) { return;}

                //    return;
                //}


                ApplicationManager.CreateConnectionString(null);
                OnTryLogin();
                if (Application.Current != null) Application.Current.Shutdown();
                //while (!ApplicationManager.CreateConnectionString(UserControls.Helpers.SelectItemsManager.SelectServer(ConfigSettings.GetDataServers())))
                //{
                //    //Breack
                //    if (Application.Current!=null) Application.Current.Shutdown();

                //    var configServer = new ServerConfig(new ServerViewModel(new DataServer()));
                //    configServer.ShowDialog();
                //    //this.Hide();
                //    return;
                //}


                //        MainWindow mainWindow = new MainWindow();


                //        //var ok = VerifyAppInfo(sender, e);
                //        var ok = VerifyAppCopy();

                //        if (!ok)
                //        {
                //            ShutdownApplication();
                //        }

                //        Login loginForm = new Login();

                //        if (loginForm.ShowDialog() ?? false)
                //        {
                //            Session.Inst.User = loginForm.User;
                //            Current.MainWindow = mainWindow;

                //            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
                //            {

                //            }
                //            mainWindow.ShowDialog();
                //        }
                //        DeleteDatFile();
                //        Current.Shutdown();
            }
            catch (Exception ex)
            {
                new EsExceptionBox {DataContext = new ReportExceptionViewModel(ex)}.ShowDialog();
            }
        }

        private void OnTryLogin()
        {
            if (LoginWindow.ShowDialog() != null)
            {

            }

            //return;
            //var xml = new XmlManager();
            //var localMode = xml.GetElementInnerText(XmlTagItems.LocalMode);
            //ApplicationManager.LocalMode = Convert.ToBoolean(localMode);
            //ApplicationManager.SaleBySingle = HgConvert.ToBoolean(xml.GetElementInnerText(XmlTagItems.SaleBySingle));
            //ApplicationManager.BuyBySingle = HgConvert.ToBoolean(xml.GetElementInnerText(XmlTagItems.BuyBySingle));
            //var weighers = xml.GetItemsByControl(XmlTagItems.Weighers, XmlTagItems.Weigher);
            //ApplicationManager.Weighers = weighers.Select(s => new ScaleModel(s.Value.ToString())).ToList();
            //ApplicationManager.DefaultPrinter = xml.GetItemsByControl(XmlTagItems.Equipments, XmlTagItems.DefaultPrinter).Select(s => s.Value.ToString()).FirstOrDefault();
            ////Printers
            //ApplicationManager.ActivePrinter = xml.GetElementInnerText(XmlTagItems.ActivePrinter);
            //ApplicationManager.SalePrinter = xml.GetElementInnerText(XmlTagItems.SalePrinter);
            //ApplicationManager.BarcodePrinter = xml.GetElementInnerText(XmlTagItems.BarcodePrinter);
        }
        private void OnClosed(bool isterminated)
        {
            if (isterminated)
            {
                LoginWindow.Close();
                if (Application.Current != null) Current.Shutdown();
            }
        }

        private void OnLogin(EsUserModel esuser)
        {
            if (esuser == null)
            {
                LoginWindow.ShowDialog();
            }
            else
            {
                _splash.Show(true);

                var members = MembersManager.GetMembersByUser(esuser.UserId);
                members = UserControls.Helpers.SelectItemsManager.SelectEsMembers(members, false);
                if (members == null || members.Count == 0)
                {
                    MessageBox.Show("Դուք ոչ մի համակարգում ընդգրկված չեք։ Ընդգրկվելու համար խնդրում ենք դիմել տնօրինություն կամ ծրագրային համակարգի ադմինիստրացիա։");
                    LoginWindow.ShowDialog();
                    return;
                }

                var member = ApplicationManager.Instance.SetEsMember = members.Single();
                var xml = new XmlManager();
                ApplicationManager.GetThisDesk = CashDeskManager.GetCashDesk(xml.GetItemsByControl(XmlTagItems.SaleCashDesks).Select(s => HgConvert.ToGuid(s.Value)).SingleOrDefault(), member.Id);
                ApplicationManager.LoadConfigData(member.Id);
                
                Market.ShowDialog();
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

        //private void LoadServices()
        //{
        //    Session.Inst.BEManager = DataManager.GetInstance();
        //    return;
        //    try
        //    {
        //        Cryptography cryptography = new Cryptography(new SecurityObject().GetHardUniqueID());
        //        if (!cryptography.Decrypt(Constants.BusinessDatFilePath, Constants.BusinessDatTempFilePath))
        //        {
        //            new SimpleExceptionBox("This is not a valid copy of realtor, for details please visit www.kostandyan.com").ShowDialog();
        //        }

        //        using (var stream = File.Open(Constants.BusinessDatTempFilePath, FileMode.Open))
        //        {
        //            //byte[] assembly = new byte[stream.Length];
        //            //stream.Read(assembly, 0, (int)stream.Length);
        //            //Assembly be = Assembly.Load(assembly);

        //            //Type type = be.GetType("RealEstate.Business.Helpers.DataManager", true);
        //            //MethodInfo mi = type.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
        //            //Session.Inst.BEManager = (IDataManager)mi.Invoke(null, null);
        //            Session.Inst.BEManager = DataManager.GetInstance();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //new SimpleExceptionBox(ex.Message).ShowDialog();
        //        new SimpleExceptionBox("This is not a valid copy of realtor, for details please visit www.kostandyan.com").ShowDialog();
        //        if (Current != null)
        //        {
        //            DeleteDatFile();
        //            Current.Shutdown();
        //        }
        //    }
        //}

        private static void DeleteDatFile()
        {
            try
            {
                File.Delete(Constants.BusinessDatTempFilePath);
            }
            catch { }

        }

        private SettingsContainer LoadSettings()
        {
            return SettingsContainer.GetSettings();
        }

        //private bool VerifyAppCopy()
        //{
        //    RealtorSecurity security = new RealtorSecurity();
        //    if (!security.VerifyAppValidation())
        //    {
        //        ShowError(CultureResources.Inst["ItsNotAValidCopyOfRealtor"]);
        //        return false;
        //    }
        //    DateTime? expDate = new RealtorSecurity().GetExpirationDate();
        //    CustomExceptionBox box = new CustomExceptionBox { ExceptionText = "Trial period ended" };
        //    if (expDate.GetValueOrDefault(DateTime.Now) <= DateTime.Now.Date)
        //    {
        //        box.ShowDialog();
        //        return false;
        //    }
        //    return true;
        //}

        //private bool VerifyAppInfo(object sender, StartupEventArgs e)
        //{
        //    RealtorSecurity security = new RealtorSecurity();

        //    if (e.Args.Length > 0 && File.Exists(e.Args[0]))
        //    {
        //        string filePath = e.Args[0];
        //        string extension = Path.GetExtension(filePath);
        //        if (extension == ".arc")
        //        {

        //            string code = string.Empty;
        //            using (StreamReader reader = File.OpenText(filePath))
        //            {
        //                code = reader.ReadToEnd();
        //                reader.Close();
        //            }
        //            if (string.IsNullOrEmpty(code))
        //            {
        //                throw new Exception("WrongFileOrFileIsCorrupted");
        //            }
        //            DateTime? expirationDate;
        //            if (!security.ValidateExpirationDateCode(code, out expirationDate))
        //            {
        //                throw new Exception(CultureResources.Inst["IncorrectExpirationDateCode"]);
        //            }

        //            MessageBox.Show(string.Format(CultureResources.Inst["CodeEnteredExpirationDateX"], expirationDate.Value.ToString(StringEncription.DateFormat)), "Կոդը ընդունված է", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }


        //    Session.Inst.IsWithoutVerification = false;

        //    if (!security.VerifyAppValidation())
        //    {
        //        ShowError(CultureResources.Inst["ItsNotAValidCopyOfRealtor"]);
        //        return false;
        //    }

        //    if (!security.VerifyLastAccessDate())
        //    {
        //        ShowError(CultureResources.Inst["ComputerDateIsIncorrect"]);
        //        return false;
        //    }

        //    string errorMessage;
        //    while (true)
        //    {
        //        errorMessage = string.Empty;
        //        bool? OK = security.VerifyExpirationDate(out errorMessage);
        //        if (OK == null)
        //        {
        //            ShowError(errorMessage);
        //            return false;
        //        }

        //        if (OK == false)
        //        {
        //            ExpirationCodeWindow dialog = null;
        //            dialog = new ExpirationCodeWindow();
        //            if (!(dialog.ShowDialog() ?? false))
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(errorMessage))
        //    {
        //        //MessageBox.Show(errorMessage, "Ակտիվացման ժամկետ", MessageBoxButton.OK, MessageBoxImage.Information);
        //        SimpleExceptionBox box = new SimpleExceptionBox(errorMessage);
        //        box.Title = CultureResources.Inst["ActivationDate"];
        //        box.ShowDialog();
        //    }
        //    return true;
        //}

        //private void ShowError(string message)
        //{
        //    SimpleExceptionBox box = new SimpleExceptionBox(message);
        //    box.ShowDialog();
        //    if (Current != null)
        //    {
        //        DeleteDatFile();
        //        Current.Shutdown();
        //    }
        //}

        private void ShowError(string message)
        {

        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            new EsExceptionBox{DataContext = new ReportExceptionViewModel(e.Exception)}.ShowDialog();
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
