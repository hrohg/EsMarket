using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace UserControls.Views.CustomControls
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class SaleUctrl : UserControl
    {
        public SaleUctrl()
        {
            InitializeComponent();
        }
        private void CtrlInvoice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.D1:
                        
                        break;
                    case Key.Enter:
                        TxtPaid.Focus();
                        TxtPaid.SelectAll();
                        break;
                    case Key.F:
                        
                        break;
                    case Key.N:
                        break;
                        //todo
                    //case Key.S:
                    //    _viewModel.OnSaveInvoice(null);
                    //    break;
                    case Key.Q:
                        
                        break;
                    case Key.X:
                        //_viewModel.OnClose(this.Parent);
                        break;
                }
            }
        }
    }
}
