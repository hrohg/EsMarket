using System.Windows;
using UserControls.ViewModels;

namespace ES.Market.Edit
{
    /// <summary>
    /// Interaction logic for WinEditSubAccountinPlans.xaml
    /// </summary>
    public partial class WinEditSubAccountinPlans : Window
    {
        public WinEditSubAccountinPlans()
        {
            InitializeComponent();
            DataContext =new AccountingPlanViewModel();
        }
    }
}
