using System.Windows;
using UserControls.ViewModels;

namespace ES.Shop.Config
{
    /// <summary>
    /// Interaction logic for ServerConfig.xaml
    /// </summary>
    public partial class ServerConfig : Window
    {
        public ServerConfig(ServerViewModel viewModel)
        {
            InitializeComponent();
            viewModel.ClosingEvent += (sender, e) => Close();
            DataContext = viewModel;
        }
    }
}
