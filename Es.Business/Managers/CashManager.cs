using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Data.Models;
using ES.Data.Models.EsModels;
using ES.DataAccess.Models;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace ES.Business.Managers
{
    public class CashManager
    {
        #region Private properties
        private EsMemberModel Member { get { return ApplicationManager.Instance.GetMember; } }
        private readonly object _syncStocks = new object();
        private readonly object _syncPartners = new object();
        private readonly object _syncProducts = new object();
        private readonly object _syncProductItems = new object();
        //private readonly object _sync = new object();
        private bool IsLocalMode
        {
            get { return ApplicationManager.Settings.IsOfflineMode; }
        }

        private List<StockModel> _stocks;
        private List<EsDefaults> _esDefaults;

        private bool _isProductItemsUpdating;
        private bool _isPartnersUpdating;
        private bool _isStocksUpdating;

        private static CashManager _instance;
        private List<ProductModel> _products;
        private LicensePlansEnum? _license;
        #endregion

        #region Public properties
        public bool IsUpdateing { get; private set; }
        #region Partners
        private List<EsPartnersTypes> _partnersTypes;

        public static List<EsPartnersTypes> PartnersTypes
        {
            get { return Instance._partnersTypes ?? (Instance._partnersTypes = PartnersManager.GetPartnersTypes()); }
        }
        private List<PartnerModel> _partners;

        public List<PartnerModel> GetPartners
        {
            get
            {
                lock (_syncPartners)
                {
                    return _partners;
                }
            }
        }

        #endregion Partners

        #region Cash desks

        private List<CashDesk> _cashDesks;

        public List<CashDesk> GetCashDesks()
        {
            var cashDesk = GetCashDesk;
            cashDesk.AddRange(GetBankAccounts);
            return cashDesk;
        }

        public List<CashDesk> GetCashDesk
        {
            get
            {
                if (_cashDesks == null) _cashDesks = CashDeskManager.GetCashDesks();
                return _cashDesks.Where(s => s.IsCash).ToList();
            }
        }

        public List<CashDesk> GetBankAccounts
        {
            get
            {
                if (_cashDesks == null) _cashDesks = CashDeskManager.GetCashDesks();
                return _cashDesks.Where(s => !s.IsCash).ToList();
            }
        }

        #endregion Cash desks

        #region Products
        public List<EsmMeasureUnitModel> MeasureOfUnits { get; private set; }
        private List<ProductItemModel> _productItems;
        private List<ProductResidue> _productResidues;
        public List<ProductModel> ExistingProducts
        {
            get
            {
                lock (_syncProductItems)
                {
                    if (!IsLocalMode || _productItems == null) _productItems = ProductsManager.GetProductItems();
                    return _productItems != null ? _productItems.Select(s => s.Product).ToList() : new List<ProductModel>();
                }
            }
            //set { _products = value; }
        }
        public List<ProductItemModel> ProductItems
        {
            get
            {
                lock (_syncProductItems)
                {
                    if (!IsLocalMode || _productItems == null)
                        _productItems = ProductsManager.GetProductItems();
                    return _productItems;
                }
            }
        }
        public List<ProductResidue> ProductResidues
        {
            get
            {
                if (!IsLocalMode || _productResidues == null) _productResidues = ProductsManager.GeProductResidues(Member.Id);
                return _productResidues;
            }
            set { _productResidues = value; }
        }

        private bool _isProductsUpdating;
        public delegate void ProductsUpdateingEvent();
        public event ProductsUpdateingEvent ProductsUpdateing;
        public delegate void ProductUpdatedDelegate();
        public event ProductUpdatedDelegate ProductUpdated;
        public delegate void ProductsChangedDelegate(List<ProductModel> products);
        public event ProductsChangedDelegate ProductsChanged;
        public List<ProductModel> GetProducts()
        {
            lock (_syncProducts)
            {
                return _products;
            }
        }
        public static ProductModel GetProduct(Guid? productId)
        {
            return Instance.GetProducts().SingleOrDefault(s => s.Id == productId);
        }
        #endregion Products

        #region Users

        private List<EsUserModel> _esUsers;
        public List<EsUserModel> GetUsers
        {
            get { return _esUsers ?? (_esUsers = UsersManager.GetEsUsers(Member.Id)); }
        }
        public EsUserModel GetUser(int id)
        {
            return GetUsers.SingleOrDefault(s => s.UserId == id);
        }
        #endregion Users
        public List<StockModel> GetStocks
        {
            get
            {
                lock (_syncStocks)
                {
                    return _stocks;
                }
            }
        }
        public List<EsDefaults> EsDefaults
        {
            get { return _esDefaults ?? (_esDefaults = DefaultsManager.GetDefaults()); }
            set { _esDefaults = value; }
        }
        public EsDefaults GetEsDefaults(string control)
        {
            return EsDefaults.FirstOrDefault(s => s.Control == control);
        }
        public LicensePlansEnum License
        {
            get
            {
                if (!_license.HasValue) _license = EsMarketManager.GetLicense(ApplicationManager.MemberId);
                return (LicensePlansEnum)_license;
            }
        }

        public static CashManager Instance { get { return _instance ?? (_instance = new CashManager()); } }
        #endregion

        #region Constructors
        private CashManager()
        {
            Initialize();
            UpdateCashAsync();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            _products = new List<ProductModel>();
            MeasureOfUnits = ProductsManager.GetMeasureOfUnits();
        }
        private void OnProductsUpdated()
        {
            _isProductsUpdating = false;
            var updatedHandler = ProductUpdated;
            if (updatedHandler != null) updatedHandler();
            ApplicationManager.Instance.SetProgressValue("products", 100);
            OnUpdateCompleted("products");
        }
        private void UpdatePartners()
        {
            lock (_syncPartners)
            {
                _isPartnersUpdating = true;
                _partners = PartnersManager.GetPartners();
                _isPartnersUpdating = false;
            }
            ApplicationManager.Instance.SetProgressValue("partners", 100);
            OnUpdateCompleted("partners");
        }

        private void SetStocks()
        {
            _isStocksUpdating = true;
            lock (_syncStocks)
            {
                _stocks = StockManager.GetStocks();
            }
            _isStocksUpdating = false;
            ApplicationManager.Instance.SetProgressValue("stocks", 100);
            OnUpdateCompleted("stocks");
        }

        private void UpdateProductItems()
        {
            _isProductItemsUpdating = true;
            lock (_syncProductItems)
            {
                _productItems = ProductsManager.GetProductItems();
            }
            _isProductItemsUpdating = false;
            ApplicationManager.Instance.SetProgressValue("productItems", 100);
            OnUpdateCompleted("productItems");
        }

        private void OnBeginCashUpdateing()
        {
            IsUpdateing = true;
            var handler = BeginCashUpdateing;
            if (handler != null)
            {
                handler("products");
                handler("partners");
                handler("stocks");
            }
        }
        private void OnUpdateCompleted(string key)
        {
            if (!_isPartnersUpdating && !_isProductItemsUpdating && !_isProductsUpdating && !_isStocksUpdating)
            {
                IsUpdateing = false;
            }
            DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, delegate
            {
                var handler = CashUpdated;
                if (handler != null) handler(key);
            });

        }
        private void OnProductsChanged(List<ProductModel> products)
        {
            var handler = ProductsChanged;
            if (handler != null) handler(products);
        }
        #endregion

        #region External methods
        public static PartnerModel GetPartner(Guid? id)
        {
            return id == null ? null : Instance.GetPartners.SingleOrDefault(p => p.Id == id);
        }
        public void UpdateCashAsync()
        {
            if (IsUpdateing) return;
            OnBeginCashUpdateing();
            UpdateStocksAsync();
            UpdatePartnersAsync();
            UpdateProductsAsync();
            //UpdateProductItemsAsync();
        }
        public void UpdateProductsAsync()
        {
            new Thread(UpdateProducts).Start();
        }
        private void UpdateProducts()
        {
            ApplicationManager.Instance.SetProgressValue("products");
            _isProductsUpdating = true;
            var updateingHandler = ProductsUpdateing;
            if (updateingHandler != null) updateingHandler();
            List<Products> products = ProductsManager.GetActiveProducts();
            List<ProductModel> productModels = new List<ProductModel>();
            int productsCount = products.Count;
            for (int i = 0; i < productsCount; i++)
            {
                ApplicationManager.Instance.SetProgressValue("products", 100 * i / productsCount);
                ProductModel product = ProductsManager.Convert(products[i]);
                productModels.Add(product);
            }
            UpdateProducts(productModels);
            ApplicationManager.Instance.RemoveProgressValue("products");
        }
        public void UpdateProducts(ProductModel product)
        {
            var exProduct = _products.SingleOrDefault(s => s.Id == product.Id);
            if (exProduct != null) _products.Remove(exProduct);
            _products.Add(product);
            OnProdutsCollectionChanged(null, null);
        }

        public void UpdateProducts(List<ProductModel> products)
        {
            lock (_syncProducts)
            {
                // TODO: rmove clear from update method 
                _products.Clear();
                _products.AddRange(products);
                products = _products;
            }
            ApplicationManager.Instance.SetProgressValue("productExistingCount");
            new Thread(() =>
            {
                var productResidue = ProductResidues;
                int productsCount = products.Count;
                for (int i = 0; i < productsCount; i++)
                {
                    ApplicationManager.Instance.SetProgressValue("productExistingCount", 100 * i / productsCount);
                    ProductModel product = products[i];
                    product.ExistingQuantity = productResidue.Any(pr => pr.ProductId == product.Id)
                        ? productResidue.Where(pr => pr.ProductId == product.Id).Select(pr => pr.Quantity).First()
                        : 0;
                }
                ApplicationManager.Instance.RemoveProgressValue("productExistingCount");
            }).Start();
            OnProdutsCollectionChanged(null, null);
        }

        private void OnProdutsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnProductsUpdated();
        }

        private void UpdateProductItemsAsync()
        {
            lock (_syncProductItems)
            {
                var thread = new Thread(UpdateProductItems);
                thread.Start();
            }
        }
        public void UpdatePartnersAsync(bool isAsync = true)
        {
            if (_isPartnersUpdating) return;
            ApplicationManager.Instance.SetProgressValue("partners", 0);
            if (!isAsync)
            {
                UpdatePartners();
            }
            else
            {
                new Thread(UpdatePartners).Start();
            }
        }
        public void UpdateStocksAsync()
        {
            ApplicationManager.Instance.SetProgressValue("stocks", 0);
            new Thread(SetStocks).Start();
        }
        public void UpdateDefaults()
        {
            _esDefaults = DefaultsManager.GetDefaults();
        }
        public void EditProduct(ProductModel product)
        {
            var exProduct = GetProducts().SingleOrDefault(s => s.Id == product.Id);
            if (exProduct != null)
            {
                if (product.IsEnabled)
                {
                    ProductsManager.CopyProduct(exProduct, product);
                }
                else
                {
                    lock (_syncProducts)
                    {
                        _products.Remove(exProduct);
                    }
                }

            }
            else
            {
                if (product.IsEnabled)
                {
                    lock (_syncProducts)
                    {
                        _products.Add(product);
                    }
                }
            }
            OnProductsChanged(new List<ProductModel> { product });
        }
        #endregion

        #region Events

        public delegate void BeginCashUpdateingDelegate(string key);
        public event BeginCashUpdateingDelegate BeginCashUpdateing;

        public delegate void CashUpdatedDelegate(string key);
        public event CashUpdatedDelegate CashUpdated;
        #endregion Events

        public void RemoveProduct(ProductModel product)
        {
            lock (_syncProducts)
            {
                _products.Remove(product);
            }

            OnProductsUpdated();
        }

        private static List<EsCategoriesModel> esCategories;
        public static List<EsCategoriesModel> GetEsCategories()
        {
            return esCategories ?? (esCategories = ProductsManager.GetEsCategories());
        }

        public EsmMeasureUnitModel GetMeasureOfUnit(int? itemMeasureOfUnitsId)
        {
            return Instance.MeasureOfUnits.SingleOrDefault(mu => mu.Id == itemMeasureOfUnitsId);
        }
    }
}
