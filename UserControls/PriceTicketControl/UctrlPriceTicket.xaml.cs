using System.Windows.Controls;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.PriceTicketControl
{
    /// <summary>
    /// Interaction logic for UctrlPriceTicket.xaml
    /// </summary>
    public partial class UctrlPriceTicket : UserControl
    {
        public UctrlPriceTicket(PriceTicketViewModelBase vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
