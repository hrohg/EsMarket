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

        protected ReportExceptionViewModel Context
        {
            get { return _context; }
            set
            {
                _context = value;
                DataContext = value;
            }
        }

        protected static bool IsShown;
        private ReportExceptionViewModel _context;

        #endregion Internal properties

        #region External properties
        public static EsExceptionBox Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EsExceptionBox();
                    var context = new ReportExceptionViewModel();
                    _instance.Context = context;
                }
                return _instance;
            }
        }

        public Reporter Company
        {
            get { return Context.Reporter; }
            set { Context.Reporter = value; }
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
            IsShown = false;
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
            if (!IsShown)
            {
                IsShown = true;
                ShowDialog();
            }
        }
    }
}
