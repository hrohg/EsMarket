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
        #region Properties
        public const string Description = "Գնում -";
        private readonly EsMemberModel _member;
        private readonly EsUserModel _user;
        private PurchaseInvoiceViewModel _viewModel;
        #endregion
        public PurchaseUctrl()
        {
            InitializeComponent();
        }
        public PurchaseUctrl(EsUserModel user, EsMemberModel member, PurchaseInvoiceViewModel viewModel):this()
        {
            _member = member;
            _user = user;
            if (_member == null || _user == null)
            {
                return;
            }
            DataContext = _viewModel = viewModel ?? new PurchaseInvoiceViewModel(user, _member);
            //if( viewModel==null || viewModel.Partner==null) _viewModel.Partner = MembersManager.GetDefaultParner(_member.Id, (long)MembersManager.PartnersTypes.Provider);
        }
        
        #region Events
        
        private void CtrlInvoice_Loaded(object sender, RoutedEventArgs e)
        {
            TxtCode.Focus();
        }
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
                        TxtCode.Focus();
                        TxtCode.SelectAll();
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
        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            TxtCode.Focus();
            TxtCode.SelectAll();
        }
        
        #endregion

        }
}
