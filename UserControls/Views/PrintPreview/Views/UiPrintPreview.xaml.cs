using System.Windows;
using System.Windows.Controls;

namespace UserControls.Views.PrintPreview.Views
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class UiPrintPreview : Window
    {
        public UiPrintPreview(UserControl uctrl)
        {
            InitializeComponent();
            PageContent.Children.Add(uctrl);
        }
    }
}
