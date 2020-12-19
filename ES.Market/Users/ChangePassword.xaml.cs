using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ES.Business.Managers;
using ES.Data.Models;
using UserControls.ViewModels;

namespace ES.Market.Users
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        private EsUserModel _esUserModel;
        public ChangePassword(EsUserModel esUser)
        {
            _esUserModel = esUser;
            InitializeComponent();
            DataContext = new EsUserViewModel(_esUserModel);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) MoveToNextUIElement(e);
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            var esUserViewModel = DataContext as EsUserViewModel;
            if(esUserViewModel!=null && UsersManager.ChangePassword(esUserViewModel.EsUser))
            {
                TbErrorMessage.Foreground=Brushes.Green;
                TbErrorMessage.Text = "Գաղտնաբառը փոփոխվել է հաջողությամբ:";
            }
        else
            {
                TbErrorMessage.Foreground=Brushes.Red;
                TbErrorMessage.Text = "Գաղտնաբառը հնարավոր չէ փոփոխել: Խնդրում ենք փորձել կրկին:";
            }
        }

        private void TxtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var esUserViewModel = DataContext as EsUserViewModel;
            if(esUserViewModel==null || esUserViewModel.EsUser==null) return;
            esUserViewModel.EsUser.NewPassword = TxtNewPassword.Password;
        }

        private void TxtConformPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var esUserViewModel = DataContext as EsUserViewModel;
            if (esUserViewModel == null || esUserViewModel.EsUser == null) return;
            esUserViewModel.EsUser.ConfirmPassword = TxtConformPassword.Password;
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

        private void TxtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter || string.IsNullOrEmpty(((PasswordBox)sender).Password)) return;
            MoveToNextUIElement(e);
        }

        private void TxtNewPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || string.IsNullOrEmpty(((PasswordBox)sender).Password)) return;
            MoveToNextUIElement(e);
        }

        private void TxtConformPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || string.IsNullOrEmpty(((PasswordBox)sender).Password)) return;
            MoveToNextUIElement(e);
        }

    }
}
