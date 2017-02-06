using System.Windows.Controls;
using UserControls.Barcodes.ViewModels;

namespace UserControls.Barcodes
{
    /// <summary>
    /// Interaction logic for UctrlBarcodeLargeWithText.xaml
    /// </summary>
    public partial class UctrlBarcodeLargeWithText : UserControl
    {
        public UctrlBarcodeLargeWithText(BarcodeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
