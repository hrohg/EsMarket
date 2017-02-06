using System.Windows.Controls;
using UserControls.ViewModels;

namespace ES.Shop.Controls
{
    /// <summary>
    /// Interaction logic for UctrlEditServices.xaml
    /// </summary>
    public partial class UctrlEditServices : UserControl
    {
        protected long _memberId;
        public UctrlEditServices(long memberId)
        {
            InitializeComponent();
            _memberId = memberId;
            DataContext = new ServicesViewModel(_memberId);
        }
    }
}
