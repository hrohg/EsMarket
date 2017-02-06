using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UserControls.ViewModels;

namespace ES.Shop.Edit
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
