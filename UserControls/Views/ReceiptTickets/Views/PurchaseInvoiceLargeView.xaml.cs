using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UserControls.Views.ReceiptTickets.Views
{
    /// <summary>
    /// Interaction logic for PurchaseInvoiceLargeView.xaml
    /// </summary>
    public partial class PurchaseInvoiceLargeView : UserControl
    {
        public PurchaseInvoiceLargeView(PurchaseInvocieTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
