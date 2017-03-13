using System.Windows.Controls;
using System.Windows.Input;
using UserControls.ViewModels.Invoices;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for InventoryWriteOffUctrl.xaml
    /// </summary>
    public partial class InventoryWriteOffUctrl : UserControl
    {
        public InventoryWriteOffUctrl()
        {
            InitializeComponent();
        }

        private void DgInvoiceItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid) sender).Focus();
        }
    }
}
