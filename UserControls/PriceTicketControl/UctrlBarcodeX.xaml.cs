using System.Windows.Controls;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.PriceTicketControl
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
