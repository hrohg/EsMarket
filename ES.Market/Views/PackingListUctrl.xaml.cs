using System.Windows;
using System.Windows.Controls;
using ES.Data.Models;
using UserControls.ViewModels.Invoices;

namespace ES.Market.Views
{
    /// <summary>
    /// Interaction logic for PackingListUctrl.xaml
    /// </summary>
    public partial class PackingListUctrl : UserControl
    {
        public PackingListUctrl(InvoiceModel invoice)
        {
            InitializeComponent();
            DataContext = new PackingListViewModel(invoice.Id);
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            var tabitem = this.Parent as TabItem;
            if (tabitem != null)
            {
                var tabControl = tabitem.Parent as TabControl;
                if (tabControl == null) return;
                tabControl.Items.Remove(tabControl.SelectedItem);
            }
        }
    }
}
