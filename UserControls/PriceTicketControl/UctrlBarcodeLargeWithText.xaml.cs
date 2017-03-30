using System.Windows.Controls;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.PriceTicketControl
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
