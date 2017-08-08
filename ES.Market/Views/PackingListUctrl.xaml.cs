using System.Windows;
using System.Windows.Controls;
using ES.Data.Model;
using ES.Data.Models;
using UserControls.ViewModels.Invoices;

namespace ES.Market.Views
{
    /// <summary>
    /// Interaction logic for PackingListUctrl.xaml
    /// </summary>
    public partial class PackingListUctrl : UserControl
    {
        private EsUserModel _user;
        private EsMemberModel _member;
        public PackingListUctrl(InvoiceModel invoice, EsUserModel user, EsMemberModel member)
        {
            _user = user;
            _member = member;
            InitializeComponent();
            DataContext = new InvoiceViewModel(invoice.Id);
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
