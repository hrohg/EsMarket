using System.Windows;
using System.Windows.Input;
using Shared.Helpers;

namespace UserControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomExceptionBox.xaml
    /// </summary>
    public partial class EsExceptionBox
    {
        #region Internal properties

        #endregion Internal properties

        #region External properties
        public static object CanProceedHandleExceptionMonitor = new object();
        public System.Exception ExceptionHandled { get; set; }
        public event System.EventHandler<EsExceptionHandlingEventArgs> ExceptionHandlingApproved;
        public string ExceptionText
        {
            get { return (string)GetValue(ExceptionTextProperty); }
            set { SetValue(ExceptionTextProperty, value); }
        }
        public string ExceptionDetailText
        {
            get { return (string)GetValue(ExceptionDetailTextProperty); }
            set { SetValue(ExceptionDetailTextProperty, value); }
        }
        #endregion External properties

        #region Dependency properties
        // Using a DependencyProperty as the backing store for ExceptionText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptionTextProperty =
                    DependencyProperty.Register("ExceptionText", typeof(string), typeof(EsExceptionBox));

        // Using a DependencyProperty as the backing store for ExceptionDetailText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExceptionDetailTextProperty =
            DependencyProperty.Register("ExceptionDetailText", typeof(string), typeof(EsExceptionBox));

        #endregion Dependency properties

        #region Constructors
        public EsExceptionBox()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Internal methods
        private void btDetails_Click(object sender, RoutedEventArgs e)
        {
            if (scroll.Visibility == Visibility.Visible)
            {
                scroll.Visibility = Visibility.Collapsed;
            }
            else
            {
                scroll.Visibility = Visibility.Visible;
            }
        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (System.InvalidOperationException) { }
            if (ExceptionHandlingApproved != null)
                ExceptionHandlingApproved(this, new EsExceptionHandlingEventArgs(ExceptionHandled));
            lock (CanProceedHandleExceptionMonitor)
                System.Threading.Monitor.Pulse(CanProceedHandleExceptionMonitor);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void btnSendErrorReport(object sender, RoutedEventArgs e)
        {
            MailSender.SendErrorReport(ExceptionText, ExceptionDetailText);
            Close();
        }

        private void ExceptionBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control && e.Key == Key.F12)
            {
                //new ExpirationDateManagement().ShowDialog();
            }
        }
        #endregion Internal methods
        
        #region External methods
        public static void CustomShow(string ExceptionText, string ExecptionDetailText)
        {
            EsExceptionBox ceb = new EsExceptionBox();
            ceb.ExceptionText = ExceptionText;
            ceb.ExceptionDetailText = ExecptionDetailText;
            ceb.ShowDialog();
            ceb.Topmost = true;
        }
        #endregion External methods

    }

    public class EsExceptionHandlingEventArgs : System.EventArgs
    {
        public EsExceptionHandlingEventArgs(System.Exception exceptionHandled)
        {
            ExceptionHandled = exceptionHandled;
        }
        public System.Exception ExceptionHandled { get; set; }
    }
}
