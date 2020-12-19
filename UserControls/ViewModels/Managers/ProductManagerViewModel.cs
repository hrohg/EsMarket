using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
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
using ES.Data.Models.EsModels;
using ES.Data.Models.Products;
using Shared.Helpers;
using UserControls.ControlPanel.Controls.ExtendedControls;
using UserControls.PriceTicketControl;
using UserControls.Helpers;
using UserControls.PriceTicketControl.Helper;
using UserControls.PriceTicketControl.ViewModels;
using ProductModel = ES.Data.Models.Products.ProductModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Managers
{
    public class ProductManagerViewModelBase : UpdatableDocumentViewModel
    {
        #region Events
        //public delegate void OnProductEditedDelegate(bool update);
        //public event OnProductEditedDelegate OnProductEdited;
        #endregion Events

        #region Product properties
        protected const string ProductProperty = "Product";
        private const string ChangeProductActivityDescriptionProperty = "ChangeProductActivityDescription";
        protected const string ProductKeysDescriptionProperty = "ProductKeysDescription";
        #endregion

        #region Private properties
        protected int MemberId { get { return ApplicationManager.Instance.GetMember.Id; } }
        protected int UserId { get { return ApplicationManager.GetEsUser.UserId; } }
        //todo
        protected ObservableCollection<ProductModel> _productItems;
        private ProductModel _product;

        private ProductModel _productOnBufer;
        private ProductViewType _viewType;
        Timer _timer;
        #endregion

        #region External properties
        public List<EsmMeasureUnitModel> MeasureOfUnits { get; private set; }
        public CollectionViewSource ProductsSource { get; private set; }
        public ProductModel Product
        {
            get { return _product ?? new ProductModel(ApplicationManager.Instance.GetMember.Id, ApplicationManager.GetEsUser.UserId, true); }
            set
            {
                if (value == _product) return;
                if (value == null) { _product = null; }
                else
                {
                    _product = value.Clone() as ProductModel;
                }
                RaisePropertyChanged(ProductProperty);
                RaisePropertyChanged(ChangeProductActivityDescriptionProperty);
                RaisePropertyChanged(ProductKeysDescriptionProperty);
                RaisePropertyChanged("EditProductStage");
            }
        }
        public IList SelectedProducts
        {
            get { return _selectedProducts; }
            set
            {
                _selectedProducts = value; OnSelectedProductsChanged();
            }
        }
        public SelectionMode SelectionMode { get { return SelectedProducts != null && SelectedProducts.Count > 1 ? SelectionMode.Multiple : SelectionMode.Single; } }
        public bool IsSingleModeSelected { get { return SelectedProducts != null && SelectedProducts.Count <= 1; } }

        #region Filter
        private string _filterText;
        private IList _selectedProducts = new List<object>();

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
        public string ProductKeysDescription
        {
            get
            {
                var productKeyDescription = "";
                if (Product.ProductKeys != null)
                    foreach (var productProductKey in Product.ProductKeys)
                    {
                        if (!string.IsNullOrEmpty(productKeyDescription)) productKeyDescription += "; ";
                        productKeyDescription += productProductKey.ProductKey;
                    }
                return productKeyDescription;
            }
            set
            {
                if (Product == null) { return; }
                var separators = new[] { @" ", ",", ";" };
                Product.ProductKeys = new List<ProductKeysModel>();
                if (!string.IsNullOrEmpty(value))
                {
                    var keys = value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var keyValue in keys)
                    {
                        if (Product.ProductKeys.Any(s => s.ProductKey == keyValue)) continue;
                        Product.ProductKeys.Add(new ProductKeysModel { Id = Guid.NewGuid(), ProductId = Product.Id, ProductKey = keyValue.Trim(), MemberId = Product.EsMemberId });
                    }
                }
            }
        }
        public bool IsGetProductFromEsServer { get; set; }
        public string EditProductStage
        {
            get
            {
                return (Product != null && _productItems.Any(s => s.Id == Product.Id)) || !IsSingleModeSelected ? "Խմբագրել" : "Ավելացնել";
                //return ProductsManager.GetProduct(Id) != null ? "Խմբագրել" : "Ավելացնել";
            }
        }
        #endregion

        #region Constructors
        public ProductManagerViewModelBase()
            : base()
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            MeasureOfUnits = ApplicationManager.CashManager.MeasureOfUnits;
            _productItems = new ObservableCollection<ProductModel>();
            _productItems.CollectionChanged += OnProductsChanged;
            ProductsSource = new CollectionViewSource { Source = _productItems };
            ProductsSource.View.Filter = ProductFilter;
            Title = "Ապրանքների խմբագրում";
            SetCommands();
            LoadProducts();
        }

        private bool ProductFilter(object obj)
        {
            var product = obj as ProductModel;
            if (product == null) return false;

            bool isVisible = true;
            switch (_viewType)
            {
                case ProductViewType.All:
                    break;
                case ProductViewType.ByActive:
                    isVisible = product.IsEnabled;
                    break;
                case ProductViewType.ByPasive:
                    isVisible = !product.IsEnabled;
                    break;
                case ProductViewType.ByEmpty:
                    isVisible = product.ExistingQuantity == 0;
                    break;
                case ProductViewType.ByBrands:
                    break;
                case ProductViewType.ByActivity:
                    break;
                case ProductViewType.WeigthsOnly:
                    isVisible = product.IsWeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return isVisible && product.HasKey(FilterText);
        }
        private void OnProductsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }
        private void OnSelectedProductsChanged()
        {
            if (!IsSingleModeSelected)
            {
                var selectedProducts = SelectedProducts.Cast<ProductModel>().ToList();
                Product = new ProductModel();

                var hcdCs = selectedProducts.First().HcdCs;
                Product.HcdCs = selectedProducts.All(s => s.HcdCs == hcdCs) ? hcdCs : null;

                var mu = selectedProducts.First().MeasureUnit;
                mu = selectedProducts.All(s => s.MeasureUnit != null && mu != null && s.MeasureUnit.Id == mu.Id) ? mu : null;
                Product.MeasureUnit = mu != null ? MeasureOfUnits.SingleOrDefault(s => s.Id == mu.Id) : null;

                //Prices
                var costPrice = selectedProducts.First().CostPrice;
                Product.CostPrice = selectedProducts.All(s => s.CostPrice == costPrice) ? costPrice : null;

                var profitPercent = selectedProducts.First().ProfitPercent;
                Product.ProfitPercent = selectedProducts.All(s => s.ProfitPercent == profitPercent) ? profitPercent : null;
                var price = selectedProducts.First().Price;
                Product.Price = selectedProducts.All(s => s.Price == price) ? price : null;
                var discount = selectedProducts.First().Discount;
                Product.Discount = selectedProducts.All(s => s.Discount == discount) ? discount : null;

                var dealerProfitPercent = selectedProducts.First().DealerProfitPercent;
                Product.DealerProfitPercent = selectedProducts.All(s => s.DealerProfitPercent == dealerProfitPercent) ? dealerProfitPercent : null;
                var dealerPrice = selectedProducts.First().DealerPrice;
                Product.Price = selectedProducts.All(s => s.HasDealerPrice && s.DealerPrice == dealerPrice) ? dealerPrice : null;
                var dealerDiscount = selectedProducts.First().DealerDiscount;
                Product.DealerDiscount = selectedProducts.All(s => s.HasDealerPrice && s.DealerDiscount == dealerDiscount) ? dealerDiscount : null;

                var minQuantity = selectedProducts.First().MinQuantity;
                Product.MinQuantity = selectedProducts.All(s => s.MinQuantity == minQuantity) ? minQuantity : null;

                var expiryDays = selectedProducts.First().ExpiryDays;
                Product.ExpiryDays = selectedProducts.All(s => s.ExpiryDays == expiryDays) ? expiryDays : null;

                var typeOfTaxes = selectedProducts.First().TypeOfTaxes;
                Product.TypeOfTaxes = selectedProducts.All(s => s.TypeOfTaxes == typeOfTaxes) ? typeOfTaxes : default(TypeOfTaxes);

            }
            else
            {


            }

            RaisePropertyChanged("SelectionMode");
            RaisePropertyChanged("IsSingleModeSelected");
            RaisePropertyChanged("Product");
        }
        private void SetCommands()
        {
            EditCommand = new RelayCommand(OnEditProducts, CanEdit);
            PrintBarcodeCommand = new RelayCommand<PrintPriceTicketEnum?>(OnPrintBarcode, CanPrintBarcode);
            ProductCopyCommand = new RelayCommand(CopyProduct, CanCopyProduct);
            ProductPastCommand = new RelayCommand(PastProduct, CanPastProduct);
            ExportProductsCommand = new RelayCommand<ExportImportEnum>(OnExportProducts, CanExportProducts);
            ExportNewProductsCommand = new RelayCommand<ExportImportEnum>(OnExportNewProducts, CanExportProducts);
            GetProductsCommand = new RelayCommand(GetProductBy);
            AddProductKeysCommand = new RelayCommand<string>(OnAddProductKeys, CanAddProductKeys);
            //PrintBarcodeCommand = new RelayCommand<PrintPriceTicketEnum?>(PrintPreviewBarcode, CanPrintBarcode);
        }
        private void TimerElapsed(object obj)
        {
            DisposeTimer();
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, (() =>
            {
                ProductsSource.View.Refresh();
            }));
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
        protected List<ProductModel> GetProducts()
        {
            ICollectionView view = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                view = ProductsSource.View;
            });
            return view.OfType<ProductModel>().ToList();
        }
        public override void OnUpdate()
        {
            IsLoading = true;
            _productOnBufer = null;

            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
            {
                CompletedUpdate();
                _productItems.Clear();
                foreach (var productModel in CashManager.Instance.GetProducts())
                {
                    _productItems.Add(productModel);
                }
                ProductsSource.View.Refresh();
                IsLoading = false;
            });
        }
        private void CompletedUpdate()
        {
            Product = _productOnBufer ?? new ProductModel(MemberId, UserId, true);
            RaisePropertyChanged("Product");
            RaisePropertyChanged("Products");
        }
        private bool IsProductExist()
        {
            return (_productItems.Any(s => s.Id == Product.Id));
        }
        protected bool IsProductSingle()
        {
            return _productItems.Count(s => s.Id == Product.Id && s.Code == Product.Code) == 1;
        }
        private bool CanGetProduct(object o)
        {
            return !string.IsNullOrEmpty(o as string);
        }
        private void OnGetProduct(object o)
        {
            if (!CanGetProduct(o)) { return; }

            ProductModel product;
            if (_productItems.Any(s => s.Code == (string)o))
            {
                product = _productItems.FirstOrDefault(s => s.Code == (string)o);
            }
            else
            {
                var products = ProductsManager.GetProductsByCodeOrBarcode(o as string);
                product = SelectItemsManager.SelectProduct(products).FirstOrDefault();
            }
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
                string.Format("{0}{1}", !ApplicationManager.Settings.SettingsContainer.MemberSettings.UseUnicCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextCode) :
                Product.Code;
            while (nextCode > 0 && _productItems.Any(s => s.Id != Product.Id && (s.Barcode == Product.Barcode || (string.IsNullOrEmpty(Product.Code) && s.Code == code))))
            {
                nextCode--;
                Product.Barcode = new BarCodeGenerator(nextCode).Barcode;
                code = string.IsNullOrEmpty(Product.Code) ? string.Format("{0}{1}", !ApplicationManager.Settings.SettingsContainer.MemberSettings.UseUnicCode ? "" : ApplicationManager.Member.Id.ToString("D2"), nextCode) :
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
            var products = days == null ? GetProducts() : GetProducts().Where(s => s.LastModifiedDate >= DateTime.Now.AddDays(-days.Value)).ToList();
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
        #endregion

        #region External methods
        protected bool OnEditProduct(ProductModel product)
        {
            product = ProductsManager.EditProduct(product);
            if (product != null)
            {
                CashManager.Instance.EditProduct(product);
                MessageManager.OnMessage(string.Format("{0} {1} (խմբագրումը հաջողվել է)", product.Code, product.Description), MessageTypeEnum.Success);
                //product.LastModifiedDate = product.LastModifiedDate;
                return true;
            }
            else
            {
                MessageManager.OnMessage(string.Format("{0} {1} (խմբագրումը ձախողվել է)", Product.Code, Product.Description), MessageTypeEnum.Error);
                return false;
            }
        }
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
            _productOnBufer = Product.Clone() as ProductModel;
        }

        protected virtual void PastProduct(object o)
        {
            if (!CanPastProduct(o))
            {
                return;
            }
            Product = _productOnBufer;
            Product.Id = Guid.NewGuid();
        }

        protected virtual bool CanClean(object o)
        {
            return true;
        }

        protected virtual bool CanEdit(object o)
        {
            return Product != null && (!string.IsNullOrEmpty(Product.Code) && !string.IsNullOrEmpty(Product.Description) && (!IsProductExist() || IsProductSingle())) || !IsSingleModeSelected;
        }

        protected virtual void OnEditProducts(object o)
        {
            IsLoading = true;
            if (!IsSingleModeSelected)
            {
                var products = SelectedProducts.Cast<ProductModel>().ToList();
                foreach (var productModel in products)
                {


                    if (Product.MeasureUnit != null) productModel.MeasureUnit = Product.MeasureUnit;
                    if (Product.HcdCs != null) productModel.HcdCs = Product.HcdCs;


                    //Prices
                    if (Product.CostPrice != null) productModel.CostPrice = Product.CostPrice;
                    if (Product.DealerDiscount != null) productModel.DealerDiscount = Product.DealerDiscount;
                    if (Product.HasDealerPrice) productModel.DealerProfitPercent = Product.DealerProfitPercent;
                    if (Product.HasDealerPrice) productModel.DealerPrice = Product.DealerPrice;
                    if (Product.Discount != null) productModel.Discount = Product.Discount;
                    if (Product.ProfitPercent != null) productModel.ProfitPercent = Product.ProfitPercent;
                    if (Product.Price != null) productModel.Price = Product.Price;

                    if (Product.MinQuantity != null) productModel.MinQuantity = Product.MinQuantity;

                    if (Product.TypeOfTaxes != (default(TypeOfTaxes))) productModel.TypeOfTaxes = Product.TypeOfTaxes;
                    if (Product.ExpiryDays != null) productModel.ExpiryDays = Product.ExpiryDays;
                    OnEditProduct(productModel);
                }
            }
            else
            {
                OnEditProduct(Product);
            }
            IsLoading = false;
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

            ProductsManager.ChangeProductCode(Product.Id, productCode, MemberId);

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
            return ProductsManager.GetNextProductCode(ApplicationManager.Instance.GetMember.Id);
            //if (!string.IsNullOrEmpty(Product.Code)) Int32.TryParse(Product.Code, out next);
            //return next;

        }

        protected string GetNextProductCode()
        {
            return string.Format("{0}{1}", !ApplicationManager.Settings.SettingsContainer.MemberSettings.UseUnicCode ? string.Empty : ApplicationManager.Member.Id.ToString("D2"), GetNextCode());
        }

        protected virtual void GetProductBy(object o)
        {
            _viewType = o is ProductViewType ? (ProductViewType)o : (ProductViewType)ProductViewType.ByActive;
            ProductsSource.View.Refresh();
        }

        protected virtual bool CanChangeProductEnabled(object o)
        {
            return Product != null && _productItems.Any(s => s.Id == Product.Id);
        }

        protected virtual void ChangeProductEnabled(object o)
        {
            if (ProductsManager.ChangeProductEnabled(Product.Id, ApplicationManager.Instance.GetMember.Id))
            {
                Product.IsEnabled = !Product.IsEnabled;
                RaisePropertyChanged(ProductProperty);
                ProductsSource.View.Refresh();
            }
        }

        protected virtual bool CanAddProductKeys(string text)
        {
            return Product != null;
        }

        protected virtual void OnAddProductKeys(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                ProductKeysDescription = text;
            }
            else
            {
                Product.ProductKeys = null;
            }
            RaisePropertyChanged(ProductKeysDescriptionProperty);
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
        public void OnUpdatedProducts(List<ProductModel> products)
        {
            IsLoading = true;
            lock (Sync)
            {
                foreach (var productModel in products)
                {
                    var exProduct = _productItems.SingleOrDefault(s => s.Id == productModel.Id);
                    if (exProduct == null)
                    {
                        _productItems.Add(productModel);
                    }
                    else
                    {
                        ProductsManager.CopyProduct(exProduct, productModel);
                    }

                }
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () => {ProductsSource.View.Refresh();});
            
            IsLoading = false;

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
        public ICommand AddProductKeysCommand { get; private set; }

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
            : base()
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

            RemoveProductCommand = new RelayCommand<List<ProductModel>>(OnRemoveProducts, CanRemoveProducts);
        }

        private bool CanRemoveProducts(List<ProductModel> products)
        {
            return products != null && products.All(s => s.ExistingQuantity == null);
        }

        private void OnRemoveProducts(List<ProductModel> products)
        {
            ProductsManager.RemoveProducts(products);
        }

        private bool IsProductExist()
        {
            return (_productItems.SingleOrDefault(s => s.Id == Product.Id) != null);
        }

        private bool IsProductSingle()
        {
            return _productItems.Count(s => s.Id == Product.Id && s.Code == Product.Code) == 1;
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
            MessageManager.OnMessage("Ապրանքների բեռնում ․․․");
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
            else
            {
                //MessageManager.OnMessage(string.Format("Բեռնվել է {0} անվանում ապրանք", products.Count));

            }
            foreach (var productModel in products)
            {
                productModel.Id = Guid.Empty;
                if (string.IsNullOrEmpty(productModel.Code)) productModel.Code = GetNextProductCode();
                productModel.LastModifiedDate = DateTime.Now;
                OnEditProduct(productModel);
            }
            MessageManager.OnMessage(string.Format("Խմբագրվել է {0} անվանում ապրանք", products.Count));
            IsLoading = false;
        }
        #endregion

        #region External methods

        public void OnNewProduct(object o)
        {
            Product = new ProductModel(MemberId, UserId, true);
        }

        protected override void OnEditProducts(object o)
        {
            IsLoading = true;
            if (IsSingleModeSelected)
            {
                if (!OnEditProduct(Product)) MessageBox.Show("խմբագրումը ձախողվել է:", "խմբագրում", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                base.OnEditProducts(o);
            }
            IsLoading = false;
        }


        public void DeleteProduct(object o)
        {

            if (ProductsManager.DeleteProduct(Product.Id, MemberId))
                ApplicationManager.Instance.CashProvider.RemoveProduct(Product);

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
        public ICommand RemoveProductCommand { get; private set; }

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
            : base()
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

        protected override void OnEditProducts(object o)
        {
            IsLoading = true;
            var product = ProductsManager.EditProduct(Product);
            {
                if (product != null)
                {
                    if (_productItems.All(s => s.Code != product.Code))
                    {

                        MessageManager.OnMessage("Ապրանքը գոյություն չունի: Ապրանքի խմբագրումն ընդհատվել է։",
                            MessageTypeEnum.Warning);
                        IsLoading = false;
                    }
                    else
                    {
                        CashManager.Instance.EditProduct(product);
                        MessageManager.OnMessage(
                            string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումն իրականացել է հաջողությամբ։",
                                product.Code, product.Description), MessageTypeEnum.Success);
                        IsLoading = false;
                    }
                }
                else
                {
                    MessageManager.OnMessage(
                        string.Format("Կոդ:{0} անվանում:{1} ապրանքի խմբագրումը ձախողվել է։", Product.Code,
                            Product.Description), MessageTypeEnum.Error);
                    IsLoading = false;
                }
            }
        }

        #endregion

        #region External methods

        #endregion

        #region Product Commands

        #endregion
    }

    public class ReeditProductsViewModel : ProductManagerViewModelBase
    {
        #region External properties
        public ObservableCollection<DataGridColumnMetedata> DataGridColumnMetadatas { get; private set; }
        #endregion External properties

        #region Constructors
        public ReeditProductsViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            DataGridColumnMetadatas = new ObservableCollection<DataGridColumnMetedata>();
            DataGridColumnMetadatas.Add(new DataGridColumnMetedata
            {
                Header = "Կոդ",
                Property = "Code"
            });
            DataGridColumnMetadatas.Add(new DataGridColumnMetedata
            {
                Header = "Բարկոդ",
                Property = "Barcode",
                IsEditable = true
            });
            DataGridColumnMetadatas.Add(new DataGridColumnMetedata
            {
                Header = "ԱՏԳԴ",
                Property = "HcdCs",
                IsEditable = true
            });
            DataGridColumnMetadatas.Add(new DataGridColumnMetedata
            {
                Header = "Անվանում",
                Property = "Description",
                IsEditable = true
            });
            DataGridColumnMetadatas.Add(new DataGridColumnMetedata
            {
                Header = "Note",
                Property = "Note",

            });
        }
        protected override bool CanEdit(object o)
        {
            return GetProducts().Any();
        }

        protected override void OnEditProducts(object o)
        {
            var products = GetProducts();
            foreach (var productModel in products)
            {
                OnEditProduct(productModel);
            }
        }

        private bool CanMnageProducts(object obj)
        {
            return GetProducts().Any();
        }

        private void OnManageProducts(object obj)
        {
            var products = GetProducts();
            var ProductKeyss = CashManager.GetEsCategories();
            foreach (var productModel in products)
            {
                if (productModel.HcdCs != null) continue;
                var keywords = productModel.Description.ToLower().Split(new[] { " ", ",", "/", "\\" }, StringSplitOptions.RemoveEmptyEntries);
                var maxCount = 0;
                EsCategoriesModel exCategoriesModel = null;
                foreach (var esCategoriesModel in ProductKeyss)
                {
                    var category = GetCompareWordsCount(esCategoriesModel, keywords);
                    var count = GetMaxCompareCount(category.Description, keywords);
                    if (count > maxCount)
                    {
                        maxCount = count;
                        exCategoriesModel = category;
                    }
                }
                if (exCategoriesModel == null) continue;
                productModel.HcdCs = exCategoriesModel.HcDcs;
                productModel.Note = exCategoriesModel.HcDcs;
            }
            ProductsSource.View.Refresh();
        }

        private EsCategoriesModel GetCompareWordsCount(EsCategoriesModel category, string[] keyWords)
        {
            if (category.Parent == null) return category;
            return (GetMaxCompareCount(category.Description, keyWords) > GetMaxCompareCount(GetCompareWordsCount(category.Parent, keyWords).Description, keyWords)) ? category : category.Parent;

        }

        private int GetMaxCompareCount(string description, string[] keyWords)
        {
            var count = 0;
            foreach (var keyWord in keyWords)
            {
                if (description.ToLower().Contains(keyWord)) count++;
            }
            return count;
        }

        #endregion Internal methods

        #region Commands
        private ICommand _manageCommand;
        public ICommand ManageCommand
        {
            get { return _manageCommand ?? (_manageCommand = new RelayCommand(OnManageProducts, CanMnageProducts)); }
        #endregion Commands
        }
    }
}
