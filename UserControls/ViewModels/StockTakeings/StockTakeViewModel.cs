using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Controls;
using UserControls.Views.CustomControls;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.StockTakeings
{
    public delegate void CreateWriteOffInvoiceDelegate(List<InvoiceItemsModel> items, long? stockId, string notes = null);
    public delegate void CreateWriteInInvoiceDelegate(List<InvoiceItemsModel> items, long? stockId, string notes = null);
    public class StockTakeBaseViewModel : DocumentViewModel
    {
        #region Properties
        protected const string StockTakeItemsProperty = "StockTakeItems";
        #endregion

        #region Private properties

        private StockModel _stock;
        private EsUserModel _creator;

        #endregion

        #region Public properties

        #region Stock take
        private StockTakeModel _stockTake;
        public StockTakeModel StockTake { get { return _stockTake; } set { _stockTake = value; RaisePropertyChanged("Title"); } }

        private List<StockTakeItemsModel> _stockTakeItems;
        public List<StockTakeItemsModel> StockTakeItems
        {
            get { return _stockTakeItems != null ? _stockTakeItems.Where(s => (s.CodeOrBarcode + s.ProductDescription).ToLower().Contains(Filter != null ? Filter.ToLower() : string.Empty)).ToList() : (_stockTakeItems = new List<StockTakeItemsModel>()); }
            set
            {
                _stockTakeItems = value;
                RaiseProperties();
            }
        }

        #endregion Stock take

        public StockModel Stock { get { return _stock; } set { _stock = value; } }
        public EsUserModel Creator { get { return _creator; } set { _creator = value; } }

        public StockTakeItemsModel SelectedItem { get; set; }

        public decimal Count { get { return StockTakeItems.Count; } }
        public decimal Quantity { get { return StockTakeItems.Sum(s => s.StockTakeQuantity); } }
        public decimal Amount { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * s.StockTakeQuantity); } }
        public decimal Surplace { get { return StockTakeItems.Sum(s => (s.Quantity - s.StockTakeQuantity) < 0 ? -s.Quantity + s.StockTakeQuantity : 0); } }
        public decimal SurplaceAmunt { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * ((s.Quantity - s.StockTakeQuantity) < 0 ? -s.Quantity + s.StockTakeQuantity : 0)); } }
        public decimal Deficit { get { return StockTakeItems.Sum(s => (s.Quantity - s.StockTakeQuantity) > 0 ? s.Quantity - s.StockTakeQuantity : 0); } }
        public decimal DeficitAmount { get { return StockTakeItems.Sum(s => (s.Price ?? 0) * ((s.Quantity - s.StockTakeQuantity) > 0 ? s.Quantity - s.StockTakeQuantity : 0)); } }
        public override double Total { get { return (double)(Surplace - Deficit); } }
        public override double TotalAmount { get { return (double)(SurplaceAmunt - DeficitAmount); } }

        #region Filter
        Timer _timer;
        private void TimerElapsed(object obj)
        {
            RaiseProperties();
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
        public StockTakeBaseViewModel(StockTakeModel stockTake)
        {
            StockTake = stockTake;
            Stock = StockManager.GetStock(stockTake.StockId);
            Creator = UsersManager.GetEsUser(StockTake.CreatorId);
            Initialize();
        }
        #endregion //Constructors

        #region Private methods

        private void Initialize()
        {
            Title = string.Format("Գույքագրում {0}", StockTake.StockTakeName);
            Description = "Գույքագրում";
            IsActive = true;
            new Thread(LoadStockTakeItemsAsync).Start();
        }

        private void LoadStockTakeItemsAsync()
        {
            IsLoading = true;
            StockTakeItems = StockTakeManager.GetStockTakeItems(StockTake.Id);
            IsLoading = false;
        }
        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
        public void RaiseProperties()
        {
            RaisePropertyChanged(StockTakeItemsProperty);
            RaisePropertyChanged("Count");
            RaisePropertyChanged("Quantity");
            RaisePropertyChanged("Amount");
            RaisePropertyChanged("Surplace");
            RaisePropertyChanged("SurplaceAmunt");
            RaisePropertyChanged("Deficit");
            RaisePropertyChanged("DeficitAmount");
            RaisePropertyChanged("Total");
            RaisePropertyChanged("TotalAmount");
        }
        #endregion

        #region Public methods

        #endregion

        #region Commands

        #region Import

        private ICommand _importCommand;
        public ICommand ImportCommand { get { return _importCommand ?? (_importCommand = new RelayCommand<ExportImportEnum?>(OnImport, CanImport)); } }

        private bool CanImport(ExportImportEnum? e)
        {
            return false;
        }

        private void OnImport(ExportImportEnum? e)
        {

        }

        #endregion Import

        #region Export
        private ICommand _exportCommand;
        public ICommand ExportCommand { get { return _exportCommand ?? (_exportCommand = new RelayCommand<ExportImportEnum?>(OnExport, CanExport)); } }
        private bool CanExport(ExportImportEnum? e)
        {
            return StockTakeItems.Any();
        }
        private void OnExport(ExportImportEnum? e)
        {

        }
        #endregion Export

        #region View detiles
        private ICommand _viewDetilesCommand;
        public ICommand ViewDetilesCommand
        {
            get
            {
                return _viewDetilesCommand ?? (_viewDetilesCommand = new RelayCommand(OnViewDetiles, CanViewDetiles));
            }
        }
        private bool CanViewDetiles(object o)
        {
            return StockTakeItems.Any();
        }
        private void OnViewDetiles(object o)
        {
            var products = ProductsManager.GetProducts();
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
        #endregion View detiles

        #region Print

        private ICommand _printCommand;
        public ICommand PrintCommand { get { return _printCommand ?? (_printCommand = new RelayCommand(OnPrint, CanPrint)); } }

        private bool CanPrint(object obj)
        {
            return false;
        }

        private void OnPrint(object obj)
        {

        }
        #endregion Print

        #endregion
        #region Events
        public event CreateWriteInInvoiceDelegate CreateWriteInInvoiceEvent;
        protected void OnCreateWriteInInvoice()
        {
            var handler = CreateWriteInInvoiceEvent;
            if (handler != null)
            {
                var products = ApplicationManager.CashManager.Products;
                handler(StockTakeItems.Where(s => s.Balance > 0).Where(s => products.Any(t => t.Id == s.ProductId)).Select(s => new InvoiceItemsModel
                {
                    ProductId = s.ProductId ?? Guid.Empty,
                    Code = !string.IsNullOrEmpty(products.First(p => p.Id == s.ProductId).Barcode) ? products.First(p => p.Id == s.ProductId).Barcode : products.First(p => p.Id == s.ProductId).Code,
                    Quantity = s.Balance ?? 0
                }).ToList(), StockTake.StockId, string.Format("Գույքագրման համար {0}, ամսաթիվ {1}", StockTake.StockTakeName, StockTake.CreateDate));
            }
        }
        public event CreateWriteOffInvoiceDelegate CreateWriteOffInvoiceEvent;
        protected void OnCreateWriteOffInvoice()
        {
            var handler = CreateWriteOffInvoiceEvent;
            if (handler != null)
            {
                var products = ApplicationManager.CashManager.Products;
                handler(StockTakeItems.Where(s => s.Balance < 0).Where(s=>products.Any(t=>t.Id==s.ProductId)).Select(s => new InvoiceItemsModel
                {
                    ProductId = s.ProductId??Guid.Empty,
                    Code = !string.IsNullOrEmpty(products.First(p => p.Id == s.ProductId).Barcode)? products.First(p => p.Id == s.ProductId).Barcode: products.First(p => p.Id == s.ProductId).Code,
                    Quantity = -s.Balance ?? 0
                }).ToList(), StockTake.StockId, string.Format("Գույքագրման համար {0}, ամսաթիվ {1}", StockTake.StockTakeName, StockTake.CreateDate));
            }
        }
        #endregion Events
    }

    public class StockTakeViewModel : StockTakeBaseViewModel
    {
        public StockTakeViewModel(StockTakeModel model) : base(model) { }


    }

    public class StockTakeManagerViewModel : StockTakeBaseViewModel
    {
        #region Private properties
        private long _memberId;
        private string _productSearchKey;
        #endregion

        #region Public properties

        #region Code or barcode
        private string _codeOrBarcode;
        public string CodeOrBarcode
        {
            get { return _codeOrBarcode; }
            set
            {
                if (value == _codeOrBarcode) return;
                _codeOrBarcode = value;
                RaisePropertyChanged("CodeOrBarcode");
            }
        }
        #endregion Code or barcode

        #region StockTake item
        private const string StockTakeItemProperty = "StockTakeItem";
        private StockTakeItemsModel _stockTakeItem;
        public StockTakeItemsModel StockTakeItem
        {
            get { return _stockTakeItem; }
            set
            {
                _stockTakeItem = value;
                RaisePropertyChanged(StockTakeItemProperty);
            }
        }
        #endregion StockTake item

        #endregion

        #region Constructors
        public StockTakeManagerViewModel(StockTakeModel stockTake)
            : base(stockTake)
        {
            _memberId = ApplicationManager.Instance.GetMember.Id;
            Initialize();
        }
        #endregion //Constructors

        #region Private methods
        private void Initialize()
        {

        }
        private void SetStockTakeItem(EsProductModel product)
        {
            StockTakeItem = new StockTakeItemsModel(StockTake.Id);
            if (product != null)
            {
                StockTakeItem.ProductId = product.Id;
                StockTakeItem.CodeOrBarcode = CodeOrBarcode = product.Code;
                StockTakeItem.ProductDescription = product.Description;
                StockTakeItem.Mu = product.Mu;
                StockTakeItem.Price = product.Price;
                StockTakeItem.Quantity = ProductsManager.GetProductItemCountFromStock(product.Id, new List<long> { StockTake.StockId ?? 0 }, _memberId);
            }
            else
            {
                StockTakeItem = null;
            }
            RaisePropertyChanged(StockTakeItemProperty);
        }
        #endregion

        #region Public methods
        public void OnSetProductItem(ProductItemModel productItem)
        {
            SetStockTakeItem(productItem.Product);
        }
        public void GetProduct(Guid id)
        {
            SetStockTakeItem(new ProductsManager().GetProduct(id, _memberId));
        }

        #endregion

        #region Commands

        #region Get product command
        private ICommand _getProductCommand;
        public ICommand GetProductItemCommand { get { return _getProductCommand ?? (_getProductCommand = new RelayCommand(OnGetProductItem, CanGetProductItem)); } }
        private bool CanGetProductItem(object o)
        {
            return !string.IsNullOrEmpty(CodeOrBarcode);
        }
        private void OnGetProductItem(object o)
        {
            if (!CanGetProductItem(o)) return;
            GetProduct(CodeOrBarcode);
        }
        protected void GetProduct(string code)
        {
            SetStockTakeItem(new ProductsManager().GetProductsByCodeOrBarcode(code, _memberId));
        }
        #endregion Get product command

        #region Get product by name command
        private ICommand _getProductByNameCommand;
        public ICommand GetProductByNameCommand { get { return _getProductByNameCommand ?? (_getProductByNameCommand = new RelayCommand(OnGetProduct)); } }
        protected void OnGetProduct(object o)
        {
            var products = ApplicationManager.Instance.CashProvider.Products.OrderBy(s => s.Description);
            var selectedItems = new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(), false);
            selectedItems.SearchKey = o is FiltersUsage && ((FiltersUsage)o) == FiltersUsage.WithFilters ? _productSearchKey : string.Empty;
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
            _productSearchKey = selectedItems.SearchKey;
            if (product == null)
            {
                return;
            }
            GetProduct(product.Code);
        }
        #endregion Get product by name command

        #region Add stock take item
        private ICommand _addStockTakingItemCommand;
        public ICommand AddStockTakingItemCommand
        {
            get
            {
                return _addStockTakingItemCommand ?? (_addStockTakingItemCommand = new RelayCommand(OnAddStockTakingItem, CanAddStockTakingItem));
            }
        }
        private bool CanAddStockTakingItem(object o)
        {
            return StockTake.ClosedDate == null &&
                StockTakeItem != null &&
                StockTakeItem.ProductId != null && CodeOrBarcode == StockTakeItem.CodeOrBarcode;
        }
        public void OnAddStockTakingItem(object o)
        {
            if (!CanAddStockTakingItem(o)) { return; }
            AddStockTakingItem();

        }

        private void AddStockTakingItem(bool alwaysAdd = false)
        {
            StockTakeItem.StockTakeDate = DateTime.Now;
            var exItem = StockTakeManager.GetStockTakeItem(StockTake.Id, StockTakeItem.CodeOrBarcode, _memberId);
            var index = exItem != null ? exItem.Index : StockTakeItems.Any() ? StockTakeItems.Max(s => s.Index) + 1 : 1;
            if (exItem != null && !alwaysAdd)
            {
                if (MessageBox.Show("Տվյալ կոդով ապրանք արդեն գույքագրվել է " + exItem.StockTakeQuantity + " հատ։ \n Ցանկանու՞մ եք ավելացնել ևս " + StockTakeItem.StockTakeQuantity + "-ով։",
                    "Կրկնակի գույքագրում", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) { return; }
                StockTakeItem.StockTakeQuantity += exItem.StockTakeQuantity;
            }
            else
            {
                StockTakeItem.Index = index;
            }
            if (!StockTakeManager.EditStockTakeItems(StockTakeItem, StockTake.StockId, _memberId)) { return; }
            SelectedItem = StockTakeItems.FirstOrDefault(s => s.Index == index);
            StockTakeItem = null;
            CodeOrBarcode = null;
            RaisePropertyChanged(StockTakeItemProperty);
            StockTakeItems = StockTakeManager.GetStockTakeItems(StockTake.Id);
            RaiseProperties();
        }
        #endregion Add stock take item

        #region RemoveStockTakingItemCommand

        private ICommand _removeStockTakingItemCommand;
        public ICommand RemoveStockTakingItemCommand
        {
            get
            {
                return _removeStockTakingItemCommand ?? (_removeStockTakingItemCommand = new RelayCommand(RemoveStockTakingItem, CanRemoveStockTakingItem));
            }
        }
        private bool CanRemoveStockTakingItem(object obj)
        {
            return SelectedItem != null && StockTake.ClosedDate == null && StockTakeItems.SingleOrDefault(s => s.Id == SelectedItem.Id) != null;
        }
        private void RemoveStockTakingItem(object obj)
        {
            if (!CanRemoveStockTakingItem(obj))
            {
                return;
            }
            if (!StockTakeManager.RemoveStoCkakeItem(SelectedItem.Id, _memberId)) { return; }
            StockTakeItems = StockTakeManager.GetStockTakeItems(StockTake.Id);
            RaisePropertyChanged(StockTakeItemsProperty);
        }
        #endregion RemoveStockTakingItemCommand

        #region GetUnavailableProductItemsCommand
        private ICommand _getUnavailableProductItemsCommand;
        public ICommand GetUnavailableProductItemsCommand
        {
            get
            {
                return _getUnavailableProductItemsCommand ?? (_getUnavailableProductItemsCommand = new RelayCommand(GetUnavailableProductItems, CanGetUnavailableProductItems));
            }
        }
        public bool CanGetUnavailableProductItems(object obj)
        {
            return StockTake.ClosedDate == null;
        }
        private void GetUnavailableProductItems(object obj)
        {
            var unavailableProductItems = new ProductsManager().GetUnavailableProductItems(
                StockTakeItems.Where(s => s.ProductId != null).Select(s => s.ProductId != null ? (Guid)s.ProductId : new Guid()).ToList(), new List<long> { Stock.Id });
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
                    case MessageBoxResult.No:
                        continue;
                    case MessageBoxResult.Yes:
                        SetStockTakeItem(product);
                        StockTakeItem.StockTakeQuantity = 0;
                        OnAddStockTakingItem(null);
                        break;
                    case MessageBoxResult.None:
                        break;
                    case MessageBoxResult.OK:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private ICommand _selectUnavailableProductItemsCommand;

        public ICommand SelectUnavailableProductItemsCommand
        {
            get { return _selectUnavailableProductItemsCommand ?? (_selectUnavailableProductItemsCommand = new RelayCommand(SelectUnavailableProductItems, CanSelectUnavailableProductItems)); }
        }

        public bool CanSelectUnavailableProductItems(object obj)
        {
            return StockTake.ClosedDate == null;
        }

        private void SelectUnavailableProductItems(object obj)
        {
            var unavailableProductItems = new ProductsManager().GetUnavailableProductItems(StockTakeItems.Where(s => s.ProductId != null).Select(s => s.ProductId != null ? (Guid)s.ProductId : new Guid()).ToList(), new List<long> { Stock.Id });
            var productIds = unavailableProductItems.GroupBy(s => s.ProductId).Select(s => s.First()).OrderBy(s => s.Product.Code).ToList();

            var selectedItems = SelectItemsManager.SelectProductItems(productIds.Select(s => new ProductItemsByCheck
            {
                Id = s.ProductId,
                Code = s.Product.Code,
                Description = s.Product.Description,
                Price = s.Product.Price,
                Quantity = unavailableProductItems.Where(pi => pi.ProductId == s.ProductId).Sum(pi => pi.Quantity)
            }).ToList(), true);
            if (selectedItems == null || !selectedItems.Any()) return;
            foreach (var selectedProduct in selectedItems)
            {
                var product = unavailableProductItems.Where(s => s.ProductId == selectedProduct.Id).Select(s => s.Product).FirstOrDefault();
                if (product == null) continue;
                SetStockTakeItem(product);
                StockTakeItem.StockTakeQuantity = 0;
                AddStockTakingItem(true);
            }
        }

        #endregion GetUnavailableProductItemsCommand

        #region Completed stock taking command

        private ICommand _writeOffStockTakingCommand;

        public ICommand WriteOffStockTakingCommand
        {
            get { return _writeOffStockTakingCommand ?? (_writeOffStockTakingCommand = new RelayCommand(OnWriteOffStockTaking, CanWriteOffStockTaking)); }
        }

        private bool CanWriteOffStockTaking(object obj)
        {
            return !StockTake.IsClosed;
        }

        private void OnWriteOffStockTaking(object o)
        {
            OnCreateWriteOffInvoice();
        }

        private ICommand _writeInStockTakingCommand;

        public ICommand WriteInStockTakingCommand
        {
            get { return _writeInStockTakingCommand ?? (_writeInStockTakingCommand = new RelayCommand(OnWriteInStockTaking, CanWriteInStockTaking)); }
        }

        private bool CanWriteInStockTaking(object obj)
        {
            return !StockTake.IsClosed;
        }


        private void OnWriteInStockTaking(object o)
        {
            OnCreateWriteInInvoice();
        }

        private ICommand _completedCommand;

        public ICommand CompletedCommand
        {
            get { return _completedCommand ?? (_completedCommand = new RelayCommand(OnCompletedStockTaking, CanCompletesStockTaking)); }
        }

        private bool CanCompletesStockTaking(object obj)
        {
            return StockTake.ClosedDate == null && ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        private void OnCompletedStockTaking(object o)
        {
            if (MessageBox.Show("Դուք իսկապե՞ս ցանկանում եք ամփոփել գույքագրումը: Ուշադրություն, ամփոփումից հետո այլևս հնարավոր չի լինի խմբագրել այն:", "Գույքագրման ամփոփում", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                StockTakeManager.CompletedStockTake(StockTake);
                RaisePropertyChanged("StockTake");
            }
        }

        #endregion Completed stock takeing command

        #endregion
    }
}
