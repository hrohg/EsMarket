using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Data.Models;
using UserControls.PriceTicketControl;
using UserControls.Commands;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.PriceTicketControl.ViewModels;
using ProductModel = ES.Data.Models.ProductModel;

namespace UserControls.ViewModels.Products
{
    public class ProductViewModel : ITabItem
    {
        /// <summary>
        /// Initalize a new instance of the ProductViewModel class.
        /// </summary>

        #region Product properties
        private const string ProductProperty = "Product";
        private const string ProductsProperty = "Products";
        private const string ChangeProductActivityDescriptionProperty = "ChangeProductActivityDescription";
        private const string ProductGroupDescriptionProperty = "ProductGroupDescription";
        #endregion

        #region Private properties
        private long _memberId { get { return ApplicationManager.Instance.GetMember.Id; } }
        private long _userId { get { return ApplicationManager.GetEsUser.UserId; } }
        private ProductModel _product;
        private List<ProductModel> _products;
        private string _filterText;
        Timer _timer = null;
        private ProductModel _productOnBufer;
        private bool _isLoading;
        #endregion

        #region External properties
        public string Title { get; set; }
        public string Description
        {
            get;
            set;
        }
        public bool IsModified { get; set; }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value) { return; }
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }
        public ProductModel Product
        {
            get { return _product ?? new ProductModel(ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId, true); }
            set
            {
                if (value == _product) return;
                _product = ProductsManager.CopyProduct(value);
                OnPropertyChanged("Product");
                OnPropertyChanged(ChangeProductActivityDescriptionProperty);
                OnPropertyChanged(ProductGroupDescriptionProperty);
            }
        }
        public List<ProductModel> Products
        {
            get
            {
                return string.IsNullOrEmpty(FilterText) ? _products :
                     _products.Where(s => (s.Code + s.Barcode + s.Description + s.Price + s.CostPrice + s.Note).ToLower().Contains(FilterText)
                         || s.ProductGroups.Any(t => t.Barcode.ToLower().Contains(FilterText.ToLower()))).ToList();
            }
            set { _products = value; OnPropertyChanged(ProductsProperty); }
        }

        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value.ToLower();
                OnPropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }

        public string ChangeProductActivityDescription { get { return Product != null && Product.IsEnabled ? "Պասիվացում" : "Ակտիվացում"; } }
        public string ProductGroupDescription
        {
            get
            {
                return Product != null && Product.ProductGroups != null ? " " + Product.ProductGroups.Aggregate(string.Empty, (current, group) => current + (string.Format(" {0};", group.Barcode))).Trim() : string.Empty;
            }
            set
            {
                if (Product == null) { return; }
                var separators = new[] { @" ", ",", ";" };
                Product.ProductGroups = !string.IsNullOrEmpty(value)
                    ? value.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(s =>
                        new ProductGroupModel { ProductId = Product.Id, MemberId = (int)Product.EsMemberId, Barcode = s }).ToList()
                    : null;
            }
        }
        public bool IsGetProductFromEsServer { get; set; }
        #endregion

        #region Constructors
        public ProductViewModel()
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքների խմբագրում";
            LoadProducts();
            SetCommands();
        }
        private void SetCommands()
        {
            NewCommand = new ProductNewCommand(this);
            EditCommand = new ProductCommand(this);
            DeleteCommand = new ProductDeleteCommand(this);
            ChangeProductCodeCommand = new ProductCodeChangeCommand(this);
            PrintBarcodeCommand = new PrintBarcodeCommand(this);
            ProductCopyCommand = new ProductCopyCommand(this);
            ProductPastCommand = new ProductPastCommand(this);
            ImportProductsCommand = new RelayCommand(OnImportProducts, CanImportProducts);
        }

        private void TimerElapsed(object obj)
        {
            OnPropertyChanged(ProductsProperty);
            DisposeTimer();
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        private void LoadProducts()
        {
            new Thread(OnUpdateProducts).Start();
        }

        private void OnUpdateProducts()
        {
            IsLoading = true;
            _products = ApplicationManager.Instance.CashProvider.Products;
            var productResidue = ApplicationManager.Instance.CashProvider.ProductResidues;
            foreach (var item in _products)
            {
                var product = item;
                item.ExistingQuantity = productResidue.Any(pr => pr.ProductId == product.Id) ?
                    productResidue.Where(pr => pr.ProductId == product.Id).Select(pr => pr.Quantity).First() : 0;
            }
            Products = _products.OrderByDescending(s => s.Code).ToList();
            Product = new ProductModel(_memberId, _userId, true); OnPropertyChanged("Product");
            IsLoading = false;
        }
        private bool IsProductExist()
        {
            return (Products.SingleOrDefault(s => s.Id == Product.Id) != null);
        }

        private bool IsProductSingle()
        {
            return Products.Count(s => s.Id == Product.Id && s.Code == Product.Code) == 1;
        }
        private void OnClose(object o)
        {
            ApplicationManager.OnTabItemClose(o as TabItem);
        }

        private bool CanGetProduct(object o)
        {
            return !string.IsNullOrEmpty(o as string);
        }
        private void OnGetProduct(object o)
        {
            if (!CanGetProduct(o)) { return; }
            var product = new ProductsManager().GetProductsByCodeOrBarcode(o as string);
            Product = product ?? new ProductModel(ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId, true) { Code = o as string };
        }
        private bool CanGenerateBarcode(object o)
        {
            return string.IsNullOrEmpty(o as string) && Product != null;
        }

        private void OnGenerateBarcode(object o)
        {
            if (!CanGenerateBarcode(o)) { return; }
            if (!string.IsNullOrEmpty(Product.Barcode)) { return; }
            var nextCode = GetNextCode;
            Product.Barcode = new BarCodeGenerator(nextCode).Barcode;
            var code = !string.IsNullOrEmpty(Product.Code) ? Product.Code : Product.Barcode.Substring(7, 5);
            while (Products.FirstOrDefault(s => s.Id != Product.Id && (s.Barcode == Product.Barcode || s.Code == code)) != null)
            {
                nextCode--;
                Product.Barcode = new BarCodeGenerator(nextCode).Barcode;
                code = !string.IsNullOrEmpty(Product.Code) ? Product.Code : Product.Barcode.Substring(7, 5);
            }
            if (string.IsNullOrEmpty(Product.Code))
            {
                Product.Code = code;
            }
        }
        private bool CanImportProducts(object o)
        {
            return true;
        }
        private void OnImportProducts(object o)
        {
            IsLoading = true;
            var thread = new Thread(() => ImportProducts(o));
            thread.Start();
            IsLoading = false;
        }

        private void ImportProducts(object o)
        {
            var filePath = FileManager.OpenExcelFile();
            if (string.IsNullOrEmpty(filePath)) { return; }
            var products = ExcelImportManager.ImportProducts(filePath);
            if (products == null)
            {
                MessageManager.OnMessage("Ապրանքների բեռնումն ընդհատվել է:", MessageTypeEnum.Error);
                return;
            }
            if (ProductsManager.EditProducts(products))
            {
                ApplicationManager.Instance.CashProvider.UpdateCash();
                Products = ApplicationManager.Instance.CashProvider.Products;
                MessageManager.OnMessage("Ապրանքների բեռնումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
            }
            else
            {
                MessageManager.OnMessage("Ապրանքների խմբագրումն ընդհատվել է:", MessageTypeEnum.Warning);
            }
        }
        #endregion

        #region External methods
        /// <summary>
        /// Buffer
        /// </summary>
        /// <returns></returns>
        public bool CanPastProduct()
        {
            return _productOnBufer != null;
        }
        public bool CanCopyProduct()
        {
            return Product != null;
        }
        public void CopyProduct()
        {
            if (!CanCopyProduct()) { return; }
            _productOnBufer = new ProductModel(Product.EsMemberId, Product.LastModifierId, Product.IsEnabled)
            {
                Id = Guid.NewGuid(),
                Code = Product.Code,
                Barcode = Product.Barcode,
                Description = Product.Description,
                Mu = Product.Mu,
                Note = Product.Note,
                CostPrice = Product.CostPrice,
                DealerProfitPercent = Product.DealerProfitPercent,
                DealerPrice = Product.DealerPrice,
                DealerDiscount = Product.DealerDiscount,
                ProfitPercent = Product.ProfitPercent,
                Price = Product.Price,
                Discount = Product.Discount,
                ExpiryDays = Product.ExpiryDays,
                MinQuantity = Product.MinQuantity,
                BrandId = Product.BrandId
            };
        }
        public void PastProduct()
        {
            if (!CanPastProduct()) { return; }
            Product.Id = Guid.NewGuid();
            Product.Code = _productOnBufer.Code;
            Product.Barcode = _productOnBufer.Barcode;
            Product.Description = _productOnBufer.Description;
            Product.Mu = _productOnBufer.Mu;
            Product.Note = _productOnBufer.Note;
            Product.CostPrice = _productOnBufer.CostPrice;
            Product.DealerProfitPercent = _productOnBufer.DealerProfitPercent;
            Product.DealerPrice = _productOnBufer.DealerPrice;
            Product.DealerDiscount = _productOnBufer.DealerDiscount;
            Product.ProfitPercent = _productOnBufer.ProfitPercent;
            Product.Price = _productOnBufer.Price;
            Product.Discount = _productOnBufer.Discount;
            Product.ExpiryDays = _productOnBufer.ExpiryDays;
            Product.MinQuantity = _productOnBufer.MinQuantity;
            Product.BrandId = _productOnBufer.BrandId;
            Product.LastModifierId = _productOnBufer.LastModifierId;
            Product.EsMemberId = _productOnBufer.EsMemberId;
            Product.IsEnabled = _productOnBufer.IsEnabled;
        }

        public bool CanClean()
        {
            return true;
        }
        public bool CanEdit
        {
            get
            {
                return Product != null && !string.IsNullOrEmpty(Product.Code) && !string.IsNullOrEmpty(Product.Description)
                    && (!IsProductExist() || IsProductSingle());
            }
        }
        public void NewProduct()
        {
            Product = new ProductModel(_memberId, _userId, true); OnPropertyChanged(ProductProperty);
        }
        public void OnEditProduct()
        {
            var product = new ProductsManager().EditProduct(Product);
            if (product != null)
            {
                if (_products.FirstOrDefault(s => s.Code == product.Code) == null)
                {
                    _products.Add(product);
                    //LoadProducts();
                    MessageManager.OnMessage("Ապրանքի ավելացումն իրականացել է հաջողությամբ։", MessageTypeEnum.Information);
                }
                else
                {
                    ProductsManager.CopyProduct(Products.SingleOrDefault(s => s.Id == product.Id), product);
                    MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։", product.Code, product.Description), MessageTypeEnum.Success);
                }
                ApplicationManager.Instance.CashProvider.UpdateProducts(_products);
            }
            else
            {
                MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code, Product.Description), MessageTypeEnum.Error);
            }
        }
        public void DeleteProduct(Guid id)
        {
            ProductsManager.DeleteProduct(id, _memberId);
            _products.Remove(Product); OnPropertyChanged(ProductsProperty);
            ApplicationManager.Instance.CashProvider.UpdateProducts(_products);
            Product = new ProductModel(_memberId, _userId, true); OnPropertyChanged("Product");
        }
        public bool CanChangeProductCode()
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code)
                    && IsProductExist() && IsProductSingle();
        }
        public void ChangeProductCode()
        {
            if (!CanChangeProductCode()) return;
            var productCode = ToolsManager.GetInputText(Product.Code, "Ապրանքի կոդի փոփոխում");
            if (string.IsNullOrEmpty(productCode)) { return; }
            if (ProductsManager.ChangeProductCode(Product.Id, productCode, _memberId))
            { LoadProducts(); }

        }
        public bool OnCanPrintBarcode(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Barcode);
        }
        public void PrintBarcode(object o)
        {
            if (!OnCanPrintBarcode(o)) { return; }
            var ctrl = new UctrlBarcodeWithText(new BarcodeViewModel(Product.Code, Product.Barcode, Product.Description, Product.Price, null));
            PrintManager.PrintPreview(ctrl, "Print Barcode", HgConvert.ToBoolean(o));

            //var pb = new UiPrintPreview(ctrl);
            //if (HgConvert.ToBoolean(o))
            //{
            //    pb.Show();
            //}
            //else
            //{
            //    pb.Show();
            //    PrintManager.Print(ctrl, true);
            //    pb.Close();
            //}
        }
        public void OnPrintBarcodeLarge(object o)
        {
            if (o == null || !OnCanPrintBarcode(o)) { return; }
            var ctrl = new UctrlBarcodeLargeWithText(new BarcodeViewModel(Product.Code, Product.Barcode, Product.Description, Product.Price, null));
            PrintManager.PrintPreview(ctrl, "Print Barcode", HgConvert.ToBoolean(o));

            //var pb = new UiPrintPreview(ctrl);
            //if (HgConvert.ToBoolean(o))
            //{
            //    pb.Show();
            //}
            //else
            //{
            //    PrintManager.Print(ctrl, ApplicationManager.BarcodePrinter);
            //}
        }
        public void PrintPreviewBarcode(object o)
        {
            if (!OnCanPrintBarcode(o)) { return; }
            var ctrl = new UctrlBarcodeWithText(new BarcodeViewModel(Product.Code, Product.Barcode, Product.Description, Product.Price, null));
            PrintManager.PrintPreview(ctrl, "Print Barcode", true);
        }
        private int GetNextCode
        {
            get
            {
                return ProductsManager.GetNextProductCode(ApplicationManager.Instance.GetMember.Id); //_products.Count + 1;
                //if (!string.IsNullOrEmpty(Product.Code)) Int32.TryParse(Product.Code, out next);
                //return next;
            }
        }
        public void GetProductBy(ProductViewType? type)
        {
            Products = new ProductsManager().GetProductsBy(type ?? ProductViewType.ByActive);
        }
        public bool CanChangeProductEnabled { get { return Product != null && _products.Any(s => s.Id == Product.Id); } }
        public void ChangeProductEnabled()
        {
            if (ProductsManager.ChangeProductEnabled(Product.Id, ApplicationManager.Instance.GetMember.Id))
            {
                Product.IsEnabled = !Product.IsEnabled;
                OnPropertyChanged(ProductProperty); OnPropertyChanged(ProductsProperty);
            }
        }
        public void OnAddProductGroup(string text)
        {
            if (!string.IsNullOrEmpty(text)) { ProductGroupDescription = text; }
            else
            {
                Product.ProductGroups = null;
            }
            OnPropertyChanged(ProductGroupDescriptionProperty);
        }
        #endregion

        #region Product Commands
        public ICommand NewCommand
        {
            get;
            private set;
        }
        public ICommand EditCommand
        {
            get;
            private set;
        }
        public ICommand DeleteCommand
        {
            get;
            private set;
        }
        public ICommand GetProductCommand { get { return new RelayCommand(OnGetProduct, CanGetProduct); } }
        public ICommand GenerateBarcodeCommand { get { return new RelayCommand(OnGenerateBarcode, CanGenerateBarcode); } }
        public ICommand ChangeProductCodeCommand { get; private set; }
        public ICommand PrintBarcodeCommand { get; private set; }
        public ICommand PrintBarcodeLargeCommand { get { return new RelayCommand(OnPrintBarcodeLarge, OnCanPrintBarcode); } }
        public ICommand ProductCopyCommand { get; private set; }
        public ICommand ProductPastCommand { get; private set; }
        public ICommand ImportProductsCommand { get; private set; }
        public ICommand GetProductsCommand { get { return new GetProductsCommand(this); } }
        public ICommand ChangeProductEnabledCommand { get { return new ChangeProductEnabledCommand(this); } }
        public ICommand AddProductGroupCommand { get { return new AddProductGroupCommand(this); } }
        public ICommand CloseCommand { get { return new RelayCommand(OnClose); } }
        #endregion

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
