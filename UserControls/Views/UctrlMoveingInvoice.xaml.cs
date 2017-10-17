using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common;
using ES.Data.Model;
using ES.Common;
using ES.Common.Managers;
using UserControls.ViewModels.Invoices;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using TabControl = System.Windows.Controls.TabControl;
using UserControl = System.Windows.Controls.UserControl;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for UctrlMoveingInvoice.xaml
    /// </summary>
    public partial class UctrlMoveingInvoice : UserControl
    {
        #region Properties
        //private readonly EsMemberModel _member;
        private readonly EsUserModel _user;
        //private InternalWaybillInvoiceModel _viewModel;
        #endregion
        public UctrlMoveingInvoice()
        {
            InitializeComponent();
        }
        public UctrlMoveingInvoice(InternalWaybillViewModel viewModel):this()
        {
            _user = ApplicationManager.GetEsUser;
            if (_user == null)
            {
                return;
            }
            
            //if( viewModel==null || viewModel.Partner==null) _viewModel.Partner = MembersManager.GetDefaultParner(_member.Id, (long)MembersManager.PartnersTypes.Provider);
        }

        #region Methods
        private StockModel ChooseStock(long memberId)
        {
            var stocks =  StockManager.GetStocks(memberId);
            var selectedItems = new ControlPanel.Controls.SelectItems(stocks.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), false);
            if (selectedItems.ShowDialog() != true || selectedItems.SelectedItems == null) return null;
            return  stocks.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id));
        }
        #endregion
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
                        
                        break;
                    
                    case Key.F:
                        CmMiChooseProductByName_Click(null, null);
                        break;
                    case Key.N:
                        TxtCode.Focus();
                        TxtCode.SelectAll();
                        break;
                    case Key.Q:
                        CmMiChoosePartner_Click(null, null);
                        break;
                    case Key.X:
                        BtnCancel_Click(null, null);
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
                        return;
                        break;
                }
            }
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(TxtCode.Text))
            {
                BtnAddItem_Click(sender, e);
                BtnAddItem.Command.Execute(null);
            }
        }
        private void CmMiChoosePartner_Click(object sender, EventArgs e)
        {
            var mi = sender as MenuItem;
            ChooseStock(mi == null ? 0 : HgConvert.ToInt64((mi).Tag));
        }
        
        private void CmMiChooseProductByName_Click(object sender, EventArgs e)
        {
            var products = ProductsManager.GetProducts();
            var selectedItems =
                new ControlPanel.Controls.SelectItems(
                    products.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(),
                    false);
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(
                    s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
            if (product == null)
            {
                return;
            }
            //_viewModel.SetInvoiceItem(product.Code);
            TxtPrice.Focus();
            TxtPrice.SelectAll();
        }
        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            TxtCode.Focus();
            TxtCode.SelectAll();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (_viewModel.Invoice.ApproveDate != null ||
    //MessageBox.Show("Ապրանքագիրը հաստատված չէ։ Դուք իսկապե՞ս ցանկանում եք փակել այն։", "Զգուշացում",
        //MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var tabitem = this.Parent as TabItem;
                if (tabitem != null)
                {
                    var tabControl = tabitem.Parent as TabControl;
                    if (tabControl == null) return;
                    tabControl.Items.Remove(tabControl.SelectedItem);
                }
            }

        }

        private void CmMiChooseFromStock_Click(object sender, EventArgs e)
        {
            //_viewModel.FromStock = SelectStock();
        }

        private void CmMiChooseToStock_Click(object sender, EventArgs e)
        {
            //_viewModel.ToStock = SelectStock();
            
        }

        //private StockModel SelectStock()
        //{
            //var stocks = StockManager.GetStocks(ApplicationManager.Member.Id);
            //if (stocks == null) return null;
            //if (stocks.Count == 1) { return _viewModel.FromStock = stocks.FirstOrDefault(); }
            //var selectItem = new ControlPanel.Controls.SelectItems(stocks.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), false);
            //if (selectItem.ShowDialog() != true || selectItem.SelectedItems.FirstOrDefault() == null) { return null; }
            //return stocks.FirstOrDefault(s => (long)selectItem.SelectedItems.FirstOrDefault().SelectedValue == s.Id);
        //}
        #endregion

    }
}
