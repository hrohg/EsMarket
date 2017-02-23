using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using UserControls.ViewModels.StockTakeings;
using UserControls.Views.CustomControls;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for SaleUserControl.xaml
    /// </summary>
    public partial class StockTakingUctrl : UserControl
    {
        #region Properties

        private StockTakeViewModel _viewModel;

        #endregion

        #region Methods

        #endregion

        public StockTakingUctrl(StockTakeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
        }

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

                        break;
                    case Key.Enter:
                        break;
                    case Key.A:
                        //_viewModel.ApproveInvoice();
                        break;
                    case Key.F:
                        CmMiChooseProductByName_Click(null, null);
                        break;
                    case Key.N:
                        TxtCode.Focus();
                        TxtCode.SelectAll();
                        break;
                    case Key.S:
                        //_viewModel.SaveInvoice();
                        break;
                    case Key.Q:
                        break;
                }
            }
        }

        private void TxtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) { return;}
            _viewModel.AddStockTakingItem();
            TxtCode.Focus();
        }

        private void TxtCode_KeyDown(object sender, KeyEventArgs e)
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
                _viewModel.GetProduct(TxtCode.Text);
                if (string.IsNullOrEmpty(TxtDescription.Text))
                {
                    return;
                }
                TxtQuantity.Focus();
                TxtQuantity.SelectAll();
            }
        }
        private void CmMiChooseProductByName_Click(object sender, EventArgs e)
        {
            TxtQuantity.Focus();
            TxtQuantity.SelectAll();
        }
       
        private void BtnViewDetile_Click(object sender, EventArgs e)
        {
            var products = new ProductsManager().GetProducts(ApplicationManager.GetEsMember.Id);
            var detile = from s in _viewModel.StockTakeItems
                         join t in products on s.ProductId equals t.Id
                         select new
                         {
                             Կոդ = s.CodeOrBarcode,
                             Անվանում = s.ProductDescription,
                             Չմ = s.Mu,
                             Քանակ = s.Quantity,
                             Առկա = s.StockTakeQuantity,
                             Հաշվեկշիռ = s.Balance,
                             Գումար = s.Balance * t.Price,
                             Ամսաթիվ = s.StockTakeDate
                         };
            var ui = new UIListView(detile);
            ui.Show();
        }
    }
}
