using System.Windows.Controls;

namespace UserControls.Views.ReceiptTickets.Views
{
    /// <summary>
    /// Interaction logic for MoveInvoiceView.xaml
    /// </summary>
    public partial class MoveInvoiceView : UserControl
    {
        public MoveInvoiceView(MoveInvocieTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
