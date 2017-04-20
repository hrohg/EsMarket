using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Common;
using ES.Data.Model;
using ES.Common;
using UserControls.ViewModels.Invoices;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;


namespace UserControls.Views.CustomControls
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class SaleUctrl : UserControl
    {
        #region Properties
        public const string Description = "Վաճառք -";
        private readonly EsMemberModel _member;
        private readonly EsUserModel _user;
        private SaleInvoiceViewModel _viewModel {get { return (SaleInvoiceViewModel) DataContext; }}
        #endregion
        public SaleUctrl()
        {
            InitializeComponent();
        }
        public SaleUctrl(EsUserModel user, EsMemberModel member, SaleInvoiceViewModel viewModel):this()
        {
            _member = member;
            _user = user;
            DataContext = viewModel ?? new SaleInvoiceViewModel(_user, _member);
        
        //    //if(viewModel==null) return;
        //    //if (viewModel.Partner == null) _viewModel.Partner = MembersManager.GetDefaultParner(_member.Id, (long)MembersManager.PartnersTypes.Customer);
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
        private void CmMiChooseProductByName_Click(object sender, EventArgs e)
        {
            //var products = ApplicationManager.CashManager.Products;
            //if (products == null || products.Count == 0) return;
            //var selectedItems = new SelectItems(
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
        
        private void SaleUctrl_Unloaded(object sender, RoutedEventArgs e)
        {
            //todo
            //if (ApplicationManager.UserRoles.FirstOrDefault(s => s.Id == (int) ESLSettingsManager.MemberRoles.SeniorSeller) == null &&
            //    ApplicationManager.UserRoles.FirstOrDefault(s => s.Id == (int) ESLSettingsManager.MemberRoles.Manager) == null)
            //{
            //    if(_viewModel.CanSaveInvoice(null)) {_viewModel.OnSaveInvoice(null);}
            //}
        }
        
    }
}
