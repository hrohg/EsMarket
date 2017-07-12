using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ES.Common.Helpers;
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

        void MoveToNextUIElement(KeyEventArgs e)
        {
            // Creating a FocusNavigationDirection object and setting it to a
            // local field that contains the direction selected.
            FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;

            // MoveFocus takes a TraveralReqest as its argument.
            TraversalRequest request = new TraversalRequest(focusDirection);

            // Gets the element with keyboard focus.
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;

            // Change keyboard focus.
            if (elementWithFocus != null)
            {
                if (elementWithFocus.MoveFocus(request)) e.Handled = true;
            }
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
