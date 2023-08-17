using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets.Views;
using ExcelImportManager = ES.Business.ExcelManager.ExcelImportManager;
using ProductModel = ES.Data.Models.Products.ProductModel;
using ReceiptTicketViewModel = UserControls.Views.ReceiptTickets.SaleInvoiceSmallTicketViewModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public abstract class InvoiceViewModel : InvoiceViewModelBase
    {

        #region Properties
        protected const string IsExpiringProperty = "IsExpiring";
        protected const string ShortTitleProperty = "ShortTitle";
        protected readonly object _sync = new object();
        #endregion

        #region Invoice view model private properties
        private InvoiceItemsModel _invoiceItem;
        private AccountsReceivableModel _accountsReceivable;
        private InvoicePaid _invoicePaid = new InvoicePaid();
        protected bool HasItems
        {
            get { return InvoiceItems.Any(s => s != null && s.Quantity > 0); }
        }
        protected string ProductSearchKey;
        #endregion

        #region InvoiceViewModel External properties
        public override string Title
        {
            get
            {
                string invoice = null;
                switch ((InvoiceType)Invoice.InvoiceTypeId)
                {
                    case InvoiceType.PurchaseInvoice:
                        invoice = "Գնում";
                        break;
                    case InvoiceType.SaleInvoice:
                        invoice = "Վաճառք";
                        break;
                    case InvoiceType.ProductOrder:
                        invoice = "Պատվեր";
                        break;
                    case InvoiceType.MoveInvoice:
                        invoice = "Տեսափոխություն";
                        break;
                    case InvoiceType.InventoryWriteOff:
                        invoice = "Դուրսգրում";
                        break;
                    case InvoiceType.ReturnFrom:
                        invoice = "Ետ վերադարձ";
                        break;
                    case InvoiceType.ReturnTo:
                        invoice = "Վերադարձ";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return string.Format("{0} {1} ({2})", invoice, Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty, base.Title);
            }
            set { base.Title = value; }
        }

        public override bool IsModified
        {
            get { return base.IsModified; }
            set
            {
                if (Invoice.IsApproved) base.IsModified = false;
                else base.IsModified = value;
                DisposeTimer();
                if (IsModified)
                {
                    OnAutoSaveAsync();
                }
                else
                {
                    OnDeleteAutoSaveAsync();
                }
            }
        }

        #region Is loading

        private bool _isLoading;

        public override bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }

        #endregion Is loading

        #region Code

        private string _code;

        public string Code
        {
            get { return _code; }
            set
            {
                if (value == _code) return;
                _code = value;
                RaisePropertyChanged("Code");
            }
        }

        #endregion Code
        public virtual bool DenyChangeQuantity
        {
            get { return false; }
        }
        public virtual bool DenyChangePrice
        {
            get { return false; }
        }

        public InvoiceItemsModel InvoiceItem
        {
            get { return _invoiceItem; }
            set
            {
                _invoiceItem = value;
                Code = InvoiceItem.Code;
                RaisePropertyChanged("InvoiceItem");
                RaisePropertyChanged("DenyChangePrice");
                RaisePropertyChanged(IsExpiringProperty);
                RaisePropertyChanged("IsInvoiceItemEnabled");
            }
        }


        private InvoiceItemsModel _selectedInvoiceItem;

        public InvoiceItemsModel SelectedInvoiceItem
        {
            get { return _selectedInvoiceItem; }
            set
            {
                _selectedInvoiceItem = value;
                RaisePropertyChanged("SelectedInvoiceItem");
                RaisePropertyChanged("RemoveInvoiceItemCommand");
            }
        }

        public Visibility IsExpiring
        {
            get { return InvoiceItem != null && InvoiceItem.Product != null && InvoiceItem.Product.ExpiryDays != null ? Visibility.Visible : Visibility.Hidden; }
        }

        public AccountsReceivableModel AccountsReceivable
        {
            get { return _accountsReceivable; }
            set { _accountsReceivable = value; }
        }

        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; }
        }

        public bool MoveBySingle { get; set; }
        private bool _addBySingle;

        public virtual bool AddBySingle
        {
            get { return _addBySingle; }
            set
            {
                _addBySingle = value;
                RaisePropertyChanged("AddBySingle");
                RaisePropertyChanged("AddSingleTooltip");
            }
        }

        public string AddSingleTooltip
        {
            get { return AddBySingle ? "Ավելացնել մեկական" : "Ավելացնել բազմակի"; }
        }

        public int ProductCount
        {
            get { return InvoiceItems.Count; }
        }

        public decimal ProductQuantity
        {
            get { return InvoiceItems.Sum(s => s.Quantity ?? 0); }
        }

        public decimal InvoiceProfit
        {
            get { return Invoice.Amount - Invoice.Total; }
        }

        public decimal ActualPercentage
        {
            get { return Invoice.Amount != 0 ? InvoiceProfit * 100 / Invoice.Amount : 100; }
        }

        public string PartnerFilter
        {
            get { return _partnerFilter; }
            set
            {
                _partnerFilter = value;
                RaisePropertyChanged("PartnerFilter");
            }
        }

        public ImageState InvoiceStateImageState
        {
            get { return ImageHelper.GetInvoiceStateImage(Invoice.IsApproved); }
        }

        public string InvoiceStateTooltip
        {
            get { return Invoice.IsApproved ? string.Format("Հաստատված է {0}", Invoice.ApproveDate) : "Հաստատված չէ"; }
        }

        public string Notes
        {
            get { return Invoice.Notes; }
            set
            {
                Invoice.Notes = value;
                RaisePropertyChanged("Notes");
                IsModified = true;
            }
        }

        #endregion

        #region Constructors

        protected InvoiceViewModel(InvoiceType type)
        {
            Invoice = new InvoiceModel(User, Member) { InvoiceTypeId = (short)type };
            Initialize();
        }

        protected InvoiceViewModel(Guid id)
        {
            Invoice = InvoicesManager.GetInvoice(id);
            LoadInvoice();
            Initialize();
        }

        #endregion Constructors

        #region Invoice view model internal methods

        private void Initialize()
        {
            IsClosable = true;
            CanFloat = true;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            InvoiceItem = new InvoiceItemsModel(Invoice);
            SetCommands();
            IsModified = false;
        }
        private void SetCommands()
        {
            //ICommands
            RemoveInvoiceItemCommand = new RelayCommand(RemoveInvoiceItem, CanRemoveInvoiceItem);

            AddItemsFromStocksCommand = new RelayCommand(OnAddItemsFromStocks, CanAddItemsFromStocks);
            ApproveInvoiceAndCloseCommand = new RelayCommand(OnApproveAndClose, CanApprove);

            GetProductCommand = new RelayCommand(OnGetProduct);

            CleanInvoiceIemsCommand = new RelayCommand(OnCleanInvoiceItems, CanCleanInvoiceItems);
            ImportInvoiceCommand = new RelayCommand<ExportImportEnum>(OnImportInvoice, CanImportInvoice);

        }

        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);
            Invoice.Amount = InvoiceItems.Sum(s => (s.ProductPrice ?? 0) * (s.Quantity ?? 0));
            InvoicePaid.Total = Invoice.Total = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
            RaisePropertyChanged(() => ProductQuantity);
            RaisePropertyChanged(() => ActualPercentage);
            RaisePropertyChanged(() => InvoiceProfit);
            RaisePropertyChanged(() => ProductCount);
        }

        protected void SetDefaultPartner(PartnerModel partner)
        {
            Invoice.Partner = Partner = partner;
        }
        protected virtual decimal GetProductPrice(ProductModel product)

        {
            if (product == null) return 0;
            if (Partner == null) return product.ProductPrice;
            switch (Partner.PartnersTypeId)
            {
                case (short)PartnerType.Dealer:
                    return product.ProductDealerPrice;
                default:
                    return product.ProductPrice;
            }
        }
        protected virtual decimal? GetProductDiscount(ProductModel product)
        {
            if (product == null) return 0;
            if (Partner == null) return product.HasDiscount ? decimal.Round((decimal)product.Discount, 2) : (decimal?)null;
            switch (Partner.PartnersTypeId)
            {
                case (short)PartnerType.Dealer:
                    return product.HasDealerDiscount ? decimal.Round((decimal)product.DealerDiscount, 2) : (decimal?)null;
                default:
                    return product.HasDiscount ? decimal.Round((decimal)product.Discount, 2) : (decimal?)null;
            }
        }
        protected virtual bool SetQuantity(bool addSingle)
        {
            if (InvoiceItem.Quantity > 0) return true;
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());
            InvoiceItem.Quantity = addSingle ? 1 : GetAddedItemCount(exCount, true);
            return InvoiceItem.Quantity != null && !(InvoiceItem.Quantity <= 0);
        }

        private void RemoveInvoiceItem(InvoiceItemsModel invoiceItem)
        {
            foreach (var invoiceItemsModel in InvoiceItems.Where(invoiceItemsModel => invoiceItemsModel.DisplayOrder > invoiceItem.DisplayOrder))
            {
                invoiceItemsModel.DisplayOrder--;
            }
            InvoiceItems.Remove(invoiceItem);
        }

        protected virtual void OnGetProduct(object o)
        {
            var products = ApplicationManager.Instance.CashProvider.GetProducts().OrderBy(s => s.Description);
            var selectedItems = new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(), false);
            selectedItems.SearchKey = o is FiltersUsage && ((FiltersUsage)o) == FiltersUsage.WithFilters ? ProductSearchKey : string.Empty;
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null) ? products.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id)) : null;
            ProductSearchKey = selectedItems.SearchKey;
            if (product == null)
            {
                return;
            }
            SetInvoiceItem(product.Code);
        }

        protected virtual void OnAddItemsFromInvoice(object o)
        {
            var invoice = SelectItemsManager.SelectInvoice(InvoicesManager.Convert(InvoiceTypeEnum.PurchaseInvoice | InvoiceTypeEnum.MoveInvoice), DateTime.Today.AddYears(-1)).FirstOrDefault();
            AddItemsFromInvoice(invoice);
        }
        protected void AddItemsFromInvoice(InvoiceModel invoice)
        {
            if (invoice == null) return;
            var invoiceItems = SelectItemsManager.SelectInvoiceItems(invoice.Id);
            if (invoiceItems == null) return;
            lock (_sync)
            {
                foreach (var ii in invoiceItems.ToList())
                {
                    var newInvocieItem = GetInvoiceItem(ii.Code);
                    if (newInvocieItem == null) continue;
                    newInvocieItem.Quantity = ii.Quantity;
                    InvoiceItems.Add(newInvocieItem);
                }
            }
        }
        #region Commands methods

        #region Print invoice

        protected override bool CanPrintInvoice(PrintModeEnum printSize)
        {
            return Invoice != null && InvoiceItems != null && InvoiceItems.Count > 0;
        }

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize))
            {
                return;
            }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new InvoicePreview(new ReceiptTicketViewModel(new ResponseReceiptModel())
            {
                Invoice = Invoice,
                InvoiceItems = InvoiceItems.ToList(),
                InvoicePaid = InvoicePaid
            });
            PrintManager.PrintPreview(ctrl, "Print invoice", true);
            //PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(new ReceiptTicketViewModel(new ResponceReceiptModel()){Invocie = Invoice, InvoiceItems = InvoiceItems.ToList(), InvoicePaid = InvoicePaid}), ApplicationManager.ActivePrinter);
        }

        #endregion

        private bool CanCleanInvoiceItems(object o)
        {
            return Invoice != null && Invoice.ApproveDate == null && InvoiceItems != null && InvoiceItems.Any();
        }

        private void OnCleanInvoiceItems(object o)
        {
            if (CanCleanInvoiceItems(o))
            {
                InvoiceItems.Clear();
                RaisePropertyChanged("InvoiceItems");
            }
        }

        #endregion

        #region Auto save

        private Timer _timer;

        private void DisposeTimer()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        private void TimerElapsed(object obj)
        {
            DisposeTimer();
            OnAutoSave();
        }

        private void OnAutoSaveAsync()
        {
            if (Invoice.Id == Guid.Empty || Invoice.InvoiceTypeId == 0 || Invoice.IsApproved || !IsModified)
            {
                DisposeTimer();
                return;
            }
            new Thread(() =>
            {
                if (IsModified)
                {
                    DisposeTimer();
                    _timer = new Timer(TimerElapsed, null, 5000, Timeout.Infinite);
                }
            }).Start();
        }

        private void OnAutoSave()
        {
            lock (_sync)
            {
                InvoicesManager.AutoSaveInvoice(Invoice, InvoiceItems.ToList());
            }
        }

        private void OnDeleteAutoSaveAsync()
        {
            new Thread(() =>
            {
                DisposeTimer();
                InvoicesManager.RemoveAutoSaveInvoice(Invoice.Id);
            }).Start();
        }

        #endregion Auto save

        #region Import

        protected virtual bool CanImportInvoice(ExportImportEnum importFrom)
        {
            return !Invoice.IsApproved;
        }

        protected virtual void OnImportInvoice(ExportImportEnum importFrom)
        {
            switch (importFrom)
            {
                case ExportImportEnum.Xml:
                    break;
                case ExportImportEnum.Excel:
                    var filePath = FileManager.OpenExcelFile();
                    if (File.Exists(filePath))
                    {
                        var invoiceObject = ExcelImportManager.ImportInvoice(filePath);
                        if (invoiceObject == null) return;
                        var importInvoice = invoiceObject.Item1;
                        var importInvoiceItems = invoiceObject.Item2;
                        if (importInvoice == null || importInvoiceItems == null) return;
                        Invoice = new InvoiceModel(User, Member, Invoice.InvoiceTypeId);
                        Invoice.CreateDate = importInvoice.CreateDate;
                        if (Invoice.CreateDate == DateTime.MinValue) Invoice.CreateDate = DateTime.Now;
                        InvoiceItems.Clear();
                        foreach (var invoiceItem in importInvoiceItems)
                        {
                            if (invoiceItem == null || string.IsNullOrEmpty(invoiceItem.Code)) return;
                            var products = ProductsManager.GetProductsByCodeOrBarcode(invoiceItem.Code);
                            if (!products.Any())
                            {
                                MessageManager.ShowMessage(
                                    invoiceItem.Code +
                                    " կոդով ապրանք չի հայտնաբերվել։ Գործողությունն ընդհատված է։ Փորձեք կրկին։",
                                    "Գործողության ընդհատում");
                                return;
                            }

                            var product = SelectItemsManager.SelectProduct(products).FirstOrDefault();
                            if (product == null) return;

                            invoiceItem.InvoiceId = Invoice.Id;
                            invoiceItem.ProductId = product.Id;
                            invoiceItem.Description = product.Description;
                            invoiceItem.Note = product.Note;
                            invoiceItem.Product = product;
                            InvoiceItems.Add(invoiceItem);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("importFrom", importFrom, null);
            }


        }

        protected virtual void ImportFromXml()
        {
            var invoice = InvoicesManager.ImportInvoiceFromXml();
            //if (invoice == null || invoice.InvoiceInfo == null || invoice.BuyerInfo == null || invoice.GoodsInfo == null) return;
            // var partner = ApplicationManager.CashManager.GetPartners.FirstOrDefault(s => s.FullName == invoice.BuyerInfo.Name && s.TIN == invoice.BuyerInfo.Tin);
            //if (partner != null) Partner = partner;
            //foreach (var goodsInfo in invoice.GoodsInfo)
            //{
            //    GetInvoiceItem(goodsInfo.Code);
            //    if (InvoiceItem != null)
            //    {
            //        InvoiceItem.Price = goodsInfo.Price;
            //        InvoiceItem.Quantity = goodsInfo.Quantity;
            //        InvoiceItems.Add(InvoiceItem);
            //        InvoiceItem = null;
            //    }
            //}
        }
        protected virtual void ImportFromExcel()
        {
            InvoiceItems.Clear();
            var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ApplicationManager.Settings.SettingsContainer.MemberSettings.ImportingFilePath);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                IsLoading = false;
                return;
            }
            var memberSettings = ApplicationManager.Settings.SettingsContainer.MemberSettings;
            memberSettings.ImportingFilePath = Path.GetDirectoryName(filePath);
            MemberSettings.Save(ApplicationManager.Settings.SettingsContainer.MemberSettings, memberSettings.MemberId);
            var invoice = ExcelImportManager.ImportSaleInvoice(filePath);
            if (invoice == null) return;
            var invoiceItems = invoice.Item2;
            foreach (var item in invoiceItems)
            {
                var products = ProductsManager.GetProductsByCodeOrBarcode(item.Code);
                var product = SelectItemsManager.SelectProduct(products).FirstOrDefault();

                if (product == null)
                {
                    product = new ProductModel(item.Code, Member.Id, User.UserId, true)
                    {
                        Id = item.ProductId,
                        Description = item.Description,
                        //Mu = item.Mu,
                        HcdCs = item.Product != null ? item.Product.HcdCs : null,
                        Price = item.Price
                    };
                    var result = MessageBox.Show(string.Format("{0}({1}) ապրանքը գրանցված չէ: Ցանկանու՞մ եք գրանցել:", item.Description, item.Code), "Ապրանքի գրանցում", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes && result != MessageBoxResult.No)
                    {
                        return;
                    }

                    if (result == MessageBoxResult.Yes)
                    {
                        product = ProductsManager.EditProduct(product);
                    }
                }
                var exProducts = ProductsManager.GetProductsByCodeOrBarcode(product.Code);
                var exProduct = SelectItemsManager.SelectProduct(exProducts).FirstOrDefault();
                if (exProduct == null) return;
                var discount = exProduct.Price != null && exProduct.Price > 0 && exProduct.Price > item.Price ? Math.Round((decimal)(100 - 100 * item.Price / exProduct.Price), 2) : (decimal?)0.00;
                var price = exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price * (100 - (discount ?? 0)) / 100 : item.Price;
                InvoiceItem = new InvoiceItemsModel
                {
                    ProductId = product.Id,
                    Product = product,
                    Code = product.Code,
                    Description = product.Description,
                    Price = price,
                    Discount = discount != 0 ? discount : null,
                    Quantity = item.Quantity
                };

                OnAddInvoiceItem();
                InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
                RaisePropertyChanged("InvoicePaid");
            }
        }

        #endregion Import

        public void ExportInvoice(bool isCostPrice = false, bool isPrice = true)
        {
            ExcelExportManager.ExportInvoice(Invoice, InvoiceItems);
        }
        private void OnSaveInvoice(object o)
        {
            Save();
        }

        //AddInvoice item methods
        //todo
        public void AddInvoiceItem()
        {
            OnAddInvoiceItem();
        }
        protected virtual void PreviewAddInvoiceItem(object o)
        {
            if (InvoiceItem == null || InvoiceItem.Quantity == null) return;
            OnAddInvoiceItem();
        }

        protected virtual void OnAddInvoiceItem()
        {
            if (!CanAddInvoiceItem(null))
            {
                MessageManager.ShowMessage("Ապրանքի ավելացումը հնարավոր չէ:");
                return;
            }

            lock (_sync)
            {
                var exInvocieItem = InvoiceItems.FirstOrDefault(s =>
                    s.ProductId == InvoiceItem.ProductId && s.ProductItemId == InvoiceItem.ProductItemId && s.Price == InvoiceItem.Price);

                if (exInvocieItem != null)
                {
                    exInvocieItem.Quantity += InvoiceItem.Quantity;
                    //exInvocieItem.Description += string.Join(string.IsNullOrEmpty(exInvocieItem.Description) ? "" : ", ", InvoiceItem.Description);
                    SelectedInvoiceItem = exInvocieItem;
                }
                else
                {
                    //InvoiceItem.InvoiceItemChanged += OnInvoiceItemChanged;
                    InvoiceItem.DisplayOrder = (short)(InvoiceItems.Count + 1);
                    InvoiceItems.Insert(0, InvoiceItem);
                    SelectedInvoiceItem = InvoiceItem;
                }
            }

            InvoiceItem = new InvoiceItemsModel(Invoice);
            IsModified = true;
        }

        protected virtual void OnInvoiceItemChanged(InvoiceItemsModel invoiceItem)
        {

        }

        protected virtual void CreateNewInvoiceItem(ProductItemModel productItem)
        {
            if (!IsSelected || productItem == null || productItem.Product == null) return;
            InvoiceItem = new InvoiceItemsModel
            {
                InvoiceId = Invoice.Id,
                Product = productItem.Product,
                ProductItemId = productItem.Id != Guid.Empty ? productItem.Id : (Guid?)null,
                ProductId = productItem.ProductId,
                //ExpiryDate = product.ExpiryDays != null ? DateTime.Today.AddDays((int)product.ExpiryDays) : (DateTime?)null;
                Code = productItem.Product.Code,
                Description = productItem.Product.Description,
                Price = GetProductPrice(productItem.Product),
                //Do not set a discount, because the price is indicated
                //Discount = GetProductDiscount(productItem.Product),
                Note = productItem.Product.Note
            };
            RaisePropertyChanged(IsExpiringProperty);
            RaisePropertyChanged("InvoiceItem");
        }

        protected bool Save()
        {
            if (Invoice.ApproveDate != null || !InvoiceItems.Any())
            {
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    MessageBox.Show("Գրանցումը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Warning);
                });

                return false;
            }
            if (!InvoicesManager.SaveInvoice(Invoice, InvoiceItems.ToList()))
            {
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, () =>
                {
                    MessageBox.Show("Գրանցումը ձախողվել է:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return false;
            }
            IsModified = false;
            return true;
        }

        protected virtual bool CanApprove(object o)
        {
            return !IsLoading && !Invoice.IsApproved && HasItems;
        }

        protected abstract void OnApprove(object o);
        protected abstract void OnApproveAsync(bool closeOnExit);

        protected void ReloadApprovedInvoice(InvoiceModel invoice)
        {
            ApplicationManager.CashManager.UpdatePartnersAsync();
            Invoice = invoice;
            LoadInvoice();
            AccountsReceivable = new AccountsReceivableModel(Invoice.Id, Partner.Id, User.UserId, Invoice.MemberId, true);

            IsModified = false;
            IsLoading = false;

            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
        }

        protected void ApproveCompleted(bool isSuccess)
        {
            if (isSuccess)
            {
                RaisePropertyChanged("RemoveInvoiceItemCommand");
            }
            else
            {
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Գործողության ընդհատում: Գործողությունը ձախողվել է:", MessageTypeEnum.Warning));

            }
        }

        protected void OnClose()
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () => { Close(); });
        }
        #endregion Internal methods

        #region External methods
        internal void SetPartnerCardNumber(string barcode)
        {
            OnFindPartner(barcode);
        }
        public decimal? GetAddedItemCount(decimal? exCount, bool isCountFree)
        {
            var countWindow = new SelectCount(new SelectCountModel(null, exCount, InvoiceItem.Description, isCountFree));
            countWindow.ShowDialog();
            if (countWindow.DialogResult != null && (bool)countWindow.DialogResult)
            {
                return countWindow.SelectedCount;
            }
            return null;
        }
        public override void SetExternalText(ExternalTextImputEventArgs e)
        {
            //var invoiceItem = GetInvoiceItem(barcode);
            OnSetInvoiceItem(e.Text);
            e.Handled = true;
            base.SetExternalText(e);
        }
        protected virtual InvoiceItemsModel GetInvoiceItem(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            var products = ProductsManager.GetProductsByCodeOrBarcode(code);
            decimal count = 0;
            if (products == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                products = ProductsManager.GetProductsByCodeOrBarcode(code.Substring(2, 5));
                count = HgConvert.ToDecimal((code.Substring(7, 5))) / 1000;
            }

            var product = SelectItemsManager.SelectProduct(products).FirstOrDefault();
            if (product == null)
            {
                MessageBox.Show(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code));
                return null;
            }
            return new InvoiceItemsModel
            {
                InvoiceId = Invoice.Id,
                Product = product,
                ProductId = product.Id,
                Code = product.Code,
                Description = product.Description,
                Quantity = count,
                Price = product.CostPrice,
                Note = product.Note
            };
        }

        public void OnSetProductItem(ProductItemModel productItem)
        {
            if (!IsSelected) return;
            CreateNewInvoiceItem(productItem);
            PreviewAddInvoiceItem(null);
        }

        public virtual void SetInvoiceItem(Guid productId)
        {
            if (productId == Guid.Empty)
            {
                return;
            }
            var product = ProductsManager.GetProduct(productId);
            decimal? count = null;
            InvoiceItem = new InvoiceItemsModel(Invoice, product);
            if (product != null)
            {
                InvoiceItem.Quantity = count;
                InvoiceItem.Price = GetProductPrice(product);
            }
            else
            {
                MessageManager.OnMessage(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", productId), MessageTypeEnum.Warning);
                MessageManager.ShowMessage(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", productId), "Անհայտ ապրանք");
            }

            RaisePropertyChanged("InvoiceItem");
            RaisePropertyChanged(IsExpiringProperty);
        }

        public virtual void SetInvoiceItem(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }
            var products = ProductsManager.GetProductsByCodeOrBarcode(code);
            var product = SelectItemsManager.SelectProduct(products).FirstOrDefault();

            decimal? count = null;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                products = ProductsManager.GetProductsByCodeOrBarcode(code.Substring(2, 5));
                product = SelectItemsManager.SelectProduct(products).FirstOrDefault();

                count = HgConvert.ToDecimal(code.Substring(7, 5)) / 1000;
                if (product == null || !product.IsWeight) MessageManager.ShowMessage("Ապրանքը քաշային չէ:", "Ապրանքի անհամապատասխանություն");
            }
            InvoiceItem = new InvoiceItemsModel(Invoice, product);
            if (product != null)
            {
                InvoiceItem.Quantity = count;
                InvoiceItem.Price = GetProductPrice(product);
            }
            else
            {
                MessageManager.OnMessage(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code), MessageTypeEnum.Warning);
                MessageManager.ShowMessage(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code), "Սխալ կոդ");
            }

            RaisePropertyChanged("InvoiceItem");
            RaisePropertyChanged(IsExpiringProperty);
        }

        public bool CanEdit
        {
            get { return Invoice != null; }
        }
        public bool IsInvoiceItemEnabled { get { return Invoice.ApproveDate == null && InvoiceItem.Product != null && InvoiceItem.Code == InvoiceItem.Product.Code && !IsLoading; } }

        public virtual bool CanAddInvoiceItem(object o)
        {
            return Invoice.ApproveDate == null && InvoiceItem.Product != null && InvoiceItem.Code == InvoiceItem.Product.Code && !IsLoading;
        }

        protected virtual bool CanRemoveInvoiceItem(object o)
        {
            return !Invoice.IsApproved && SelectedInvoiceItem != null;
        }

        protected virtual void RemoveInvoiceItem(object o)
        {
            int index = SelectedInvoiceItem.DisplayOrder;
            RemoveInvoiceItem(SelectedInvoiceItem);
            index--;
            if (index < 0) index = 0;
            if (index >= InvoiceItems.Count) index = InvoiceItems.Count - 1;
            if (index < 0) return;
            SelectedInvoiceItem = InvoiceItems[index];
        }

        protected virtual void RemoveInvoiceItems(IList invoiceItems)
        {
            var items = invoiceItems.Cast<InvoiceItemsModel>().ToList();
            int index = 0;
            foreach (var invoiceItem in items)
            {
                index = SelectedInvoiceItem.DisplayOrder;
                RemoveInvoiceItem(invoiceItem);
            }
            index--;
            if (index < 0) index = 0;
            if (index >= InvoiceItems.Count) index = InvoiceItems.Count - 1;
            if (index < 0) return;
            SelectedInvoiceItem = InvoiceItems[index];
        }

        public bool CanSaveInvoice(object o)
        {
            return !IsLoading && Invoice.ApproveDate == null && InvoiceItems.Count != 0 && IsModified;
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
            InvoiceItems.Clear();
            lock (_sync)
            {
                foreach (var item in t.Item2)
                {
                    InvoiceItems.Add(item);
                }
            }
            return true;
        }

        public virtual void OnApproveAndClose(object o)
        {
            if (!CanApprove(o))
            {
                MessageManager.ShowMessage("Գործողությունը հնարավոր չէ իրականացնել: Թերի տվյալներ:", "Գործողության ընդհատում", MessageBoxImage.Error);
                return;
            }
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;
        }

        public bool CanApproveMoveInvoice()
        {
            return (Invoice.ApproveDate == null && FromStock != null && ToStock != null && InvoiceItems != null && InvoiceItems.Count > 0);
        }

        //public void ApproveMoveInvoice()
        //{
        //    if (!CanApproveMoveInvoice()) return;
        //    Invoice.ApproverId = User.UserId;
        //    Invoice.ApproveDate = DateTime.Now;
        //    Invoice.Approver = User.FullName;
        //    Invoice.ProviderName = FromStock.FullName;
        //    Invoice.RecipientName = ToStock.FullName;
        //    if (InvoicesManager.ApproveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems), null) == null)
        //    {
        //        Invoice.ApproveDate = null;
        //        MessageManager.ShowMessage("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxImage.Error);
        //        return;
        //    }
        //    Invoice = InvoicesManager.GetInvoice(Invoice.Id);
        //    LoadInvoice();
        //    RaisePropertyChanged("InvoiceStateImageState");
        //    RaisePropertyChanged("InvoiceStateTooltip");
        //}

        #region AddItemsFromStocksCommands

        public bool CanAddItemsFromStocks(object o)
        {
            return Invoice.ApproveDate == null;
        }

        public void OnAddItemsFromStocks(object o)
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(), false).FirstOrDefault();
            if (stock == null) return;
            var invoiceItems = SelectItemsManager.SelectProductItemsFromStock(new List<short> { stock.Id });
            if (invoiceItems == null) return;
            lock (_sync)
            {
                foreach (var ii in invoiceItems)
                {
                    var newInvocieItem = GetInvoiceItem(ii.Code);
                    if (newInvocieItem == null) continue;
                    newInvocieItem.Quantity = ii.Quantity;
                    InvoiceItems.Add(newInvocieItem);
                }
            }
        }

        #endregion

        public override bool Close()
        {
            IsActive = true;
            if (IsModified)
            {
                var result = MessageManager.ShowMessage("Ապրանքագիրը փոփոխված է։ Դուք ցանկանու՞մ եք պահպանել փոփոխությունները։", "Պահպանել", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        if (!Save()) return false;
                        break;
                    case MessageBoxResult.None:
                    case MessageBoxResult.Cancel:
                        return false;
                    case MessageBoxResult.Yes:
                        if (!Save()) return false;
                        break;
                    case MessageBoxResult.No:
                        if (ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller)) return false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                OnDeleteAutoSaveAsync();
            }
            return base.Close();
        }

        #endregion External methods

        #region Commands

        public ICommand RemoveInvoiceItemCommand { get; private set; }
        private ICommand _removeInvoiceItemsCommand;

        public ICommand RemoveInvoiceItemsCommand
        {
            get { return _removeInvoiceItemsCommand ?? (_removeInvoiceItemsCommand = new RelayCommand<IList>(RemoveInvoiceItems)); }
        }

        #region Set invoice item command

        private ICommand _setInvoiceItemCommand;

        public ICommand SetInvoiceItemCommand
        {
            get { return _setInvoiceItemCommand ?? (_setInvoiceItemCommand = new RelayCommand<string>(OnSetInvoiceItem)); }
        }

        protected virtual void OnSetInvoiceItem(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            if (code.Length == 16)
            {
                SetPartnerCardNumber(code);
                Code = "";
            }
            else
            {
                SetInvoiceItem(code);
                if (InvoiceItem != null && InvoiceItem.Product != null) PreviewAddInvoiceItem(null);
            }
        }
        #endregion Set invoice item command

        public ICommand AddInvoiceItemCommand
        {
            get { return new RelayCommand(PreviewAddInvoiceItem, CanAddInvoiceItem); }
        }

        public ICommand SaveInvoiceCommand
        {
            get { return new RelayCommand(OnSaveInvoice, CanSaveInvoice); }
        }

        private ICommand _removeInvoiceCommand;
        public ICommand RemoveInvoiceCommand { get { return _removeInvoiceCommand ?? (_removeInvoiceCommand = new RelayCommand(OnRemoveInvoice, CanRemoveInvoice)); } }

        private bool CanRemoveInvoice(object obj)
        {
            return ApplicationManager.IsInRole(UserRoleEnum.Seller) && !Invoice.IsApproved;
        }

        private void OnRemoveInvoice(object obj)
        {
            if (InvoicesManager.RemoveInvoice(Invoice.Id))
            {
                IsModified = false;
                Close();
            }
        }

        public ICommand PrintInvoiceItemCommand { get; private set; }

        public ICommand ApproveInvoiceCommand
        {
            get { return new RelayCommand(OnApprove, CanApprove); }
        }

        #region Add Items From Invoice Command

        private ICommand _addItemsFromInvoiceCommand;

        public ICommand AddItemsFromInvoiceCommand
        {
            get { return _addItemsFromInvoiceCommand ?? (_addItemsFromInvoiceCommand = new RelayCommand(OnAddItemsFromInvoice, CanAddItemsFromInvoice)); }
        }

        private bool CanAddItemsFromInvoice(object o)
        {
            return Invoice.ApproveDate == null;
        }
        #endregion Add Items From Invoice Command

        public ICommand AddItemsFromStocksCommand { get; private set; }
        public ICommand ApproveInvoiceAndCloseCommand { get; private set; }

        public ICommand GetProductCommand { get; private set; }
        public ICommand CleanInvoiceIemsCommand { get; private set; }

        [XmlIgnore]
        public ICommand ImportInvoiceCommand { get; private set; }

        #region Partner commands

        private ICommand _getPartnerCommand;

        public ICommand GetPartnerCommand
        {
            get { return _getPartnerCommand ?? (_getPartnerCommand = new RelayCommand<PartnerType>(OnGetPartner, CanGetPartner)); }
        }

        private bool CanGetPartner(PartnerType partnerType)
        {
            return !Invoice.IsApproved;
        }

        protected virtual void OnGetPartner(PartnerType partnerType)
        {
            var partners = PartnersManager.GetPartner(partnerType);
            SetPrtners(partners);
        }

        private ICommand _findPartnerCommand;
        private string _partnerFilter;

        public ICommand FindPartnerCommand
        {
            get { return _findPartnerCommand ?? (_findPartnerCommand = new RelayCommand<string>(OnFindPartner, CanFindPartner)); }
        }

        private bool CanFindPartner(string filter)
        {
            return true; //!string.IsNullOrEmpty(PartnerFilter);
        }

        private void OnFindPartner(string filter)
        {
            var partners = PartnersManager.GetPartners().Where(s => string.IsNullOrEmpty(filter) || s.FullName.ToLower().Contains(filter.ToLower()) || (!string.IsNullOrEmpty(s.Mobile) && s.Mobile.ToLower().Contains(filter.ToLower()) || (!string.IsNullOrEmpty(s.Email) && s.Email.ToLower().Contains(filter.ToLower())) || (!string.IsNullOrEmpty(s.CardNumber) && s.CardNumber.ToLower().Contains(filter.ToLower())))).ToList();
            if (SetPrtners(partners)) PartnerFilter = string.Empty;
        }

        private bool SetPrtners(List<PartnerModel> partners)
        {
            var partner = SelectItemsManager.SelectPartners(partners, false).FirstOrDefault();
            if (partner == null) return false;
            Partner = Invoice.Partner = partner;
            RaisePropertyChanged("Description");
            return true;
        }

        #endregion Partner commands

        #region Paid command

        private ICommand _paidInvoiceCommand;

        public ICommand PaidInvoiceCommand
        {
            get { return _paidInvoiceCommand ?? (_paidInvoiceCommand = new RelayCommand(OnPaidInvoice, CanPaidInvoice)); }
        }

        private bool CanPaidInvoice(object obj)
        {
            return !Invoice.IsApproved;
        }

        protected virtual void OnPaidInvoice(object obj)
        {
            InvoicePaid.Paid = Invoice.Total;
        }

        #endregion Paid command

        #endregion Commands

        public delegate void ShowProductsPane();

        public event ShowProductsPane ShowProducts;

        public void OnProductChanged(ProductModel product)
        {
            if (Invoice.IsApproved || product == null) return;
            if (InvoiceItem != null && InvoiceItem.ProductId == product.Id) InvoiceItemProductChanged(InvoiceItem, product);
            RaisePropertyChanged(() => InvoiceItem);
            foreach (var invoiceItem in InvoiceItems.Where(ii => ii.ProductId == product.Id))
            {
                InvoiceItemProductChanged(invoiceItem, product);
            }
            RaisePropertyChanged(() => InvoiceItems);
        }
        protected virtual void InvoiceItemProductChanged(InvoiceItemsModel invoiceItem, ProductModel product)
        {
            invoiceItem.Product = product;
            invoiceItem.Description = product.Description;
            invoiceItem.Code = product.Code;
        }
    }
}