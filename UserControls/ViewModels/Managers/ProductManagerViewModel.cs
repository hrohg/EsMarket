using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CashReg.Helper;
using CashReg.Interfaces;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.ViewModels.Base;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using Shared.Helpers;
using UserControls.PriceTicketControl;
using UserControls.Helpers;
using UserControls.PriceTicketControl.Helper;
using UserControls.PriceTicketControl.ViewModels;

namespace UserControls.ViewModels.Managers
{
    public class ProductManagerViewModelBase : UpdatableDocumentViewModel
    {
        #region Events
        public delegate void OnProductEditedDelegate(bool update);
        public event OnProductEditedDelegate OnProductEdited;
        #endregion Events

        #region Product properties
        protected const string ProductProperty = "Product";
        protected const string ProductsProperty = "Products";
        private const string ChangeProductActivityDescriptionProperty = "ChangeProductActivityDescription";
        protected const string ProductGroupDescriptionProperty = "ProductGroupDescription";
        #endregion

        #region Private properties
        protected long MemberId { get { return ApplicationManager.Instance.GetMember.Id; } }
        protected long UserId { get { return ApplicationManager.GetEsUser.UserId; } }
        private ProductModel _product;
        protected List<ProductModel> _products;
        Timer _timer;
        private ProductModel _productOnBufer;

        #endregion

        #region External properties
        public ProductModel Product
        {
            get { return _product ?? new ProductModel(ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId, true); }
            set
            {
                if (value == _product) return;
                _product = ProductsManager.CopyProduct(value);
                RaisePropertyChanged(ProductProperty);
                RaisePropertyChanged(ChangeProductActivityDescriptionProperty);
                RaisePropertyChanged(ProductGroupDescriptionProperty);
                RaisePropertyChanged("EditProductStage");
            }
        }
        public List<ProductModel> Products
        {
            get
            {
                return string.IsNullOrEmpty(FilterText) ? _products :
                     _products.Where(s => (s.Code + s.Barcode + s.Description + s.Price + s.CostPrice + s.Note).ToLower().Contains(FilterText.ToLower())
                         || s.ProductGroups.Any(t => t.Barcode.ToLower().Contains(FilterText.ToLower()))).ToList();
            }
            set { _products = value; RaisePropertyChanged(ProductsProperty); }
        }
        #region Filter
        private string _filterText;
        public string FilterText
        {
            get
            {
                return _filterText;
            }
            set
            {
                _filterText = value;
                RaisePropertyChanged("FilterText");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        #endregion Filter
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
        public string EditProductStage
        {
            get
            {
                return Product != null && Products.Any(s => s.Id == Product.Id) ? "Խմբագրել" : "Ավելացնել";
                //return ProductsManager.GetProduct(Id) != null ? "Խմբագրել" : "Ավելացնել";
            }
        }
        #endregion

        #region Constructors
        public ProductManagerViewModelBase()
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
            EditCommand = new RelayCommand(OnEditProduct, CanEdit);
            PrintBarcodeCommand = new RelayCommand<PrintPriceTicketEnum?>(OnPrintBarcode, CanPrintBarcode);
            ProductCopyCommand = new RelayCommand(CopyProduct, CanCopyProduct);
            ProductPastCommand = new RelayCommand(PastProduct, CanPastProduct);
            ExportProductsCommand = new RelayCommand<ExportImportEnum>(OnExportProducts, CanExportProducts);
            ExportNewProductsCommand = new RelayCommand<ExportImportEnum>(OnExportNewProducts, CanExportProducts);
            GetProductsCommand = new RelayCommand(GetProductBy);
            AddProductGroupCommand = new RelayCommand<string>(OnAddProductGroup, CanAddProductGroup);
            //PrintBarcodeCommand = new RelayCommand<PrintPriceTicketEnum?>(PrintPreviewBarcode, CanPrintBarcode);
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
        protected void LoadProducts()
        {
            new Thread(OnUpdate).Start();
        }

        public override void OnUpdate()
        {
            IsLoading = true;
            _productOnBufer = null;
            _products = ApplicationManager.Instance.CashProvider.Products;
            var productResidue = ApplicationManager.Instance.CashProvider.ProductResidues;
            foreach (var item in _products)
            {
                var product = item;
                item.ExistingQuantity = productResidue.Any(pr => pr.ProductId == product.Id) ?
                    productResidue.Where(pr => pr.ProductId == product.Id).Select(pr => pr.Quantity).First() : 0;
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                CompletedUpdate();
                RaisePropertyChanged("Products");
            }));

            IsLoading = false;
        }

