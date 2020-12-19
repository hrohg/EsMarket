using System.Windows;
using Es.Market.Tools.ViewModels;
using ES.Common.ViewModels.Base;

namespace Es.Market.Tools.Controls
{
    /// <summary>
    /// Interaction logic for LabelPrinter.xaml
    /// </summary>
    public partial class LabelPrinter : Window
    {
        public LabelPrinter()
        {
            InitializeComponent();
            var vm = new PriceTagViewModel();
            vm.OnClosed += OnClosed;
            DataContext = vm;
        }

        private void OnClosed(PaneViewModel vm)
        {
            Close();
        }
    }
}
