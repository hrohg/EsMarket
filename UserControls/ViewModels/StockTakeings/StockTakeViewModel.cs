using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Commands;
using UserControls.Views.CustomControls;
using ProductModel = ES.Business.Models.ProductModel;

namespace UserControls.ViewModels.StockTakeings
{
    public class StockTakeViewModel : DocumentViewModel
    {
        #region Properties

        private const string ProductProperty = "Product";
        private const string StockTakeItemProperty = "StockTakeItem";
        private const string StockTakeItemsProperty = "StockTakeItems";
        #endregion

        #region Private properties
        private long _memberId;
        private StockTakeModel _stockTake;
        private ObservableCollection<StockTakeItemsModel> _stockTakeItems = new ObservableCollection<StockTakeItemsModel>();
        private StockModel _stock;
        private EsUserModel _creator;
        private ProductModel _product;
        private StockTakeItemsModel _stockTakeItem;
        private string _productSearchKey;
        #endregion

        #region Public properties
        public string Title { get; set; }
        public bool IsLoading
        {
            get;
            set;
        }
        public string Description { get; set; }
        public bool IsModified { get; set; }
        public StockTakeModel StockTake { get { return _stockTake; } set { _stockTake = value; OnPropertyChanged("Title"); } }
        public ObservableCollection<StockTakeItemsModel> StockTakeItems
        {
            get { return new ObservableCollection<StockTakeItemsModel>(_stockTakeItems.Where(s => (s.CodeOrBarcode + s.ProductDescription).ToLower().Contains(Filter != null ? Filter.ToLower() : string.Empty)).ToList()); }
            set { _stockTakeItems = value; }
        }
        public StockModel Stock { get { return _stock; } set { _stock = value; } }
        public EsUserModel Creator { get { return _creator; } set { _creator = value; } }
        public ProductModel Product { get { return _product; } set { _product = value; OnPropertyChanged(ProductProperty); } }
        public StockTakeItemsModel StockTakeItem { get { return _stockTakeItem; } set { _stockTakeItem = value; OnPropertyChanged(StockTakeItemProperty); } }
        public StockTakeItemsModel SelectedItem { get; set; }
        public decimal Count { get { return StockTakeItems.Sum(s => s.Quantity); } }
        public decimal Amount { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * s.StockTakeQuantity); } }
        public decimal Surplace { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * ((s.Quantity - s.StockTakeQuantity) < 0 ? -s.Quantity + s.StockTakeQuantity : 0)); } }
        public decimal Deficit { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * ((s.Quantity - s.StockTakeQuantity) > 0 ? s.Quantity - s.StockTakeQuantity : 0)); } }
        public decimal Total { get { return StockTakeItems.Sum(s => s.Amount); } }

        #region Filter
        Timer _timer = null;
        private void TimerElapsed(object obj)
        {
            RaisePropertyChanged(StockTakeItemsProperty);
            DisposeTimer();
        }
        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value; RaisePropertyChanged("Filter");
                DisposeTimer();
                _timer = new Timer(TimerElapsed, null, 300, 300);
            }
        }
        #endregion Filter

        #endregion

        #region Constructors
        public StockTakeViewModel(StockTakeModel stockTake, long memberId)
        {
            _memberId = memberId;
            StockTake = stockTake;
            Stock = StockManager.GetStock(stockTake.StockId, _memberId);
            Creator = UsersManager.GetEsUser(StockTake.CreatorId);
            StockTakeItem = new StockTakeItemsModel(StockTake.Id);
            StockTakeItems = new ObservableCollection<StockTakeItemsModel>(StockTakeManager.GetStockTakeItems(stockTake.Id, _memberId));
            Initialize();
            SetCommands();
        }
        #endregion //Constructors

        #region Private methods

        private void Initialize()
        {
            Title = string.Format("Գույքագրում {0}", StockTake.StockTakeName);
            Description = "Գույքագրում";
            IsActive = true;
        }
        private void SetCommands()
        {
            GetProductItemCommand = new RelayCommand(OnGetProductItem, CanGetProductItem);
            AddStockTakingItemCommand = new AddStoCkTakingItemCommand(this);
            RemoveStockTakingItemCommand = new RemoveStockTakingItemCommand(this);
            ExportToExcelCommand = new ExportToExcelCommand(this);
            ViewDetileCommand = new ViewDetileCommand(this);
            GetUnavailableProductItemsCommand = new UnavailableProductItemsCommand(this);

            ViewDetilesCommand = new RelayCommand(OnViewDetiles, CanViewDetiles);
        }

        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        #region Command methods

        private bool CanGetProductItem(object o)
        {
            return StockTakeItem != null && !string.IsNullOrEmpty(StockTakeItem.CodeOrBarcode);
        }

        private void OnGetProductItem(object o)
        {
            if (!CanGetProductItem(o)) return;
            GetProduct(StockTakeItem.CodeOrBarcode);
        }
        #endregion Command methods
        private void SetStockTakeItem(EsProductModel product)
        {
            if (StockTakeItem == null) StockTakeItem = new StockTakeItemsModel(StockTake.Id);
            if (product != null)
            {
                StockTakeItem.ProductId = product.Id;
                StockTakeItem.CodeOrBarcode = product.Code;
                StockTakeItem.ProductDescription = product.Description;
                StockTakeItem.Mu = product.Mu;
                StockTakeItem.Price = product.Price;
                StockTakeItem.Quantity = ProductsManager.GetProductItemCountFromStock(product.Id, new List<long> { StockTake.StockId ?? 0 }, _memberId);
            }
            else
            {
                StockTakeItem.CodeOrBarcode = null;
                StockTakeItem.ProductDescription = null;
                StockTakeItem.Mu = null;
                StockTakeItem.Price = null;
                StockTakeItem.StockTakeQuantity = 0;
            }
            OnPropertyChanged(StockTakeItemProperty);
        }
        protected override void OnClose(object o)
        {
            base.OnClose(this);
        }
        protected void OnGetProduct(object o)
        {
            var products = ApplicationManager.CashManager.Products.OrderBy(s => s.Description);
            var selectedItems =
                new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(), false);
            selectedItems.SearchKey = o is FiltersUsage && ((FiltersUsage)o) == FiltersUsage.WithFilters ? _productSearchKey : string.Empty;
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(
                    s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
            _productSearchKey = selectedItems.SearchKey;
            if (product == null)
            {
                return;
            }
            GetProduct(product.Code);
        }

        private bool CanViewDetiles(object o)
        {
            return StockTakeItems.Any();
        }

        private void OnViewDetiles(object o)
        {
            var products = new ProductsManager().GetProducts(ApplicationManager.GetEsMember.Id);
            var detile = from s in StockTakeItems
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
        
        #endregion

        #region Public methods
        public void OnSetProductItem(ProductItemModel productItem)
        {
            OnGetProduct(productItem.Product.Code);
        }
        public void GetProduct(string code)
        {
            SetStockTakeItem(new ProductsManager().GetProductsByCodeOrBarcode(code, _memberId));
        }
        public void GetProduct(Guid id)
        {
            SetStockTakeItem(new ProductsManager().GetProduct(id, _memberId));
        }
        public bool CanAddStockTakingItem()
        {
            return StockTakeItem != null && StockTakeItem.StockTakeQuantity != null && StockTake.ClosedDate == null && !string.IsNullOrEmpty(StockTakeItem.CodeOrBarcode);
        }
        public void AddStockTakingItem()
        {
            if (!CanAddStockTakingItem()) { return; }
            StockTakeItem.StockTakeDate = DateTime.Now;
            var exItem = StockTakeManager.GetStockTakeItem(StockTake.Id, StockTakeItem.CodeOrBarcode, _memberId);
            if (exItem != null)
            {
                if (MessageBox.Show("Տվյալ կոդով ապրանք արդեն գույքագրվել է " + exItem.StockTakeQuantity + " հատ։ \n Ցանկանու՞մ եք ավելացնել ևս " + StockTakeItem.StockTakeQuantity + "-ով։",
                    "Կրկնակի գույքագրում", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) { return; }
                StockTakeItem.StockTakeQuantity += exItem.StockTakeQuantity;
            }
            if (!StockTakeManager.EditStockTakeItems(StockTakeItem, StockTake.StockId, _memberId)) { return; }
            StockTakeItem = new StockTakeItemsModel(StockTake.Id);
            OnPropertyChanged(StockTakeItemProperty);
            StockTakeItems = new ObservableCollection<StockTakeItemsModel>(StockTakeManager.GetStockTakeItems(StockTake.Id, _memberId));
            OnPropertyChanged(StockTakeItemsProperty);

            OnPropertyChanged("Count");
            OnPropertyChanged("Amount");
            OnPropertyChanged("Surplace");
            OnPropertyChanged("Deficit");
            OnPropertyChanged("Total");

        }
        public bool CanRemoveStockTakingItem()
        {
            return SelectedItem != null && StockTake.ClosedDate == null && StockTakeItems.SingleOrDefault(s => s.Id == SelectedItem.Id) != null;
        }
        public void RemoveStockTakingItem()
        {
            if (!CanRemoveStockTakingItem())
            {
                return;
            }
            if (!StockTakeManager.RemoveStoCkakeItem(SelectedItem.Id, _memberId)) { return; }
            //StockTakeItems.Remove(StockTakeItems.SingleOrDefault(s=>s.Id==StockTakeItem.Id));
            StockTakeItem = new StockTakeItemsModel(StockTake.Id);
            OnPropertyChanged(StockTakeItemProperty);
            StockTakeItems = new ObservableCollection<StockTakeItemsModel>(StockTakeManager.GetStockTakeItems(StockTake.Id, _memberId));
            OnPropertyChanged(StockTakeItemsProperty);
        }
        public bool CanExportToExcel()
        {
            return StockTakeItems.Count > 0;
        }
        public void ExportToExcel()
        {

        }
        public void ViewDetile()
        {

        }
        public bool CanGetUnavailableProductItems()
        {
            return StockTake.ClosedDate == null;
        }

        public void GetUnavailableProductItems()
        {
            var unavailableProductItems =
                new ProductsManager().GetUnavailableProductItems(_stockTakeItems.Where(s => s.ProductId != null).Select(s => s.ProductId != null ? (Guid)s.ProductId : new Guid()).ToList(), _memberId);
            var productItemIds = unavailableProductItems.GroupBy(s => s.ProductId).Select(s => s.Select(t => t.ProductId).First()).OrderBy(s => s).ToList();
            foreach (var productId in productItemIds)
            {
                var quantity = unavailableProductItems.Where(s => s.ProductId == productId).Sum(s => s.Quantity);
                var product = unavailableProductItems.Where(s => s.ProductId == productId).Select(s => s.Product).FirstOrDefault();
                if (product == null) continue;
                switch (MessageBox.Show(product.Description + " (" + product.Code + ") ապրանքից " + quantity + " հատ չի հաշվառվել։ \n Ցանկանու՞մ եք հաշվառել։", "Հաշվառում",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        return;
                        break;
                    case MessageBoxResult.No:
                        continue;
                        break;
                    case MessageBoxResult.Yes:
                        SetStockTakeItem(product);
                        StockTakeItem.StockTakeQuantity = 0;
                        AddStockTakingItem();
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Commands
        public ICommand GetProductItemCommand { get; private set; }
        public ICommand AddStockTakingItemCommand { get; private set; }
        public ICommand RemoveStockTakingItemCommand { get; private set; }
        public ICommand ExportToExcelCommand { get; private set; }
        public ICommand ViewDetileCommand { get; private set; }
        public ICommand GetUnavailableProductItemsCommand { get; private set; }
        public ICommand GetProductCommand { get { return new RelayCommand(OnGetProduct); } }
        public ICommand ViewDetilesCommand { get; private set; }
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
