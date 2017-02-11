using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using UserControls.Barcodes;
using UserControls.Barcodes.ViewModels;
using UserControls.Commands;
using UserControls.Helpers;

namespace UserControls.ViewModels.Managers
{
    public class ProductManagerViewModel : DocumentViewModel
    {
        #region Events
        public delegate void OnProductEditedDelegate();
        public event OnProductEditedDelegate OnProductEdited;
        #endregion Events
        #region Product properties
        private const string ProductProperty = "Product";
        private const string ProductsProperty = "Products";
        private const string ChangeProductActivityDescriptionProperty = "ChangeProductActivityDescription";
        private const string ProductGroupDescriptionProperty = "ProductGroupDescription";
        #endregion

        #region Private properties
        private long _memberId { get { return ApplicationManager.GetEsMember.Id; } }
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

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value) { return; }
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }
        public ProductModel Product
        {
            get { return _product ?? new ProductModel(ApplicationManager.GetEsMember.Id, ApplicationManager.GetEsUser.UserId, true); }
            set
            {
                if (value == _product) return;
                _product = ProductsManager.CopyProduct(value);
                RaisePropertyChanged("Product");
                RaisePropertyChanged(ChangeProductActivityDescriptionProperty);
                RaisePropertyChanged(ProductGroupDescriptionProperty);
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
            set { _products = value; RaisePropertyChanged(ProductsProperty); }
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
                RaisePropertyChanged("FilterText");
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
        public ProductManagerViewModel()
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            Title = "Ապրանքների խմբագրում";
            SetCommands();
            LoadProducts();
        }
        private void SetCommands()
        {
            NewCommand = new RelayCommand(OnNewProduct, CanClean);
            EditCommand = new RelayCommand(OnEditProduct, CanEdit);
            DeleteCommand = new RelayCommand(DeleteProduct, CanEdit);
            ChangeProductCodeCommand = new RelayCommand(ChangeProductCode, CanChangeProductCode);
            PrintBarcodeCommand = new RelayCommand(PrintPreviewBarcode, OnCanPrintBarcode);
            ProductCopyCommand = new RelayCommand(CopyProduct, CanCopyProduct);
            ProductPastCommand = new RelayCommand(PastProduct, CanPastProduct);
            ImportProductsCommand = new RelayCommand(OnImportProducts, CanImportProducts);

            GetProductsCommand = new RelayCommand(GetProductBy);
            ChangeProductEnabledCommand = new RelayCommand(ChangeProductEnabled, CanChangeProductEnabled);
            AddProductGroupCommand = new RelayCommand<string>(OnAddProductGroup, CanAddProductGroup);
        }

        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged(ProductsProperty);
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
            _productOnBufer = null;
            _products = ApplicationManager.CashManager.Products;
            var productResidue = ApplicationManager.CashManager.ProductResidues;
            foreach (var item in _products)
            {
                var product = item;
                item.ExistingQuantity = productResidue.Any(pr => pr.ProductId == product.Id) ?
                    productResidue.Where(pr => pr.ProductId == product.Id).Select(pr => pr.Quantity).First() : 0;
            }
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => CompletedUpdate(_products.OrderByDescending(s => s.Code).ToList())));
            }
            IsLoading = false;
        }

        private void CompletedUpdate(List<ProductModel> products)
        {
            Products = products;
            Product = _productOnBufer ?? new ProductModel(_memberId, _userId, true); RaisePropertyChanged("Product");
        }
        private bool IsProductExist()
        {
            return (Products.SingleOrDefault(s => s.Id == Product.Id) != null);
        }

        private bool IsProductSingle()
        {
            return Products.Count(s => s.Id == Product.Id && s.Code == Product.Code) == 1;
        }
        private bool CanGetProduct(object o)
        {
            return !string.IsNullOrEmpty(o as string);
        }
        private void OnGetProduct(object o)
        {
            if (!CanGetProduct(o)) { return; }
            var product = new ProductsManager().GetProductsByCodeOrBarcode(o as string, ApplicationManager.GetEsMember.Id);
            Product = product ?? new ProductModel(ApplicationManager.GetEsMember.Id, ApplicationManager.GetEsUser.UserId, true) { Code = o as string };
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
            Product.Barcode = new BarCodeGenerator(nextCode, _memberId).Barcode;
            var code = !string.IsNullOrEmpty(Product.Code) ? Product.Code : Product.Barcode.Substring(7, 5);
            while (Products.FirstOrDefault(s => s.Id != Product.Id && (s.Barcode == Product.Barcode || s.Code == code)) != null)
            {
                nextCode--;
                Product.Barcode = new BarCodeGenerator(nextCode, memberId: _memberId).Barcode;
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
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Ապրանքների բեռնումն ընդհատվել է:", MessageModel.MessageTypeEnum.Error));
                return;
            }
            if (ProductsManager.EditProducts(products))
            {
                ApplicationManager.CashManager.Refresh();
                Products = ApplicationManager.CashManager.Products;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Ապրանքների բեռնումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success));
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Ապրանքների խմբագրումն ընդհատվել է:", MessageModel.MessageTypeEnum.Warning));
            }
        }
        #endregion

        #region External methods
        /// <summary>
        /// Buffer
        /// </summary>
        /// <returns></returns>
        public bool CanPastProduct(object o)
        {
            return _productOnBufer != null;
        }
        public bool CanCopyProduct(object o)
        {
            return Product != null;
        }
        public void CopyProduct(object o)
        {
            if (!CanCopyProduct(o)) { return; }
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
        public void PastProduct(object o)
        {
            if (!CanPastProduct(o)) { return; }
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

        public bool CanClean(object o)
        {
            return true;
        }
        public bool CanEdit(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code) && !string.IsNullOrEmpty(Product.Description)
                    && (!IsProductExist() || IsProductSingle());
        }
        public void OnNewProduct(object o)
        {
            Product = new ProductModel(_memberId, _userId, true); RaisePropertyChanged(ProductProperty);
        }
        public void OnEditProduct(object o)
        {
            var product = new ProductsManager().EditProduct(Product);
            if (product != null)
            {
                if (_products.FirstOrDefault(s => s.Code == product.Code) == null)
                {
                    _products.Add(product);
                    //LoadProducts();
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Ապրանքի ավելացումն իրականացել է հաջողությամբ։", MessageModel.MessageTypeEnum.Information));
                }
                else
                {
                    ProductsManager.CopyProduct(Products.SingleOrDefault(s => s.Id == product.Id), product);
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։", product.Code, product.Description), MessageModel.MessageTypeEnum.Success));
                    var handler = OnProductEdited;
                    if (handler != null) handler();
                }
                ApplicationManager.CashManager.Products = _products;
            }
            else
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code, Product.Description), MessageModel.MessageTypeEnum.Error));
            }
        }
        public void DeleteProduct(object o)
        {
            ProductsManager.DeleteProduct(Product.Id, _memberId);
            _products.Remove(Product); RaisePropertyChanged(ProductsProperty);
            ApplicationManager.CashManager.Products = Products;
            Product = new ProductModel(_memberId, _userId, true); RaisePropertyChanged("Product");
        }
        public bool CanChangeProductCode(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code)
                    && IsProductExist() && IsProductSingle();
        }
        public void ChangeProductCode(object o)
        {
            if (!CanChangeProductCode(o)) return;
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
                return ProductsManager.GetNextProductCode(ApplicationManager.GetEsMember.Id); //_products.Count + 1;
                //if (!string.IsNullOrEmpty(Product.Code)) Int32.TryParse(Product.Code, out next);
                //return next;
            }
        }
        public void GetProductBy(object o)
        {
            var type = o is ProductViewType ? (ProductViewType)o : (ProductViewType?)null;
            Products = new ProductsManager().GetProductsBy(type ?? ProductViewType.ByActive, ApplicationManager.GetEsMember.Id);
        }
        public bool CanChangeProductEnabled(object o)
        { return Product != null && _products.Any(s => s.Id == Product.Id); }
        public void ChangeProductEnabled(object o)
        {
            if (ProductsManager.ChangeProductEnabled(Product.Id, ApplicationManager.GetEsMember.Id))
            {
                Product.IsEnabled = !Product.IsEnabled;
                RaisePropertyChanged(ProductProperty); RaisePropertyChanged(ProductsProperty);
            }
        }

        private bool CanAddProductGroup(string text)
        {
            return Product != null;
        }
        public void OnAddProductGroup(string text)
        {
            if (!string.IsNullOrEmpty(text)) { ProductGroupDescription = text; }
            else
            {
                Product.ProductGroups = null;
            }
            RaisePropertyChanged(ProductGroupDescriptionProperty);
        }


        public void SetProduct(ProductModel product)
        {
            if (IsLoading)
            { _productOnBufer = product; }
            else
            {
                Product = product;
            }
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
        public ICommand GetProductsCommand { get; private set; }
        public ICommand ChangeProductEnabledCommand { get; private set; }
        public ICommand AddProductGroupCommand { get; private set; }
        public ICommand CloseCommand { get { return new RelayCommand(OnClose); } }
        #endregion
    }
}