        private void CompletedUpdate()
        {
            Product = _productOnBufer ?? new ProductModel(MemberId, UserId, true);
            RaisePropertyChanged("Product");
            RaisePropertyChanged("Products");
        }
        private bool IsProductExist()
        {
            return (Products.SingleOrDefault(s => s.Id == Product.Id) != null);
        }

        protected bool IsProductSingle()
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
            var nextCode = GetNextCode();
            Product.Barcode = new BarCodeGenerator(nextCode).Barcode;
            var code = string.IsNullOrEmpty(Product.Code) ?
                string.Format("{0}{1}", ApplicationManager.Settings.SettingsContainer.MemberSettings.UseShortCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextCode) :
                Product.Code;
            while (Products.Any(s => s.Id != Product.Id && (s.Barcode == Product.Barcode || (string.IsNullOrEmpty(Product.Code) && s.Code == code))))
            {
                nextCode--;
                Product.Barcode = new BarCodeGenerator(nextCode).Barcode;
                code = string.IsNullOrEmpty(Product.Code) ? string.Format("{0}{1}", ApplicationManager.Settings.SettingsContainer.MemberSettings.UseShortCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextCode) :
                    Product.Code;
            }
            if (string.IsNullOrEmpty(Product.Code)) Product.Code = code;
        }
        private bool CanExportProducts(ExportImportEnum o)
        {
            return true;
        }
        private void OnExportProducts(ExportImportEnum o)
        {
            IsLoading = true;
            var thread = new Thread(() => ExportProducts(o));
            thread.Start();
            IsLoading = false;
        }
        private void OnExportNewProducts(ExportImportEnum o)
        {
            IsLoading = true;
            var win = new SelectCount(new SelectCountModel(1, "Խմբագրված օրեր"), Visibility.Collapsed);
            win.ShowDialog();
            if (win.DialogResult == null || !(bool)win.DialogResult) return;
            var count = (int)win.SelectedCount;
            var thread = new Thread(() => ExportProducts(o, count));
            thread.Start();
            IsLoading = false;
        }
        private void ExportProducts(ExportImportEnum exportToFile, int? days = null)
        {
            var products = days == null ? Products : Products.Where(s => s.LastModifiedDate >= DateTime.Now.AddDays(-days.Value)).ToList();
            bool result;
            switch (exportToFile)
            {
                case ExportImportEnum.Xml:
                    var filePath = FileManager.SaveFile("Export to xml file", "Xml file | *.xml");
                    if (string.IsNullOrEmpty(filePath))
                    {
                        return;
                    }
                    //result = XmlManager.Save(Products.Select(s => s.ToEsGoods()).ToList(), filePath);
                    result = XmlManager.Save(products, filePath);
                    break;
                case ExportImportEnum.Excel:
                    result = ExcelExportManager.ExportProducts(products);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportToFile", exportToFile, null);
            }
            if (!result)
            {
                MessageManager.OnMessage("Ապրանքների արտահանումը ձախոսվել է:", MessageTypeEnum.Warning);
                return;
            }
        }

        protected void NotifyEditedProducts(bool update)
        {
            var handler = OnProductEdited;
            if (handler != null) handler(update);
        }
        #endregion

        #region External methods

        /// <summary>
        /// Buffer
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanPastProduct(object o)
        {
            return _productOnBufer != null;
        }

        protected virtual bool CanCopyProduct(object o)
        {
            return Product != null;
        }

