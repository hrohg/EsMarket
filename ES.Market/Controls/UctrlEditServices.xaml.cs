using System.Windows.Controls;
using UserControls.ViewModels;

namespace ES.Market.Controls
{
    /// <summary>
    /// Interaction logic for UctrlEditServices.xaml
    /// </summary>
    public partial class UctrlEditServices : UserControl
    {
        public UctrlEditServices()
        {
            InitializeComponent();
            DataContext = new ServicesViewModel();
        }
    }
}
