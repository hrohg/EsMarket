using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using ProductModel = ES.Data.Models.ProductModel;

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
        private bool _isUpdateing;

        private List<ProductModel> _products;
        #endregion

        #region Public properties
        public bool IsUpdateing
        {
            get { return _isUpdateing; }
        }
        #region Partners
        private List<EsPartnersTypes> _partnersTypes;

        public List<EsPartnersTypes> GetPartnersTypes
        {
            get { return _partnersTypes ?? (_partnersTypes = PartnersManager.GetPartnersTypes()); }
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
        private List<ProductItemModel> _productItems;
        private List<ProductResidue> _productResidues;
        public List<ProductModel> ExistingProducts
        {
            get
            {
                if (!IsLocalMode || _productItems == null) _productItems = ProductsManager.GetProductItems();
                return _productItems.Select(s => s.Product).ToList();
            }
            set { _products = value; }
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
                if (!IsLocalMode || _productResidues == null)
                    _productResidues = ProductsManager.GeProductResidues(Member.Id);
                return _productResidues;
            }
            set { _productResidues = value; }
        }

        private bool _isProductsUpdating;
        public delegate void ProductsUpdateingEvent();
        public event ProductsUpdateingEvent ProductsUpdateing;
        public delegate void ProductUpdatedDelegate();
        public event ProductUpdatedDelegate ProductUpdated;

        public List<ProductModel> Products
        {
            get
            {
                lock (_syncProducts)
                {
                    return _products;
                }
            }
        }

        #endregion Products

        #region Users

        private List<EsUserModel> _esUsers;
        public List<EsUserModel> GetUsers
        {
            get { return _esUsers ?? (_esUsers = UsersManager.GetEsUsers(Member.Id)); }
        }
        public EsUserModel GetUser(long id)
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


        public static CashManager Instance { get { return _instance ?? (_instance = new CashManager()); } }
        #endregion

        #region Constructors
        private CashManager()
        {
            UpdateCashAsync();
        }
        #endregion

        #region Internal methods
        private void OnProductsUpdated()
        {
            _isProductsUpdating = false;
            var updatedHandler = ProductUpdated;
            if (updatedHandler != null) updatedHandler();
            OnUpdateCompleted();
        }
        private void SetPartners()
        {
            _isPartnersUpdating = true;
            lock (_syncPartners)
            {
                _partners = PartnersManager.GetPartners();
            }
            _isPartnersUpdating = false;
            OnUpdateCompleted();
        }

        private void SetStocks()
        {
            _isStocksUpdating = true;
            lock (_syncStocks)
            {
                _stocks = StockManager.GetStocks();
            }
            _isStocksUpdating = false;
            OnUpdateCompleted();
        }

        private void SetProductItems()
        {
            _isProductItemsUpdating = true;
            lock (_syncProductItems)
            {
                _productItems = ProductsManager.GetProductItems();
            }
            _isProductItemsUpdating = false;
            OnUpdateCompleted();
        }
        private void GetProducts()
        {
            _isProductsUpdating = true;
            var updateingHandler = ProductsUpdateing;
            if (updateingHandler != null) updateingHandler();
            lock (_syncProducts)
            {
                _products = ProductsManager.GetProducts();
            }
            OnProductsUpdated();
        }

        private void OnBeginCashUpdateing()
        {
            _isUpdateing = true;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, delegate
            {
                var handler = BeginCashUpdateing;
                if (handler != null) handler();
            });
        }
        private void OnUpdateCompleted()
        {
            if (!_isPartnersUpdating && !_isProductItemsUpdating && !_isProductsUpdating && !_isStocksUpdating)
            {
                _isUpdateing = false;
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, delegate
                {
                    var handler = CashUpdated;
                    if (handler != null) handler();
                });
            }
        }
        #endregion

        #region External methods

        public void UpdateCashAsync()
        {
            if (_isUpdateing) return;
            OnBeginCashUpdateing();
            UpdateProductsAsync();
            UpdateProductItemsAsync();
            UpdateStocksAsync();
            UpdatePartnersAsync();
            OnUpdateCompleted();
        }
        public void UpdateProductsAsync()
        {
            lock (_syncProducts)
            {
                new Thread(GetProducts).Start();
            }
        }
        public void UpdateProducts(List<ProductModel> products)
        {
            lock (_syncProducts)
            {
                _products = products;
                OnProductsUpdated();
            }
        }
        private void UpdateProductItemsAsync()
        {
            lock (_syncProductItems)
            {
                var thread = new Thread(SetProductItems);
                thread.Start();
            }
        }
        public void UpdatePartnersAsync()
        {
            lock (_syncPartners)
            {
                new Thread(SetPartners).Start();
            }

        }
        public void UpdateStocksAsync()
        {
            lock (_syncStocks)
            {
                new Thread(SetStocks).Start();
            }
        }
        public void UpdateDefaults()
        {
            _esDefaults = DefaultsManager.GetDefaults();
        }
        #endregion

        #region Events

        public delegate void BeginCashUpdateingDelegate();
        public event BeginCashUpdateingDelegate BeginCashUpdateing;

        public delegate void CashUpdatedDelegate();
        public event CashUpdatedDelegate CashUpdated;
        #endregion Events

    }
}
