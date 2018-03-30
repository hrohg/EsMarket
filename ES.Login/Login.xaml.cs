using System.Windows;

namespace ES.Login
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow:Window
    {
        public LoginWindow(LoginViewModel vm)
        {
            if(vm == null) Application.Current.Shutdown();
            InitializeComponent();            
            DataContext = vm;
        }
    }
}
