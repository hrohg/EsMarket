using System.Windows;
using ES.Common.Cultures;
using UserControls.Controls;
using UserControls.ViewModels;
using UserControls.Views;

namespace DatabaseManagement
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    public App()
	    {
	        
	    }
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			//if (DateTime.Now >= new DateTime(2011, 5, 25))
			//{
			//    System.Diagnostics.Process.Start("DataSave.exe");
			//    throw new Exception("Save Data");
			//}
			
			var splash = new SplashScreen("DMSplash.png");
			splash.Show(true);
			
			//Session.Inst.BEManager = DataManager.GetInstance();
			//CultureResources.ChangeCulture(new CustomCultureInfo("hy-AM"));
			
			MainWindow mainWindow = new MainWindow();
			Current.MainWindow = mainWindow;
			mainWindow.ShowDialog();

			Current.Shutdown();
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			EsExceptionBox box = new EsExceptionBox
			{
				DataContext = new ReportExceptionViewModel(e.Exception)
			};
			box.ShowDialog();
			e.Handled = true;
			Current.Shutdown();
		}
	}
}
