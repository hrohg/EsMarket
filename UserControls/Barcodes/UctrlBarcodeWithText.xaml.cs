using System.Windows.Controls;
using UserControls.Barcodes.ViewModels;

namespace UserControls.Barcodes
{
    /// <summary>
    /// Interaction logic for BarcodeWithText.xaml
    /// </summary>
    public partial class UctrlBarcodeWithText :UserControl
    {
        public UctrlBarcodeWithText(BarcodeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
