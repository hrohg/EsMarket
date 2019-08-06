using System;
using System.Windows;
using System.Windows.Threading;
using UserControls.ViewModels;

namespace UserControls.Controls
{
    /// <summary>
    /// Interaction logic for CustomExceptionBox.xaml
    /// </summary>
    public partial class EsExceptionBox
    {
        #region Internal properties
        private static EsExceptionBox _instance;
        private static readonly ReportExceptionViewModel Context = new ReportExceptionViewModel();
        private static bool isShown;
        #endregion Internal properties

        #region External properties
        public static EsExceptionBox Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EsExceptionBox() { DataContext = Context };

                }
                return _instance;
            }
        }
        #endregion External properties

        #region Constructors
        private EsExceptionBox()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Internal methods

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Visibility = Visibility.Collapsed;
            isShown = false;
        }
        #endregion Internal methods

        #region External methods

        #endregion External methods

        public void OnException(DispatcherUnhandledExceptionEventArgs e)
        {
            OnException(e.Exception);
        }
        public void OnException(Exception e)
        {
            Context.OnException(e);
            if (!isShown)
                isShown = true;
            {
                this.ShowDialog();
            }
        }
    }
}
