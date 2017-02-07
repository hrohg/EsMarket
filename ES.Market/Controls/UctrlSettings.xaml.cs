using System.Windows.Controls;

namespace ES.Market.Controls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class UctrlSettings : UserControl
    {
        public UctrlSettings()
        {
            InitializeComponent();
            var parent = Parent as UserControl;
            if (parent != null)
            {
                DataContext = parent.DataContext;
            }
        }
    }
}
