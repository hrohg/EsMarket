using System.Windows.Controls;
using Shared.Implementations;

namespace UserControls.Views.PrintPreview.Views
{
    /// <summary>
    /// Interaction logic for PrintPreview.xaml
    /// </summary>
    public partial class UiPrintPreview : EsWindow
    {
        public UiPrintPreview(UserControl uctrl)
        {
            InitializeComponent();
            PageContent.Children.Add(uctrl);
        }
    }
}
