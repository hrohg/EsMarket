using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ES.Common.Managers;

namespace ES.Login
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow(LoginViewModel vm)
        {
            if(vm == null) Application.Current.Shutdown();
            InitializeComponent();            
            DataContext = vm;
        }

        private void TxtLogin_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key != Key.Enter || string.IsNullOrEmpty(TxtLogin.Text)) return;
            //MoveToNextUIElement(e);
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key != Key.Enter || string.IsNullOrEmpty(TxtPassword.Password)) return;
            //MoveToNextUIElement(e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter) MoveToNextUIElement(e);
        }

        private void TxtLogin_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                 case   Key.Delete:
                    new XmlManager().RemoveXmlElement(XmlTagItems.Logins, new XElement(XmlTagItems.Login) {Value = (string) TxtLogin.SelectedItem});
                    break;
                default:
                    break;
            }
        }
    }
}
