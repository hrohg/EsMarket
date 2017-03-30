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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.PriceTicketControl
{
    /// <summary>
    /// Interaction logic for UctrlPriceTicket.xaml
    /// </summary>
    public partial class UctrlPriceTicket : UserControl
    {
        public UctrlPriceTicket(PriceTicketViewModelBase vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
