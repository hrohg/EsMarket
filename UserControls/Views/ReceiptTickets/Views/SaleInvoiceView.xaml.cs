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
    /// Interaction logic for SaleInvoiceView.xaml
    /// </summary>
    public partial class SaleInvoiceView : UserControl
    {
        public SaleInvoiceView(SaleInvocieTicketViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
