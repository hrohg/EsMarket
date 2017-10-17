using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ES.Business.Models;
using ES.Data.Model;
using ES.Data.Models;
using ES.DataAccess.Models;
using ProductModel = ES.Business.Models.ProductModel;

namespace ES.Business.Managers
{
    public class LocalManager
    {
        #region Private properties
        private EsMemberModel Member { get { return ApplicationManager.Instance.GetMember; } }
        private readonly object _locker = new object();
        private readonly bool _localMode;

        private List<StockModel> _stocks;
        private List<EsDefaults> _esDefaults;

        private bool _isProductItemsUpdating;
        private bool _isStocksUpdating;
        private bool _isPartnersUpdating;
        #endregion

        #region Public properties

        #region Partners
        private List<EsPartnersTypes> _partnersTypes;

        public List<EsPartnersTypes> GetPartnersTypes
        {
            get { return _partnersTypes ?? (_partnersTypes = PartnersManager.GetPartnersTypes(Member.Id)); }
        }
        private List<PartnerModel> _partners;

        public List<PartnerModel> GetPartners
        {
            get { return _partners ?? (_partners = PartnersManager.GetPartners()); }
        }

        #endregion Partners

        #region Cash desks

        private List<CashDesk> _cashDesks;

        public List<CashDesk> GetCashDesk
        {
            get
            {
                if (_cashDesks == null) _cashDesks = CashDeskManager.GetCashDesks();
                return _cashDesks.Where(s=>s.IsCash).ToList();
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
                if (!_localMode || _productItems == null) _productItems = new ProductsManager().GetProductItems(Member.Id);
                return _productItems.Select(s => s.Product as ProductModel).ToList();
            }
            set { _products = value; }
        }
        public List<ProductItemModel> ProductItems
        {
            get
            {
                if (!_localMode || _productItems == null)
                    _productItems = new ProductsManager().GetProductItems(Member.Id);
                return _productItems;
            }
            set
            {
                _productItems = value;
            }
        }
        public List<ProductResidue> ProductResidues
        {
            get
            {
                if (!_localMode || _productResidues == null)
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
        private List<ProductModel> _products;
        public List<ProductModel> Products
        {
            get
            {
                if (_products == null)
                {
                    UpdateProducts(false);
                }
                return _products;
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
            return GetUsers.SingleOrDefault(s=>s.UserId==id);
        }
        #endregion Users
        public List<StockModel> GetStocks
        {
            get
            {
                if (!_localMode || _stocks == null)
                {
                    _stocks = StockManager.GetStocks(Member.Id);
                }
                return _stocks;
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

        #endregion

        #region Constructors
        public LocalManager(bool isOffline)
        {
            _localMode = isOffline;
            if (isOffline)
            {
                UpdateCash();
            }
        }
        public LocalManager()
            : this(ApplicationManager.Settings.MemberSettings.IsOfflineMode)
        {

        }
        #endregion

        #region Internal methods
        private void OnProductsUpdated()
        {
            _isProductsUpdating = false;
            var updatedHandler = ProductUpdated;
            if (updatedHandler != null) updatedHandler();
        }
        private void SetPartners()
        {
            _isPartnersUpdating = true;
            lock (_locker)
            {
                _partners = PartnersManager.GetPartners();
            }
            _isPartnersUpdating = false;
        }

        private void SetStocks()
        {
            _isStocksUpdating = true;
            lock (_locker)
            {
                _stocks = StockManager.GetStocks(Member.Id);
            }
            _isStocksUpdating = false;
        }

        private void SetProductItems()
        {
            _isProductItemsUpdating = true;
            lock (_locker)
            {
                _productItems = new ProductsManager().GetProductItems(Member.Id);
            }
            _isProductItemsUpdating = false;
        }
        private void GetProducts()
        {
            _isProductsUpdating = true;
            var updateingHandler = ProductsUpdateing;
            if (updateingHandler != null) updateingHandler();
            lock (_locker)
            {
                _products = ProductsManager.GetProducts();
            }
            OnProductsUpdated();
        }

        #endregion

        #region External methods
        public void UpdateCash(bool async = true)
        {
            UpdateProducts(async);
            UpdateProductItems();
            UpdateStocks();

        }
        public void UpdateProducts(bool isAsync = true)
        {
            if (_isProductsUpdating)
            {
                while (_isProductsUpdating)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (isAsync)
                {
                    var thread = new Thread(GetProducts);
                    thread.Start();
                }
                else
                {
                    GetProducts();
                }
            }
        }
        public void UpdateProducts(List<ProductModel> products)
        {
            _products = products;
            OnProductsUpdated();
        }
        public void UpdateProductItems(bool async = true)
        {
            if (_isProductItemsUpdating)
            {
                while (_isProductItemsUpdating)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (async)
                {
                    var thread = new Thread(SetProductItems);
                    thread.Start();
                }
                else
                {
                    SetProductItems();
                }
            }
        }
        public void UpdatePartners(bool async = true)
        {
            if (_isPartnersUpdating)
            {
                while (_isPartnersUpdating)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (async)
                {
                    var thread = new Thread(SetPartners);
                    thread.Start(); 
                }
                else
                {
                    SetPartners();
                }
            }
        }
        public void UpdateStocks(bool async = true)
        {
            if (_isStocksUpdating)
            {
                while (_isStocksUpdating)
                {
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (async)
                {
                    var thread = new Thread(SetStocks);
                    thread.Start();
                }
                else
                {
                    SetStocks();
                }
            }
        }
        public void UpdateDefaults()
        {
            _esDefaults = DefaultsManager.GetDefaults();
        }
        #endregion
        
    }
}
