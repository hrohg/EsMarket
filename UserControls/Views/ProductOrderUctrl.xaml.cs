using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Data.Model;
using ES.Common;
using UserControls.ViewModels.Products;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for EsTabControl.xaml
    /// </summary>
    public partial class ProductOrderUctrl : UserControl
    {
        #region Properties
        public const string Description = "Պատվեր -";
        private readonly EsMemberModel _member;
        private readonly EsUserModel _user;
        private ProductOrderViewModel _viewModel;
        #endregion
        public ProductOrderUctrl(EsUserModel user, EsMemberModel member)
        {
            InitializeComponent();
            _member = member;
            _user = user;
            DataContext = _viewModel = new ProductOrderViewModel(new ProductOrderModel(), member);
        }
        private void CtrlProductOrder_Loaded(object sender, RoutedEventArgs e)
        {
            if (_member == null || _user == null)
            {
                return;
            }
            TxtCode.Focus();
        }
        private void CtrlProductOrder_KeyDown(object sender, KeyEventArgs e)
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
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(TxtCode.Text))
            {
                BtnAddItem_Click(sender, e);
                BtnAddItem.Command.Execute(null);
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
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(TxtCode.Text))
            {
                _viewModel.SetProductOrderItem(TxtCode.Text);
                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    return;
                }
                TxtPrice.Focus();
                TxtPrice.SelectAll();
            }
        }
        private void CmMiChooseProductByName_Click(object sender, EventArgs e)
        {
            var products = new ProductsManager().GetProducts(_member.Id);
            var selectedItems =
                new ControlPanel.Controls.SelectItems(
                    products.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = s.Description, SelectedValue = s.Id }).ToList(),
                    false);
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(
                    s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
            if (product == null)
            {
                return;
            }
            _viewModel.SetProductOrderItem(product.Code);
        }
        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            decimal count = 0;
            if (!_viewModel.CanAddProductOrderItem)
            {
                return;
            }
            if (CbAddSingle.IsChecked != null && (bool)CbAddSingle.IsChecked)
            {
                count = 1;
            }
            else
            {
                var countWindow = new ControlPanel.Controls.SelectCount(new ControlPanel.Controls.SelectCountModel(null, null, _viewModel.ProductOrderItem.Description));
                countWindow.ShowDialog();
                if (countWindow.DialogResult != null && (bool)countWindow.DialogResult)
                {
                    count = countWindow.SelectedCount;
                }
            }
            _viewModel.ProductOrderItem.Quantity = count;
            TxtCode.Focus();
            TxtCode.SelectAll();
        }
        private void CmMiChoosePartner_Click(object sender, EventArgs e)
        {
            ChoosePartner(HgConvert.ToInt64(((MenuItem)sender).Tag));
        }
        private void ChoosePartner(long partnerTypeId)
        {
            var partners = partnerTypeId!=0? PartnersManager.GetPartner(_member.Id, partnerTypeId):PartnersManager.GetPartners(_member.Id);
            var selectedItems =
                new ControlPanel.Controls.SelectItems(partners.Select(s => new ControlPanel.Controls.ItemsToSelect { DisplayName = s.FullName, SelectedValue = s.Id }).ToList(), false);
            if (selectedItems.ShowDialog() != true || selectedItems.SelectedItems == null) return;
            _viewModel.Partner = partners.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id));
            var parentTab = this.Parent as TabItem;
            if (parentTab != null) parentTab.Header = Description + TxtPartner.Text;
        }
        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            var tabitem = this.Parent as TabItem;
            if (tabitem != null)
            {
                var tabControl = tabitem.Parent as TabControl;
                if (tabControl == null) return;
                tabControl.Items.Remove(tabControl.SelectedItem);
            }
        }

        private void CmMiSave_Click(object sender, EventArgs e)
        {

        }
        private void CmMiAccept_Click(object sender, EventArgs e)
        {

        }
        private void CmMiAcceptPrint_Click(object sender, EventArgs e)
        { }

        private void CmMiClose_Click(object sender, EventArgs e)
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
}
