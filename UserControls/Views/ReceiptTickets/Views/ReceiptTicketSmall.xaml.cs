using System.Windows.Controls;

namespace UserControls.Views.ReceiptTickets.Views
{
    /// <summary>
    /// Interaction logic for ReceiptTicketSmall.xaml
    /// </summary>
    public partial class ReceiptTicketSmall : UserControl
    {
        public ReceiptTicketSmall()
        {
            InitializeComponent();
        }
        public ReceiptTicketSmall(SaleInvocieSmallTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
