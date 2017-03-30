using System.Windows.Controls;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.PriceTicketControl
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
