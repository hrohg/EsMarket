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
        private InvoiceViewModel _viewModel;
        public InventoryWriteOffUctrl()
        {
            InitializeComponent();
            _viewModel = (InvoiceViewModel)DataContext;
        }

        private void TxtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                       
                        return;
                        break;
                }
            }
            if (e.Key == Key.Enter)
            {
                _viewModel.SetInvoiceItem(TxtCode.Text);
                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    return;
                }
                _viewModel.OnAddInvoiceItem(null);
            }
        }

        private void DgInvoiceItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((DataGrid) sender).Focus();
        }
    }
}
