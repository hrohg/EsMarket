using System;
using System.Windows;
using System.Windows.Threading;

namespace ES.Login
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    Application.Current.Resources.MergedDictionaries.Clear();
        //    Application.Current.Resources.Clear();
        //    var dictionary = new ResourceDictionary() { Source = new Uri("/ResourceLibrary;component/Resources/Resources.xaml", UriKind.Relative) };
        //    Application.Current.Resources.MergedDictionaries.Add(dictionary);
        //    try
        //    {
        //        base.OnStartup(e);
        //        this.DispatcherUnhandledException+=Application_DispatcherUnhandledException;
        //    }
        //    catch (ApplicationException ex)
        //    {
        //        MessageBox.Show(ex.Message, "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        Current.Shutdown();
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Unknown exception.", "Application error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        Current.Shutdown();
        //    }
        //}
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.Shutdown();
        }

        public static void ShutdownApplication()
        {
            Current.Shutdown();
        }
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
    }
}
