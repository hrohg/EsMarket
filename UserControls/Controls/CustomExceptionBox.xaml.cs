using System.Windows;

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
        #endregion External properties
        
        #region Constructors
        public EsExceptionBox()
        {
            InitializeComponent();
        }
        #endregion Constructors

        #region Internal methods
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion Internal methods
        
        #region External methods
        
        #endregion External methods

    }
}
