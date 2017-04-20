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
            DataContext = viewModel ?? new PurchaseInvoiceViewModel(user, member);
        }
        
        #region Events
        
        private void CtrlInvoice_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.D1:
                        CbAddSingle.IsChecked = !CbAddSingle.IsChecked;
                        break;
                    case Key.Enter:
                        TxtPaid.Focus();
                        TxtPaid.SelectAll();
                        break;
                    case Key.F:
                        CmMiChooseProductByName_Click(null, null);
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
        private void CmMiChooseProductByName_Click(object sender, EventArgs e)
        {
            //var products = ApplicationManager.CashManager.Products.OrderBy(s=>s.Description);
            //var selectedItems =
            //    new SelectItems(
            //        products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(),
            //        false);
            //var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
            //    ? products.FirstOrDefault(
            //        s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
            //    : null;
            //if (product == null)
            //{
            //    return;
            //}
            //_viewModel.SetInvoiceItem(product.Code);
            TxtPrice.Focus();
            TxtPrice.SelectAll();
        }
        #endregion

        }
}
