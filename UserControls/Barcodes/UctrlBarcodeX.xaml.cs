using System.Windows.Controls;
using UserControls.Barcodes.ViewModels;

namespace UserControls.Barcodes
{
    /// <summary>
    /// Interaction logic for UctrlBarcodeX.xaml
    /// </summary>
    public partial class UctrlBarcodeX : UserControl
    {
        public UctrlBarcodeX(BarcodeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
