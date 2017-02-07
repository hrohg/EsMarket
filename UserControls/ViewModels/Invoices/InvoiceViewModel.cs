using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using ES.Data.Enumerations;
using ES.Data.Model;
using ES.Data.Models;
using ES.Common;
using ES.DataAccess.Models;
using Shared.Helpers;
using UserControls.Commands;
using UserControls.Helpers;
using UserControls.Interfaces;
using UserControls.Views.ReceiptTickets.Views;
using ExcelImportManager = ES.Business.ExcelManager.ExcelImportManager;
using ProductModel = ES.Business.Models.ProductModel;
using ReceiptTicketViewModel = UserControls.Views.ReceiptTickets.SaleInvocieSmallTicketViewModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public class InvoiceViewModel :DocumentViewModel, IInvoiceViewModel, ITabItem
    {
        /// <summary>
        /// Initalize a new instance of the ProductViewModel class.
        /// </summary>
        #region Properties
        private const string DenayChangePriceProperty = "DenayChangePrice";

        private const string InvoiceProperty = "Invoice";
        private const string FromStockProperty = "FromStock";
        private const string ToStockProperty = "ToStock";
        protected const string IsExpiringProperty = "IsExpiring";
        protected const string FilteredInvoiceItemsProperty = "FilteredInvoiceItems";
        protected const string PartnerProperty = "Partner";
        protected const string TitleProperty = "Title";
        protected const string ShortTitleProperty = "ShortTitle";
        protected const string IsModifiedProperty = "IsModified";
        #endregion

        /// <summary>
        /// Invoice view model private properties
        /// </summary>

        #region Invoice view model private properties
        private bool _isLoading;
        //private long userId;
        protected EsUserModel User;
        protected EsMemberModel Member;
        private List<MembersRoles> _roles = new List<MembersRoles>();
        private InvoiceModel _invoice;
        private PartnerModel _partner;
        private InvoiceItemsModel _invoiceItem;
        private ObservableCollection<InvoiceItemsModel> _invoiceItems = new ObservableCollection<InvoiceItemsModel>();
        private AccountsReceivableModel _accountsReceivable;
        private InvoicePaid _invoicePaid = new InvoicePaid();
        protected List<StockModel> FromStocks { get; set; }
        protected StockModel _fromStock;
        protected StockModel _toStock;
        protected string ProductSearchKey;
        private bool _isModified;
        private void InvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (InvoiceItemsModel item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= InvoiceItemPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (InvoiceItemsModel item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += InvoiceItemPropertyChanged;
                }
            }
            InvoiceItemPropertyChanged(null, null);
        }
        private void InvoiceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvoicePaid.Total = Invoice.Total = InvoiceItems.Sum(s => s.Amount);
            Invoice.Amount = InvoiceItems.Sum(s => (s.Product.Price ?? 0) * (s.Quantity ?? 0));
            OnPropertyChanged("ProductQuantity");
            OnPropertyChanged("ActualPercentage");
            OnPropertyChanged("InvoiceProfit");
            OnPropertyChanged("ProductCount");
        }
        #endregion

        /// <summary>
        /// InvoiceViewModel public properties
        /// </summary>
        #region InvoiceViewModel External properties
        public virtual string Description { get; set; }
        public virtual string Title { get; set; }
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                OnPropertyChanged(IsModifiedProperty);
                OnPropertyChanged(ShortTitleProperty);
                OnPropertyChanged(TitleProperty);
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public virtual PartnerModel Partner
        {
            get { return _partner; }
            set
            {
                _partner = value;
                Invoice.PartnerId = value != null ? value.Id : (Guid?)null;
                IsModified = true;
                OnPropertyChanged(PartnerProperty);
            }
        }
        public InvoiceModel Invoice
        {
            get { return _invoice; }
            set
            {
                if (value == null) return;
                _invoice = value;
                OnPropertyChanged(InvoiceProperty);
                FromStock = _invoice.FromStockId != null ? StockManager.GetStock(_invoice.FromStockId, _invoice.MemberId) : null;
                ToStock = _invoice.ToStockId != null ? StockManager.GetStock(_invoice.ToStockId, _invoice.MemberId) : null;
                Partner = Invoice.Partner ?? PartnersManager.GetPartner(Invoice.PartnerId, Invoice.MemberId);
                var invoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
                InvoiceItems = new ObservableCollection<InvoiceItemsModel>();
                _invoiceItems.CollectionChanged += InvoiceItemsChanged;
                foreach (var invoiceItemsModel in invoiceItems)
                {
                    InvoiceItems.Add(invoiceItemsModel);
                }
                SelectedInvoiceItem = invoiceItems.LastOrDefault();

                IsModified = true;
            }
        }
        public bool DenayChangePrice { get { return (_roles.FirstOrDefault(s => s.RoleName == "Manager" || s.RoleName == "Director") == null); } }
        public InvoiceItemsModel InvoiceItem
        {
            get { return _invoiceItem; }
            set { _invoiceItem = value; OnPropertyChanged("InvoiceItem"); OnPropertyChanged(IsExpiringProperty); }
        }

        private InvoiceItemsModel _selectedInvoiceItem;
        public InvoiceItemsModel SelectedInvoiceItem
        {
            get { return _selectedInvoiceItem; }
            set
            {
                _selectedInvoiceItem = value;
                OnPropertyChanged("SelectedInvoiceItem");
            }
        }
        public Visibility IsExpiring
        {
            get
            {
                return InvoiceItem != null && InvoiceItem.Product != null && InvoiceItem.Product.ExpiryDays != null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public ObservableCollection<InvoiceItemsModel> InvoiceItems
        {
            get { return _invoiceItems; }
            set
            {
                _invoiceItems = value;
                OnPropertyChanged("InvoiceItems"); OnPropertyChanged(FilteredInvoiceItemsProperty);
                InvoicePaid.Total = Invoice.Total = InvoiceItems.Sum(s => s.Amount);
                Invoice.Amount = InvoiceItems.Sum(s => s.Product.Price ?? 0 * s.Quantity ?? 0);
                IsModified = true;
            }
        }
        public ObservableCollection<InvoiceItemsModel> FilteredInvoiceItems
        {
            get
            {
                return new ObservableCollection<InvoiceItemsModel>(_invoiceItems.Where(s => string.IsNullOrEmpty(Filter) ||
                    s.Code.Contains(Filter) ||
                    s.Description.Contains(Filter) ||
                    s.Price.ToString().Contains(Filter)));
            }
        }
        public AccountsReceivableModel AccountsReceivable { get { return _accountsReceivable; } set { _accountsReceivable = value; } }
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; }
        }
        public StockModel FromStock
        {
            get
            {
                return _fromStock;
            }
            set
            {
                _fromStock = value;
                Invoice.FromStockId = value != null ? value.Id : (long?)null;
                Invoice.ProviderName = value != null ? value.FullName : string.Empty;
                if (value != null) FromStocks = new List<StockModel> { value };
                OnPropertyChanged(FromStockProperty);
                IsModified = true;
            }
        }
        public StockModel ToStock
        {
            get
            {
                return _toStock;
            }
            set
            {
                _toStock = value;
                Invoice.ToStockId = value != null ? value.Id : (long?)null;
                Invoice.RecipientName = value != null ? value.FullName : string.Empty;
                OnPropertyChanged(ToStockProperty);
                IsModified = true;
            }
        }
        public string Filter { get; set; }
        public bool SaleBySingle { get; set; }
        public bool MoveBySingle { get; set; }
        public bool BuyBySingle { get; set; }

        public int ProductCount
        {
            get
            {
                return InvoiceItems.Count;
            }
        }
        public decimal ProductQuantity { get { return InvoiceItems.Sum(s => s.Quantity ?? 0); } }
        public decimal InvoiceProfit { get { return Invoice.Amount - Invoice.Total; } }
        public decimal ActualPercentage { get { return InvoiceItems.Sum(s => s.Percentage) / (ProductCount != 0 ? ProductCount : 1); } }
        #endregion

        #region Constructors
        public InvoiceViewModel(EsUserModel user, EsMemberModel member)
        {
            User = user;
            Member = member;
            Invoice = new InvoiceModel(user, member);
            Initialize();
        }
        public InvoiceViewModel(Guid id, EsUserModel user, EsMemberModel member)
        {
            User = user;
            Member = member;
            Invoice = InvoicesManager.GetInvoice(id, ApplicationManager.GetEsMember.Id);
            Initialize();
        }
        #endregion

        #region Invoiceview model private methods

        private void Initialize()
        {
            Title = "Ապրանքագիր";
            IsClosable = true;
            SaleBySingle = ApplicationManager.BuyBySingle;
            BuyBySingle = ApplicationManager.BuyBySingle;
            SetModels();
            SetICommands();
            IsModified = false;
            CanFloat = true;
        }
        private void SetICommands()
        {
            //ICommands
            RemoveInvoiceItemCommand = new RemoveInvoiceItemCommands(this);

            //PrintInvoiceItemCommand = new PrintInvoiceItemCommands(this);
            //ExportInvoiceCommand = new ExportInvoiceItemCommands(this);

            //ExportInvoiceToExcelCommand = new ExportInvoiceToExcelCommands(this);
            //ExportInvoiceToXmlCommand = new ExportInvoiceToXmlCommands(this);
            //ExportInvoiceToExcelRusCommand = new ExportInvoiceToExcelRusCommands(this);
            //ExportInvoiceToXmlRusCommand = new ExportInvoiceToXmlRusCommands(this);
            ImportInvoiceCommand = new RelayCommand(OnImportInvoice, CanImportInvoice);
            ApproveMoveInvoiceCommand = new ApproveMoveInvoiceCommands(this);
            AddItemsFromPurchaseInvoiceCommand = new AddItemsFromInvoiceCommands(this, InvoiceType.PurchaseInvoice);
            AddItemsFromMoveInvoiceCommand = new AddItemsFromInvoiceCommands(this, InvoiceType.MoveInvoice);
            AddItemsFromStocksCommand = new AddItemsFromStocksCommand(this, InvoiceType.MoveInvoice);
            ApproveInvoiceAndCloseCommand = new ApproveCloseInvoiceCommands(this);

            PrintInvoiceCommand = new RelayCommand<PrintSizeEnum>(OnPrintInvoice, CanPrintInvoice);
            CleanInvoiceIemsCommand = new RelayCommand(OnCleanInvoiceItems, CanCleanInvoiceItems);
        }
        private void SetModels()
        {
            InvoiceItem = new InvoiceItemsModel(_invoice);
            if (Partner == null)
            {
                SetDefaultPartner();
            }
            _roles = ApplicationManager.UserRoles; //UsersManager.GetUserRoles(_user.UserId, memberId: _member.Id);
            OnPropertyChanged(DenayChangePriceProperty);
        }
        private void SetDefaultPartner()
        {
            Partner = PartnersManager.GetDefaultParnerByInvoiceType(Invoice.MemberId, Invoice.InvoiceTypeId);
            var partners = ApplicationManager.CashManager.GetPartners;
            switch (Invoice.InvoiceTypeId)
            {
                //case (long)InvoiceType.SaleInvoice:
                //    var customerDefault = ApplicationManager.CashManager.GetEsDefaults(DefaultControls.Customer);
                //    Partner = customerDefault == null ? partners.FirstOrDefault() : partners.FirstOrDefault(s => s.Id == customerDefault.ValueInGuid);
                //    break;
                case (long)InvoiceType.PurchaseInvoice:
                    var provideDefault = ApplicationManager.CashManager.GetEsDefaults(DefaultControls.Provider);
                    Partner = provideDefault == null ? partners.FirstOrDefault() : partners.FirstOrDefault(s => s.Id == provideDefault.ValueInGuid);
                    break;
                default:
                    return;
            }
            if (Partner == null) { Partner = partners.FirstOrDefault(); }
        }
        public virtual decimal? GetPartnerPrice(EsProductModel product)
        {
            if (product == null)
            {
                return 0;
            }
            if (Partner == null)
            {
                return product.Price;
            }
            decimal price = 0;
            switch (Invoice.InvoiceTypeId)
            {
                case (long)InvoiceType.PurchaseInvoice:
                    return product.CostPrice;
                    break;
                case (long)InvoiceType.SaleInvoice:
                    switch (Partner.PartnersTypeId)
                    {
                        case (long)PartnerType.Dealer:
                            price = HgConvert.ToDecimal(product.DealerPrice);
                            if (HgConvert.ToDecimal(product.DealerDiscount) != 0)
                                price = Math.Max(HgConvert.ToDecimal(product.DealerPrice),
                                    HgConvert.ToDecimal(product.Price * (1 - product.DealerDiscount / 100)));
                            if (HgConvert.ToDecimal(Partner.Discount) != 0)
                            {
                                price = Math.Max(HgConvert.ToDecimal(product.DealerPrice),
                                    HgConvert.ToDecimal(product.Price) * (1 - HgConvert.ToDecimal(Partner.Discount) / 100));
                            }
                            if (price == 0) price = HgConvert.ToDecimal(product.Price);
                            break;
                        case (long)PartnerType.Customer:
                        case (long)PartnerType.Provider:
                            price = HgConvert.ToDecimal(product.Price);
                            if (HgConvert.ToDecimal(product.Discount) != 0)
                            {
                                price = Math.Max(HgConvert.ToDecimal(product.DealerPrice),
                                    HgConvert.ToDecimal(product.Price * (1 - product.Discount / 100)));
                            }
                            if (HgConvert.ToDecimal(Partner.Discount) != 0)
                            {
                                price = Math.Max(HgConvert.ToDecimal(product.DealerPrice),
                                    HgConvert.ToDecimal(product.Price) * (1 - HgConvert.ToDecimal(Partner.Discount) / 100));
                            }
                            break;
                        default:
                            price = HgConvert.ToDecimal(product.Price);
                            if (HgConvert.ToDecimal(Partner.Discount) != 0)
                            {
                                price = Math.Max(HgConvert.ToDecimal(product.DealerPrice),
                                    HgConvert.ToDecimal(product.Price) * (1 - HgConvert.ToDecimal(product.Discount) / 100));
                            }
                            break;
                    }
                    return Math.Max(HgConvert.ToDecimal(product.DealerPrice), price);
                    break;
                default:
                    return product.Price;
            }
        }
        protected virtual bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemCount(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList(), Member.Id);
            InvoiceItem.Quantity = addSingle ? 1 : GetAddedItemCount(exCount, true);
            return InvoiceItem.Quantity != null && !(InvoiceItem.Quantity <= 0);
        }

        public delegate void ShowProductsPane();
        public event ShowProductsPane ShowProducts;
        protected virtual void OnGetProduct(object o)
        {
            var products = ApplicationManager.CashManager.Products.OrderBy(s => s.Description);
            var selectedItems =
                new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(), false);
            selectedItems.SearchKey = o is FiltersUsage && ((FiltersUsage)o) == FiltersUsage.WithFilters ? ProductSearchKey : string.Empty;
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(
                    s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
            ProductSearchKey = selectedItems.SearchKey;
            if (product == null)
            {
                return;
            }
            SetInvoiceItem(product.Code);
        }

        #region Commands methods
        protected virtual bool CanImportInvoice(object o)
        {
            return true;
        }
        protected virtual void OnImportInvoice(object o)
        {
            var filePath = FileManager.OpenExcelFile();
            if (File.Exists(filePath))
            {
                var invoiceObject = ExcelImportManager.ImportInvoice(filePath);
                if (invoiceObject == null) return;
                var importInvoice = invoiceObject.Item1 as InvoiceModel;
                var importInvoiceItems = invoiceObject.Item2 as List<InvoiceItemsModel>;
                if (importInvoice == null || importInvoiceItems == null) return;
                Invoice = new InvoiceModel(User, Member, Invoice.InvoiceTypeId);
                Invoice.CreateDate = importInvoice.CreateDate;
                if (Invoice.CreateDate == DateTime.MinValue) Invoice.CreateDate = DateTime.Now;
                InvoiceItems.Clear();
                foreach (var invoiceItem in importInvoiceItems)
                {
                    if (invoiceItem == null || string.IsNullOrEmpty(invoiceItem.Code)) return;
                    var product = new ProductsManager().GetProductsByCodeOrBarcode(invoiceItem.Code, Member.Id);
                    if (product == null)
                    {
                        MessageBox.Show(invoiceItem.Code +
                            " կոդով ապրանք չի հայտնաբերվել։ Գործողությունն ընդհատված է։ Փորձեք կրկին։",
                            "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    invoiceItem.InvoiceId = Invoice.Id;
                    invoiceItem.ProductId = product.Id;
                    invoiceItem.Description = product.Description;
                    invoiceItem.Mu = product.Mu;
                    invoiceItem.Note += product.Note;
                    invoiceItem.Product = product;
                    InvoiceItems.Add(invoiceItem);
                }
            }
        }

        private bool CanCleanInvoiceItems(object o)
        {
            return Invoice!=null && Invoice.ApproveDate==null && InvoiceItems != null && InvoiceItems.Any() ;
        }

        private void OnCleanInvoiceItems(object o)
        {
            if (CanCleanInvoiceItems(o))
            {
                InvoiceItems.Clear();
                OnPropertyChanged("InvoiceItems");
            }
        }
        #endregion

        #endregion

        /// <summary>
        /// nvoiceview model public methods
        /// </summary>

        #region Invoiceview model public methods
        public decimal GetAddedItemCount(decimal? exCount, bool isCountFree)
        {
            var countWindow = new SelectCount(new SelectCountModel(null, exCount, InvoiceItem.Description, isCountFree));
            countWindow.ShowDialog();
            if (countWindow.DialogResult != null && (bool)countWindow.DialogResult)
            {
                return countWindow.SelectedCount;
            }
            return 0;
        }
        public void OnClose(object o)
        {
            if (Invoice.ApproveDate == null && InvoiceItems.Count > 0)
            {
                if (MessageBox.Show("Ապրանքագիրը հաստատված չէ։ Դու՞ք իսկապես ցանկանում եք փակել այն։", "Զգուշացում", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                { return; }
            }
            //var tabControl = o as TabControl;
            //var tabitem = tabControl != null ? tabControl.SelectedItem as TabItem : o as TabItem;
            //if (tabControl == null && tabitem != null)
            //{
            //    tabControl = (TabControl)tabitem.Parent;
            //}
            //if (tabitem != null)
            //{
            //    tabControl.Items.Remove(tabitem);
            //}
            base.OnClose(this);
        }
        public InvoiceItemsModel GetInvoiceItem(string code)
        {
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code, Member.Id);
            decimal? count = null;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                new ProductsManager().GetProductsByCodeOrBarcode(code.Substring(2, 5), Member.Id);
                count = HgConvert.ToDecimal((code.Substring(7, 5))) / 1000;
            }
            switch (Invoice.InvoiceTypeId)
            {
                case (long)InvoiceType.SaleInvoice:
                case (long)InvoiceType.MoveInvoice:
                    return GetSaleInvoiceItem(product, count);
                    break;
                case (long)InvoiceType.PurchaseInvoice:
                    return GetPurchaseInvoiceItem(product);
                    break;
                default:
                    return null;
                    break;
            }
        }
        private InvoiceItemsModel GetPurchaseInvoiceItem(ProductModel product)
        {
            if (product == null)
            {
                return null;
            }
            return new InvoiceItemsModel
            {
                InvoiceId = Invoice.Id,
                Product = product,
                ProductId = product.Id,
                Code = product.Code,
                Description = product.Description,
                Mu = product.Mu,
                Quantity = 0,
                Price = product.CostPrice,
                Discount = product.Discount,
                Note = product.Note
            };
        }

        private InvoiceItemsModel GetSaleInvoiceItem(ProductModel product, decimal? count = null)
        {
            if (product == null)
            {
                return null;
            }
            return new InvoiceItemsModel
            {
                InvoiceId = Invoice.Id,
                Product = product,
                ProductId = product.Id,
                Code = product.Code,
                Description = product.Description,
                Mu = product.Mu,
                Quantity = count,
                Price = GetPartnerPrice(product),
                CostPrice = product.CostPrice,
                Discount = product.Discount,
                Note = product.Note
            };
        }


        public virtual void SetInvoiceItem(string code)
        {
            if (string.IsNullOrEmpty(code)) { return; }
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code, Member.Id);
            decimal? count = null;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                product = new ProductsManager().GetProductsByCodeOrBarcode(code.Substring(2, 5), Member.Id);
                count = HgConvert.ToDecimal(code.Substring(7, 5)) / 1000;
            }

            if (product != null)
            {
                InvoiceItem.InvoiceId = Invoice.Id;
                InvoiceItem.Product = product;
                InvoiceItem.ProductId = product.Id;
                InvoiceItem.ExpiryDate = product.ExpiryDays != null ? DateTime.Today.AddDays((int)product.ExpiryDays) : (DateTime?)null;
                InvoiceItem.Code = product.Code;
                InvoiceItem.Description = product.Description;
                InvoiceItem.Mu = product.Mu;
                InvoiceItem.Quantity = count;
                InvoiceItem.Price = product.CostPrice;
                InvoiceItem.Discount = product.Discount;
                InvoiceItem.Note = product.Note;
            }
            OnPropertyChanged(IsExpiringProperty);
        }

        public bool CanEdit
        {
            get { return Invoice != null; }
        }
        public virtual bool CanAddInvoiceItem(object o)
        {
            return Invoice.ApproveDate == null && InvoiceItem.Product != null && InvoiceItem.Code == InvoiceItem.Product.Code;
        }
        public virtual void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o)) { return; }
            InvoiceItem.Index = InvoiceItems.Count + 1;
            InvoiceItems.Insert(0, InvoiceItem);
            SelectedInvoiceItem = InvoiceItem;
            InvoiceItem = new InvoiceItemsModel(Invoice);
            IsModified = true;
            OnPropertyChanged(FilteredInvoiceItemsProperty);
        }

        public bool CanExportInvoice()
        {
            return InvoiceItems.Count > 0;
        }
        public void ExportPrintInvoice()
        {
            ExcelExportManager.ExportInvoice(Invoice, InvoiceItems, printInvoice: true);
        }

        public void ExportInvoice(bool isCostPrice = false, bool isPrice = true)
        {
            ExcelExportManager.ExportInvoice(Invoice, InvoiceItems);
        }
        public virtual bool CanRemoveInvoiceItem()
        {
            return Invoice.ApproveDate == null && SelectedInvoiceItem != null && (ApplicationManager.UserRoles.Any(s => s.Id == (int)ESLSettingsManager.MemberRoles.SeniorSeller || s.Id == (int)ESLSettingsManager.MemberRoles.Manager));
        }
        public virtual void RemoveInvoiceItem()
        {
            var index = SelectedInvoiceItem.Index;
            foreach (var invoiceItemsModel in InvoiceItems.Where(invoiceItemsModel => invoiceItemsModel.Index > index))
            {
                invoiceItemsModel.Index--;
            }
            InvoiceItems.Remove(SelectedInvoiceItem);
            index = index ?? 0;
            index--;
            if (index < 0) index = 0;
            if (index >= InvoiceItems.Count) index = InvoiceItems.Count - 1;
            if (index < 0) return;
            SelectedInvoiceItem = InvoiceItems[index ?? 0];

        }
        public bool CanSaveInvoice(object o)
        {
            return Invoice.ApproveDate == null
                && InvoiceItems.Count != 0;
        }
        public virtual void OnSaveInvoice(object o)
        {
            if (!CanSaveInvoice(o))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողությունը հնարավոր չէ իրականացնել:Գործողության ընդհատում:", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            if (InvoicesManager.SaveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems)))
            {
                Invoice = InvoicesManager.GetInvoice(Invoice.Id, Invoice.MemberId);
                IsModified = false;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գրանցումն իրականացել է հաջողությամբ:", MessageModel.MessageTypeEnum.Success));
                return;
            }
            ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողությունը հնարավոր չէ իրականացնել:Գործողության ընդհատում:", MessageModel.MessageTypeEnum.Warning));
        }
        public virtual bool CanApprove(object o)
        {
            return CanSaveInvoice(o);
        }
        protected virtual void OnPrintInvoice(PrintSizeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new InvoicePreview(new ReceiptTicketViewModel(new ResponseReceiptModel())
            {
                Invoice = Invoice,
                InvoiceItems = InvoiceItems.ToList(),
                InvoicePaid = InvoicePaid
            });
            PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);

        }
        public void PrintInvoice(bool isPrice, bool isPrint = true)
        {
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            ExcelExportManager.ExportInvoice(Invoice, list, isPrice, isPrint);
        }
        public bool PrintReceiptFromExcel()
        {
            Tuple<InvoiceModel, List<InvoiceItemsModel>> t = ExcelImportManager.ImportSaleInvoice();
            if (t == null) return false;
            Invoice = t.Item1;
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>();
            foreach (var item in t.Item2)
            {
                InvoiceItems.Add(item);
            }
            Invoice.Total = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
            return true;
        }
        public virtual void OnApprove(object o)
        {
            if (!CanApprove(o))
            {
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;
            InvoicePaid.PartnerId = Invoice.PartnerId; // todo
            Invoice.ApproveDate = DateTime.Now;

            var invocie = InvoicesManager.ApproveInvoice(Invoice, InvoiceItems.ToList(), InvoicePaid);
            if (invocie != null)
            {
                Invoice = Invoice;
                IsModified = false;
            }
            else
            {
                Invoice.ApproverId = null;
                Invoice.Approver = null;
                Invoice.ApproveDate = null;
                MessageBox.Show("Գործողությունն իրականացման ժամանակ տեղի է ունեցել սխալ:", "Գործողության ընդհատում",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public virtual void OnApproveAndClose(object o)
        {
            if (!CanApprove(o))
            {
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            var xml = new XmlManager();
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;
            InvoicePaid.CashDeskId = xml.GetItemsByControl(XmlTagItems.SaleCashDesks).Select(s => HgConvert.ToGuid(s.Value)).SingleOrDefault();
            InvoicePaid.CashDeskForTicketId = CashDeskManager.TryGetCashDesk(false, Invoice.MemberId).Select(s => s.Id).FirstOrDefault();
            InvoicePaid.PartnerId = Invoice.PartnerId;
            switch (Invoice.InvoiceTypeId)
            {
                case (long)InvoiceType.PurchaseInvoice:
                    var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(Member.Id), false).FirstOrDefault();
                    if (stock == null)
                    {
                        MessageBox.Show("Պահեստ ընտրված չէ: Խնդրում ենք խմբագրել նոր պահեստ:", "Գործողության ընդհատում",
                            MessageBoxButton.OK, MessageBoxImage.Error); return;
                    }
                    Invoice.ToStockId = stock.Id;
                    Invoice.RecipientName = stock.FullName;
                    var purchaseInvoice = InvoicesManager.ApproveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems), InvoicePaid);
                    if (purchaseInvoice == null)
                    {
                        Invoice.ApproveDate = null;
                        MessageBox.Show("Գործողությունն իրականացման ժամանակ տեղի է ունեցել սխալ:", "Գործողության ընդհատում",
                            MessageBoxButton.OK, MessageBoxImage.Error); return;
                    }
                    Invoice = purchaseInvoice;
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel((DateTime)purchaseInvoice.ApproveDate, string.Format("Ապրանքագիր {0} հաստատված է։", purchaseInvoice.InvoiceNumber), MessageModel.MessageTypeEnum.Success));
                    break;
                case (long)InvoiceType.SaleInvoice:
                    Invoice.RecipientName = Partner.FullName;
                    var invoice = InvoicesManager.ApproveSaleInvoice(Invoice, InvoiceItems.ToList(), FromStocks.Select(s => s.Id), InvoicePaid);
                    if (invoice == null)
                    {
                        Invoice.ApproveDate = null;
                        MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում",
                            MessageBoxButton.OK, MessageBoxImage.Error); return;
                    }
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel((DateTime)invoice.ApproveDate, string.Format("Ապրանքագիր {0} հաստատված է։", invoice.InvoiceNumber), MessageModel.MessageTypeEnum.Success));
                    break;
                default:
                    AccountsReceivable = new AccountsReceivableModel(Invoice.Id, Partner.Id, User.UserId, Invoice.MemberId, null);
                    break;
            }
            OnClose(o);
        }
        public bool CanApproveMoveInvoice()
        {
            return (Invoice.ApproveDate == null && FromStock != null && ToStock != null && InvoiceItems != null && InvoiceItems.Count > 0);
        }
        public void ApproveMoveInvoice()
        {
            if (!CanApproveMoveInvoice()) return;
            Invoice.ApproverId = User.UserId;
            Invoice.ApproveDate = DateTime.Now;
            Invoice.Approver = User.FullName;
            Invoice.ProviderName = FromStock.FullName;
            Invoice.RecipientName = ToStock.FullName;
            if (InvoicesManager.ApproveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems)) == null)
            {
                Invoice.ApproveDate = null;
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում",
                    MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
            Invoice = InvoicesManager.GetInvoice(Invoice.Id, Invoice.MemberId);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
            IsModified = false;
        }
        #region Print invoice
        protected bool CanPrintInvoice(PrintSizeEnum printSize)
        {
            return Invoice != null && InvoiceItems != null && InvoiceItems.Count > 0;
        }
        #endregion
        private void CheckForPrize(InvoiceModel invoice)
        {
            if (!InvoicesManager.CheckForPrize(invoice)) return;
            MessageBox.Show(string.Format("Շնորհավորում ենք դուք շահել եք։ \n Ապրանքագիր։ {0} \n Ամսաթիվ։ {1} \n Պատվիրատու։ {2}",
                invoice.InvoiceNumber, invoice.ApproveDate, invoice.RecipientName), "Շահում", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        #region AddItemsFromPurchaseInvoiceCommands
        public bool CanAddItemsFromPurchaseInvoice()
        {
            return Invoice.ApproveDate == null;
        }
        public void AddItemsFromExistingInvoice(InvoiceType type)
        {
            var invoice = SelectItemsManager.SelectInvoice((long)type, Member.Id, null, false).FirstOrDefault();
            if (invoice == null) return;
            var invoiceItems = SelectItemsManager.SelectInvoiceItems(invoice.Id, 0);
            foreach (var ii in invoiceItems.ToList())
            {
                var newInvocieItem = GetInvoiceItem(ii.Code);
                newInvocieItem.Quantity = ii.Quantity;
                InvoiceItems.Add(newInvocieItem);
            }

        }
        #endregion
        #region AddItemsFromStocksCommands
        public bool CanAddItemsFromStocks()
        {
            return Invoice.ApproveDate == null;
        }
        public void AddItemsFromStocks()
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(Member.Id), false).FirstOrDefault();
            if (stock == null) return;
            var invoiceItems = SelectItemsManager.SelectProductItems(new List<long>() { stock.Id }, Member.Id);
            foreach (var ii in invoiceItems)
            {
                var newInvocieItem = GetInvoiceItem(ii.Code);
                newInvocieItem.Quantity = ii.Quantity;
                InvoiceItems.Add(newInvocieItem);
            }

        }
        #endregion
        #endregion

        #region Invoice view model commands
        public ICommand RemoveInvoiceItemCommand { get; private set; }
        public ICommand AddInvoiceItemCommand { get { return new RelayCommand(OnAddInvoiceItem, CanAddInvoiceItem); } }
        public ICommand SaveInvoiceCommand { get { return new RelayCommand(OnSaveInvoice, CanSaveInvoice); } }
        public ICommand PrintInvoiceItemCommand { get; private set; }
        public ICommand ApproveInvoiceCommand { get { return new RelayCommand(OnApprove, CanApprove); } }
        public ICommand ImportInvoiceCommand { get; private set; }
        public ICommand ExportInvoiceCommand { get; private set; }
        public ICommand ExportInvoiceToExcelCommand { get; private set; }
        public ICommand ExportInvoiceToXmlCommand { get; private set; }
        public ICommand ExportInvoiceToExcelRusCommand { get; private set; }
        public ICommand ExportInvoiceToXmlRusCommand { get; private set; }
        public ICommand ApproveMoveInvoiceCommand { get; private set; }
        public ICommand AddItemsFromPurchaseInvoiceCommand { get; private set; }
        public ICommand AddItemsFromMoveInvoiceCommand { get; private set; }
        public ICommand AddItemsFromStocksCommand { get; private set; }
        public ICommand ApproveInvoiceAndCloseCommand { get; private set; }
        public ICommand CloseCommand { get { return new RelayCommand(OnClose); } }
        public ICommand PrintInvoiceCommand { get; private set; }
        public ICommand GetProductCommand { get { return new RelayCommand(OnGetProduct); } }
        public ICommand CleanInvoiceIemsCommand { get; private set; }
        #endregion


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
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
