using System.Windows.Controls;
using UserControls.ViewModels.Invoices;

namespace UserControls.Views.ReceiptTickets.Views
{
    /// <summary>
    /// Interaction logic for InvoicePreview.xaml
    /// </summary>
    public partial class InvoicePreview : UserControl
    {
        public InvoicePreview(SaleInvoiceSmallTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        public InvoicePreview(InvoiceViewModelBase vm)
        {
            InitializeComponent();
            vm.IsPrintPreviewMode = true;
            DataContext = vm;
        }
    }
}
