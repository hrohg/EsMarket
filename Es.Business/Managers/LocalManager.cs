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
        private bool LocalMode = false;
        private long memberId;
        private List<PartnerModel> _partners;
        private List<StockModel> _stocks;
        private List<ProductModel> _products;
        private List<ProductItemModel> _productItems;
        private List<EsDefaults> _esDefaults;
        private List<ProductResidue> _productResidues; 
        #endregion

        #region Public properties
        public List<PartnerModel> GetPartners
        {
            get
            {
                if (!LocalMode || _partners == null)
                {
                    _partners = PartnersManager.GetPartners(memberId);
                }
                return _partners;
            }
        }
        public List<StockModel> GetStocks
        {
            get
            {
                if (!LocalMode || _stocks == null)
                {
                    _stocks = StockManager.GetStocks(memberId);
                }
                return _stocks;
            }
        }
        public List<ProductModel> Products
        {
            get
            {
                if (!LocalMode || _products == null) _products = new ProductsManager().GetProducts(memberId);
                return _products;
            }
            set { _products = value; }
        }
        public List<ProductModel> ExistingProducts
        {
            get
            {
                if (!LocalMode || _productItems == null) _productItems = new ProductsManager().GetProductItems(memberId);
                return _productItems.Select(s => s.Product as ProductModel).ToList();
            }
            set { _products = value; }
        }
        public List<ProductItemModel> ProductItems
        {
            get
            {
                if (!LocalMode || _productItems == null)
                    _productItems = new ProductsManager().GetProductItems(memberId);
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
                if (!LocalMode || _productResidues == null)
                    _productResidues = ProductsManager.GeProductResidues(memberId);
                return _productResidues;
            }
            set { _productResidues = value; }
        }

        public List<EsDefaults> EsDefaults
        {
            get
            {
                if (_esDefaults == null)
                {
                    _esDefaults = DefaultsManager.GetDefaults(memberId);
                }
                return _esDefaults;
            }
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
            LocalMode = isOffline;
            memberId = ApplicationManager.GetEsMember != null ? ApplicationManager.GetEsMember.Id : 0;
            if (isOffline)
            {
                new Thread(Refresh).Start();
            }
        }
        public LocalManager()
            : this(ApplicationManager.LocalMode)
        {

        }
        #endregion

        #region Internal methods

        private void SetPartners()
        {
            _partners = PartnersManager.GetPartners(memberId);
        }

        private void SetStocks()
        {
            _stocks = StockManager.GetStocks(memberId);
        }

        private void SetProducts()
        {
            _products = new ProductsManager().GetProducts(memberId);
        }

        private void SetProductItems()
        {
            _productItems = new ProductsManager().GetProductItems(memberId);
        }

        #endregion

        #region External methods
        public void Refresh()
        {
            
            new Thread(SetProducts).Start();
            new Thread(SetStocks).Start();
            //new Thread(SetProducts).Start();
            new Thread(SetProductItems).Start();
        }
        #endregion
    }
}
