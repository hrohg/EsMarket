using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using UserControls.Commands;
using ProductModel = ES.Business.Models.ProductModel;
using ProductOrderModel = ES.Business.Models.ProductOrdersModel;

namespace UserControls.ViewModels.Products
{
    public class ProductOrderViewModel:INotifyPropertyChanged
    {
        /// <summary>
        /// Initalize a new instance of the ProductViewModel class.
        /// </summary>
        private PartnerModel _partner;
        private ProductOrderModel _productOrder = new ProductOrderModel() ;
        private ProductOrderItems _productOrderItem = new ProductOrderItems();
        private ObservableCollection<ProductOrderItemsModel> _productOrderItems = new ObservableCollection<ProductOrderItemsModel>();
        public PartnerModel Partner
        {
            get { return _partner; }
            set { _partner=value;
                OnPropertyChanged("Partner"); }
        }
        public ProductOrderModel ProductOrder
        {
            get { return _productOrder; }
            set { _productOrder = value; OnPropertyChanged("ProductOrder"); }
        }
        public ProductOrderItems ProductOrderItem { get { return _productOrderItem; } set { _productOrderItem = value; OnPropertyChanged("ProductOrderItem"); } }
        public ObservableCollection<ProductOrderItemsModel> ProductOrderItems { get { return _productOrderItems; } set
        {
            _productOrderItems = value; OnPropertyChanged("PropertyOrderItems");
        } } 
        public ProductOrderViewModel(ProductOrderModel productOrder)
        {
            _productOrder = productOrder;
            EditCommand = new ProductOrderEditCommands(this);
        }

        public void SetProductOrderItem(string code)
        {
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code, ApplicationManager.Member.Id);
            SetProductOrderItem(product);
        }
        private void SetProductOrderItem(ProductModel product)
        {
            if (product == null)
            {
                return;
            }
            ProductOrderItem.ProductOrderId = ProductOrder.Id;
            ProductOrderItem.ProductId = product.Id;
            ProductOrderItem.LastModifierId = product.LastModifierId;
            ProductOrderItem.ModifyDate = DateTime.Now;
            ProductOrderItem.Code = product.Code;
            ProductOrderItem.Description = product.Description;
            ProductOrderItem.Mu = product.Mu;
            ProductOrderItem.Quantity = 0;
            ProductOrderItem.Price = product.Price;
            ProductOrderItem.CostPrice = product.CostPrice;
            ProductOrderItem.Note = product.Note;
        }

        public bool CanAddProductOrderItem
        {
            get { return ProductOrderItem.Products != null && ProductOrderItem.Code == ProductOrderItem.Products.Code; }
        }

        public bool CanEdit
        {
            get { return true; }
        }

        public ICommand EditCommand
        {
            get; private set; }
        public void EditProduct()
        { }
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