        protected virtual void CopyProduct(object o)
        {
            if (!CanCopyProduct(o))
            {
                return;
            }
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

        protected virtual void PastProduct(object o)
        {
            if (!CanPastProduct(o))
            {
                return;
            }
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

        protected virtual bool CanClean(object o)
        {
            return true;
        }

        protected virtual bool CanEdit(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code) && !string.IsNullOrEmpty(Product.Description) && (!IsProductExist() || IsProductSingle());
        }

        protected virtual void OnEditProduct(object o)
        {
            IsLoading = true;
            var product = new ProductsManager().EditProduct(Product);
            if (product != null)
            {
                if (_products.FirstOrDefault(s => s.Code == product.Code) == null)
                {
                    _products.Add(product);
                    MessageManager.OnMessage("Ապրանքի ավելացումն իրականացել է հաջողությամբ։");
                }
                else
                {
                    ProductsManager.CopyProduct(Products.SingleOrDefault(s => s.Id == product.Id), product);
                    MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։", product.Code, product.Description), MessageTypeEnum.Success);
                }
            }
            else
            {
                MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code, Product.Description), MessageTypeEnum.Error);
                IsLoading = false;
                return;
            }
            IsLoading = false;
            NotifyEditedProducts(false);
        }

        protected virtual bool CanChangeProductCode(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code) && IsProductExist() && IsProductSingle();
        }

        protected virtual void ChangeProductCode(object o)
        {
            if (!CanChangeProductCode(o)) return;
            var productCode = ToolsManager.GetInputText(Product.Code, "Ապրանքի կոդի փոփոխում");
            if (string.IsNullOrEmpty(productCode))
            {
                return;
            }
            if (ProductsManager.ChangeProductCode(Product.Id, productCode, MemberId))
            {
                LoadProducts();
            }
        }
        protected virtual bool CanPrintBarcode(PrintPriceTicketEnum? printPriceTicketEnum)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Barcode) && printPriceTicketEnum != null;
        }
        protected virtual void OnPrintBarcode(PrintPriceTicketEnum? printPriceTicketEnum)
        {
            PriceTicketManager.PrintPriceTicket(printPriceTicketEnum, Product);
        }

        protected virtual void PrintPreviewBarcode(PrintPriceTicketEnum? printPriceTicketEnum)
        {
            if (!CanPrintBarcode(printPriceTicketEnum))
            {
                return;
            }
            var ctrl = new UctrlBarcodeWithText(new BarcodeViewModel(Product.Code, Product.Barcode, Product.Description, Product.Price, null));
            PrintManager.PrintPreview(ctrl, "Print Barcode", true);
        }

        private int GetNextCode()
        {
            return ProductsManager.GetNextProductCode(ApplicationManager.Instance.GetMember.Id); //_products.Count + 1;
            //if (!string.IsNullOrEmpty(Product.Code)) Int32.TryParse(Product.Code, out next);
            //return next;

        }

        protected virtual void GetProductBy(object o)
        {
            var type = o is ProductViewType ? (ProductViewType)o : (ProductViewType?)null;
            Products = new ProductsManager().GetProductsBy(type ?? ProductViewType.ByActive);
        }

        protected virtual bool CanChangeProductEnabled(object o)
        {
            return Product != null && _products.Any(s => s.Id == Product.Id);
        }

        protected virtual void ChangeProductEnabled(object o)
        {
            if (ProductsManager.ChangeProductEnabled(Product.Id, ApplicationManager.Instance.GetMember.Id))
            {
                Product.IsEnabled = !Product.IsEnabled;
                RaisePropertyChanged(ProductProperty);
                RaisePropertyChanged(ProductsProperty);
            }
        }

        protected virtual bool CanAddProductGroup(string text)
        {
            return Product != null;
        }

        protected virtual void OnAddProductGroup(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                ProductGroupDescription = text;
            }
            else
            {
                Product.ProductGroups = null;
            }
            RaisePropertyChanged(ProductGroupDescriptionProperty);
        }

        public virtual void SetProduct(ProductModel product)
        {
            if (IsLoading)
            {
                _productOnBufer = product;
            }
            else
            {
                Product = product;
            }
        }
        public virtual void SetProductCategory(EsCategoriesModel category)
        {
            if (Product != null)
            {
                Product.HcdCs = category.HcDcs;
                RaisePropertyChanged("Product");
            }
        }
        #endregion

        #region Product Commands

        public ICommand EditCommand { get; private set; }

        public ICommand GetProductCommand
        {
            get { return new RelayCommand(OnGetProduct, CanGetProduct); }
        }

        public ICommand GenerateBarcodeCommand
        {
            get { return new RelayCommand(OnGenerateBarcode, CanGenerateBarcode); }
        }


        public ICommand PrintBarcodeCommand { get; private set; }
        public ICommand ProductCopyCommand { get; private set; }
        public ICommand ProductPastCommand { get; private set; }
        public ICommand ExportProductsCommand { get; private set; }
        public ICommand ExportNewProductsCommand { get; private set; }
        public ICommand GetProductsCommand { get; private set; }
        public ICommand AddProductGroupCommand { get; private set; }

        #endregion
    }
    public class ProductManagerViewModel : ProductManagerViewModelBase
    {
        #region Events

        #endregion Events

        #region Product properties

        #endregion

        #region Private properties

        #endregion

        #region External properties
        public string ChangeProductActivityDescription { get { return Product != null && Product.IsEnabled ? "Պասիվացում" : "Ակտիվացում"; } }
        public List<IEcrDepartment> EcrDepartments { get; private set; }
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
            Title = "Ապրանքների խմբագրում, ավելացում";
            EcrDepartments = CashReg.Helper.Enumerations.GetEcrDepartments();
            EcrDepartments.Insert(0, new Department { Id = -1, Name = "Ընտրել հարկման տեսակը", Type = -1 });
            SetCommands();
        }
        private void SetCommands()
        {
            NewCommand = new RelayCommand(OnNewProduct, CanClean);
            DeleteCommand = new RelayCommand(DeleteProduct, CanEdit);
            ChangeProductCodeCommand = new RelayCommand(ChangeProductCode, CanChangeProductCode);
            ImportProductsCommand = new RelayCommand<ExportImportEnum>(OnImportProducts, CanImportProducts);
            ChangeProductEnabledCommand = new RelayCommand(ChangeProductEnabled, CanChangeProductEnabled);
        }
        private bool IsProductExist()
        {
            return (Products.SingleOrDefault(s => s.Id == Product.Id) != null);
        }

        private bool IsProductSingle()
        {
            return Products.Count(s => s.Id == Product.Id && s.Code == Product.Code) == 1;
        }

        private bool CanImportProducts(ExportImportEnum o)
        {
            return true;
        }
        private void OnImportProducts(ExportImportEnum o)
        {
            IsLoading = true;
            var thread = new Thread(() => ImportProductsAsync(o));
            thread.Start();
        }
        private void ImportProductsAsync(ExportImportEnum importToFile)
        {
            string filePath = null;
            List<ProductModel> products = null;
            switch (importToFile)
            {
                case ExportImportEnum.Xml:
                    filePath = FileManager.OpenFile("Open xml file", "Xml file | *.xml");
                    if (string.IsNullOrEmpty(filePath))
                    {
                        IsLoading = false;
                        return;
                    }
                    //var goods = XmlManager.Read<List<EsGood>>(filePath);
                    //products = goods.Select(ProductsManager.Convert).ToList();
                    products = XmlManager.Read<List<ProductModel>>(filePath);
                    foreach (var productModel in products)
                    {
                        productModel.LastModifierId = ApplicationManager.GetEsUser.UserId;
                        //productModel.LastModifiedDate = DateTime.Now;
                        productModel.EsMemberId = ApplicationManager.Member.Id;
                    }
                    break;
                case ExportImportEnum.Excel:
                    filePath = FileManager.OpenFile();
                    if (string.IsNullOrEmpty(filePath))
                    {
                        IsLoading = false;
                        return;
                    }
                    products = ExcelImportManager.ImportProducts(filePath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("importToFile", importToFile, null);
            }
            if (products == null)
            {
                MessageManager.OnMessage("Ապրանքների բեռնումն ձախողվել է:", MessageTypeEnum.Error);
                IsLoading = false;
                return;
            }
            foreach (var productModel in products)
            {
                productModel.Id = Guid.Empty;
                Product = productModel;
                OnEditProduct(null);
            }
            //if (ProductsManager.EditProducts(products))
            //{
            //    ApplicationManager.Instance.CashProvider.UpdateCash();
            //    if (Application.Current != null)
            //    {
            //        Application.Current.Dispatcher.BeginInvoke(new Action(() => { Products = ApplicationManager.Instance.CashProvider.Products; }));

            //    }
            //    MessageManager.OnMessage("Ապրանքների բեռնումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
            //    //NotifyEditedProducts();
            //}
            //else
            //{
            //    MessageManager.OnMessage("Ապրանքների խմբագրումն ընդհատվել է:", MessageTypeEnum.Warning);
            //}
            IsLoading = false;
        }
        #endregion

        #region External methods

        public void OnNewProduct(object o)
        {
            Product = new ProductModel(MemberId, UserId, true);
        }

        protected override void OnEditProduct(object o)
        {
            IsLoading = true;
            var product = new ProductsManager().EditProduct(Product);
            if (product != null)
            {
                if (_products.FirstOrDefault(s => s.Code == product.Code) == null)
                {
                    _products.Add(product);
                    MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի ավելացումն իրականացել է հաջողությամբ։", product.Code, product.Description));
                }
                else
                {
                    ProductsManager.CopyProduct(Products.SingleOrDefault(s => s.Id == product.Id), product);
                    MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։", product.Code, product.Description), MessageTypeEnum.Success);
                }
                Product.LastModifiedDate = product.LastModifiedDate;
            }
            else
            {
                MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code, Product.Description), MessageTypeEnum.Error);
                IsLoading = false;
                return;
            }
            IsLoading = false;
            NotifyEditedProducts(false);
        }

        public void DeleteProduct(object o)
        {
            ProductsManager.DeleteProduct(Product.Id, MemberId);
            _products.Remove(Product);
            RaisePropertyChanged(ProductsProperty);
            ApplicationManager.Instance.CashProvider.UpdateProducts(_products);
            Product = new ProductModel(MemberId, UserId, true);
            RaisePropertyChanged("Product");
        }
        #endregion

        #region Product Commands

        public ICommand NewCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand ImportProductsCommand { get; private set; }
        public ICommand ChangeProductEnabledCommand { get; private set; }
        public ICommand ChangeProductCodeCommand { get; private set; }

        #endregion
    }

    public class CorrectProductsViewModel : ProductManagerViewModelBase
    {
        #region Events

        #endregion Events

        #region Product properties

        #endregion

        #region Private properties

        #endregion

        #region External properties

        #endregion

        #region Constructors
        public CorrectProductsViewModel()
        {
            Initialize();
        }
        #endregion

        #region Internal methods
        private void Initialize()
        {
            Title = "Ապրանքների խմբագրում";
        }
        protected override bool CanEdit(object o)
        {
            return Product != null && !string.IsNullOrEmpty(Product.Code) && !string.IsNullOrEmpty(Product.Description) && IsProductSingle();
        }
        protected override void OnEditProduct(object o)
        {
            IsLoading = true;
            var product = new ProductsManager().EditProduct(Product);
            if (product != null)
            {
                if (_products.FirstOrDefault(s => s.Code == product.Code) == null)
                {

                    MessageManager.OnMessage("Ապրանքը գոյություն չունի: Ապրանքի խմբագրումն ընդհատվել է։", MessageTypeEnum.Warning);
                    IsLoading = false;
                }
                else
                {
                    ProductsManager.CopyProduct(Products.SingleOrDefault(s => s.Id == product.Id), product);
                    MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։", product.Code, product.Description), MessageTypeEnum.Success);
                    IsLoading = false;
                    NotifyEditedProducts(false);
                }
            }
            else
            {
                MessageManager.OnMessage(string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code, Product.Description), MessageTypeEnum.Error);
                IsLoading = false;
            }
        }
        #endregion

        #region External methods

        #endregion

        #region Product Commands

        #endregion
    }
}
