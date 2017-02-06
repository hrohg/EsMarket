using System.Windows.Controls;

namespace UserControls.Views.ReceiptTickets.Views
{
    /// <summary>
    /// Interaction logic for InvoicePreview.xaml
    /// </summary>
    public partial class InvoicePreview : UserControl
    {
        public InvoicePreview(SaleInvocieSmallTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
