using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common;
using ES.Data.Model;
using ES.Common;
using UserControls.ViewModels.Invoices;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class PurchaseUctrl : UserControl
    {
        public PurchaseUctrl()
        {
            InitializeComponent();
        }
        public PurchaseUctrl(EsUserModel user, EsMemberModel member, PurchaseInvoiceViewModel viewModel):this()
        {
            DataContext = viewModel ?? new PurchaseInvoiceViewModel();
        }
        
        #region Events
        
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
                    case Key.S:
                        //_viewModel.OnSaveInvoice(null);
                        break;
                    case Key.Q:
                        
                        break;
                    case Key.X:
                        //_viewModel.OnClose(this.Parent);
                        break;
                }
            }
        }
        private void TxtPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        TxtPaid.Focus();
                        TxtPaid.SelectAll();
                        return;
                        break;
                }
            }
            if (e.Key == Key.Enter)
            {
                //_viewModel.OnAddInvoiceItem(null);
            }
        }
        private void TxtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        TxtPaid.Focus();
                        TxtPaid.SelectAll();
                        return;
                        break;
                }
            }
        }
        
        #endregion

        }
}
