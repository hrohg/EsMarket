using System.Windows;
using UserControls.ViewModels;

namespace UserControls.Controls
{
    /// <summary>
    /// Interaction logic for PriceTagViewDialog.xaml
    /// </summary>
    public partial class PriceTagViewDialog : Window
    {
        public PriceTagViewDialog()
        {
            InitializeComponent();
            DataContext = new PriceTagsViewModel();
        }
    }
}
