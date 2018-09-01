using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
using ReceiptTicketViewModel = UserControls.Views.ReceiptTickets.SaleInvocieSmallTicketViewModel;
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
                base.IsModified = value;
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

        public bool DenayChangePrice
        {
            get { return (!ApplicationManager.IsInRole(UserRoleEnum.Admin) && !ApplicationManager.IsInRole(UserRoleEnum.Manager)) || InvoiceItem.Product == null; }
        }

        public InvoiceItemsModel InvoiceItem
        {
            get { return _invoiceItem; }
            set
            {
                _invoiceItem = value;
                Code = InvoiceItem.Code;
                RaisePropertyChanged("InvoiceItem");
                RaisePropertyChanged("DenayChangePrice");
                RaisePropertyChanged(IsExpiringProperty);
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
            get { return InvoicePaid.Total.HasValue && InvoicePaid.Total != 0 ? InvoiceProfit * 100 / InvoicePaid.Total.Value : 100; }
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
            Invoice = new InvoiceModel(User, Member) { InvoiceTypeId = (int)type };
            Initialize();
        }

        protected InvoiceViewModel(Guid id)
        {
            Invoice = InvoicesManager.GetInvoice(id);
            Initialize();
        }

        #endregion Constructors

        #region Invoice view model private methods

        private void Initialize()
        {
            IsClosable = true;
            CanFloat = true;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            if (Invoice.Partner == null) SetDefaultPartner();
            InvoiceItem = new InvoiceItemsModel(Invoice);
            SetICommands();
            IsModified = false;
        }

        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);

            InvoicePaid.Total = Invoice.Total = InvoiceItems.Sum(s => s.Amount);
            Invoice.Amount = InvoiceItems.Sum(s => (s.Product.Price ?? 0) * (s.Quantity ?? 0));
            RaisePropertyChanged("ProductQuantity");
            RaisePropertyChanged("ActualPercentage");
            RaisePropertyChanged("InvoiceProfit");
            RaisePropertyChanged("ProductCount");
        }

        private void SetICommands()
        {
            //ICommands
            RemoveInvoiceItemCommand = new RelayCommand(RemoveInvoiceItem, CanRemoveInvoiceItem);

            AddItemsFromStocksCommand = new RelayCommand(OnAddItemsFromStocks, CanAddItemsFromStocks);
            ApproveInvoiceAndCloseCommand = new RelayCommand(OnApproveAndClose, CanApprove);

            GetProductCommand = new RelayCommand(OnGetProduct);

            CleanInvoiceIemsCommand = new RelayCommand(OnCleanInvoiceItems, CanCleanInvoiceItems);
            ImportInvoiceCommand = new RelayCommand<ExportImportEnum>(OnImportInvoice, CanImportInvoice);

        }

        private void SetDefaultPartner()
        {
            Invoice.Partner = Partner = PartnersManager.GetDefaultParnerByInvoiceType((InvoiceType)Invoice.InvoiceTypeId)??PartnersManager.GetDefaultPartner(PartnerType.None);
        }

        /// <summary>
        /// Returns product price accending by discount.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        protected virtual decimal GetProductPrice(EsProductModel product)
        {
            return (product != null) ? (product.Price ?? 0) * (1 - (product.Discount ?? 0) / 100 ): 0;
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
            foreach (var invoiceItemsModel in InvoiceItems.Where(invoiceItemsModel => invoiceItemsModel.Index > invoiceItem.Index))
            {
                invoiceItemsModel.Index--;
            }
            InvoiceItems.Remove(invoiceItem);
        }

        public delegate void ShowProductsPane();

        public event ShowProductsPane ShowProducts;

        protected virtual void OnGetProduct(object o)
        {
            var products = ApplicationManager.Instance.CashProvider.Products.OrderBy(s => s.Description);
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
                            var product = new ProductsManager().GetProductsByCodeOrBarcode(invoiceItem.Code);
                            if (product == null)
                            {
                                MessageBox.Show(
                                    invoiceItem.Code +
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException("importFrom", importFrom, null);
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

        protected virtual void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            var exInvocieItem = InvoiceItems.FirstOrDefault(s => s.ProductId == InvoiceItem.ProductId && s.ProductItemId == InvoiceItem.ProductItemId && s.Price == InvoiceItem.Price);
            if (exInvocieItem != null)
            {
                exInvocieItem.Quantity += InvoiceItem.Quantity;
                SelectedInvoiceItem = exInvocieItem;
            }
            else
            {
                InvoiceItem.Index = InvoiceItems.Count + 1;
                InvoiceItems.Insert(0, InvoiceItem);
                SelectedInvoiceItem = InvoiceItem;
            }

            InvoiceItem = new InvoiceItemsModel(Invoice);
            IsModified = true;
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
                Mu = productItem.Product.Mu,
                Price = GetProductPrice(productItem.Product),
                Discount = productItem.Product.Discount,
                Note = productItem.Product.Note
            };
            RaisePropertyChanged(IsExpiringProperty);
            RaisePropertyChanged("InvoiceItem");
        }

        protected bool Save()
        {
            if (!CanSaveInvoice(null))
            {
                MessageManager.OnMessage("Գրանցումը հնարավոր չէ իրականացնել:Գործողության ընդհատում:", MessageTypeEnum.Warning);
                return false;
            }
            if (!InvoicesManager.SaveInvoice(Invoice, InvoiceItems.ToList()))
            {
                MessageManager.OnMessage("Գրանցումը ձախողվել է:Գործողության ընդհատում:", MessageTypeEnum.Warning);
                return false;
            }
            //Invoice.InvoiceNumber = InvoicesManager.GetInvoiceNumber(Invoice.Id, Invoice.MemberId);
            IsModified = false;
            return true;
        }

        protected virtual bool CanApprove(object o)
        {
            return Invoice.ApproveDate == null && InvoiceItems.Any();
        }

        protected abstract void SetPrice();

        protected abstract void OnApprove(object o);
        protected abstract void OnApproveAsync(bool closeOnExit);

        protected void ReloadApprovedInvoice(InvoiceModel invoice)
        {
            Invoice = invoice;
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index));

            AccountsReceivable = new AccountsReceivableModel(Invoice.Id, Partner.Id, User.UserId, Invoice.MemberId, true);
            ApplicationManager.Instance.CashManager.UpdatePartners();
            Partner = ApplicationManager.Instance.CashManager.GetPartners.SingleOrDefault(s => s.Id == Partner.Id);

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
                MessageManager.OnMessage(new MessageModel(DateTime.Now, "Գործողության ընդհատում: Գործողությունը հնարավոր չէ իրականացնել:", MessageTypeEnum.Warning));

            }
        }
        #endregion Internal methods

        #region External methods

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

        protected virtual InvoiceItemsModel GetInvoiceItem(string code)
        {
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code);
            decimal count = 0;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                new ProductsManager().GetProductsByCodeOrBarcode(code.Substring(2, 5));
                count = HgConvert.ToDecimal((code.Substring(7, 5))) / 1000;
            }

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
                Price = product.CostPrice,
                Discount = product.Discount,
                Note = product.Note
            };
        }

        public void OnSetProductItem(ProductItemModel productItem)
        {
            if (!IsSelected) return;
            CreateNewInvoiceItem(productItem);
            OnAddInvoiceItem(InvoiceItem);
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
                MessageBox.Show(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", productId), "Սխալ կոդ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            RaisePropertyChanged("InvoiceItem");
            RaisePropertyChanged(IsExpiringProperty);
        }

        public virtual void SetInvoiceItem(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return;
            }
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code);
            decimal? count = null;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                product = new ProductsManager().GetProductsByCodeOrBarcode(code.Substring(2, 5));
                count = HgConvert.ToDecimal(code.Substring(7, 5)) / 1000;
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
                MessageBox.Show(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code), "Սխալ կոդ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            RaisePropertyChanged("InvoiceItem");
            RaisePropertyChanged(IsExpiringProperty);
        }

        public void AddInvoiceItem(InvoiceItemsModel invocieItem)
        {
            lock (_sync)
            {
                InvoiceItems.Add(invocieItem);
            }
        }

        public bool CanEdit
        {
            get { return Invoice != null; }
        }

        public virtual bool CanAddInvoiceItem(object o)
        {
            return Invoice.ApproveDate == null && InvoiceItem.Product != null && InvoiceItem.Code == InvoiceItem.Product.Code && !IsLoading;
        }



        public virtual bool CanRemoveInvoiceItem(object o)
        {
            return Invoice.ApproveDate == null && SelectedInvoiceItem != null && (ApplicationManager.Instance.UserRoles.Any(s => s.Id == (int)EsSettingsManager.MemberRoles.SeniorSeller || s.Id == (int)EsSettingsManager.MemberRoles.Manager));
        }

        public virtual void RemoveInvoiceItem(object o)
        {
            var index = SelectedInvoiceItem.Index;
            RemoveInvoiceItem(SelectedInvoiceItem);
            index--;
            if (index < 0) index = 0;
            if (index >= InvoiceItems.Count) index = InvoiceItems.Count - 1;
            if (index < 0) return;
            SelectedInvoiceItem = InvoiceItems[index];
        }

        public virtual void RemoveInvoiceItems(IList invoiceItems)
        {
            var items = invoiceItems.Cast<InvoiceItemsModel>().ToList();
            int index = 0;
            foreach (var invoiceItem in items)
            {
                index = SelectedInvoiceItem.Index;
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
            return Invoice.ApproveDate == null && InvoiceItems.Count != 0 && IsModified;
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
            lock (_sync)
            {
                foreach (var item in t.Item2)
                {
                    InvoiceItems.Add(item);
                }
            }
            Invoice.Total = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
            return true;
        }

        public virtual void OnApproveAndClose(object o)
        {
            if (!CanApprove(o))
            {
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void ApproveMoveInvoice()
        {
            if (!CanApproveMoveInvoice()) return;
            Invoice.ApproverId = User.UserId;
            Invoice.ApproveDate = DateTime.Now;
            Invoice.Approver = User.FullName;
            Invoice.ProviderName = FromStock.FullName;
            Invoice.RecipientName = ToStock.FullName;
            if (InvoicesManager.ApproveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems), null) == null)
            {
                Invoice.ApproveDate = null;
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Invoice = InvoicesManager.GetInvoice(Invoice.Id);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index));
            IsModified = false;
            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
        }

        #region AddItemsFromStocksCommands

        public bool CanAddItemsFromStocks(object o)
        {
            return Invoice.ApproveDate == null;
        }

        public void OnAddItemsFromStocks(object o)
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(), false).FirstOrDefault();
            if (stock == null) return;
            var invoiceItems = SelectItemsManager.SelectProductItemsFromStock(new List<long> { stock.Id });
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
            if (IsModified)
            {
                var result = MessageBox.Show("Ապրանքագիրը փոփոխված է։ Դուք ցանկանու՞մ եք պահպանել փոփոխությունները։", "Պահպանել", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
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
            SetInvoiceItem(code);
            if (InvoiceItem != null && InvoiceItem.Product != null) OnAddInvoiceItem(null);
        }

        #endregion Set invoice item command

        public ICommand AddInvoiceItemCommand
        {
            get { return new RelayCommand(OnAddInvoiceItem, CanAddInvoiceItem); }
        }

        public ICommand SaveInvoiceCommand
        {
            get { return new RelayCommand(OnSaveInvoice, CanSaveInvoice); }
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
            get { return _addItemsFromInvoiceCommand ?? (_addItemsFromInvoiceCommand = new RelayCommand<InvoiceTypeEnum?>(OnAddItemsFromInvoice, CanAddItemsFromInvoice)); }
        }

        private bool CanAddItemsFromInvoice(InvoiceTypeEnum? type)
        {
            return Invoice.ApproveDate == null;
        }

        private void OnAddItemsFromInvoice(InvoiceTypeEnum? type)
        {
            if (!type.HasValue) return;
            var invoice = SelectItemsManager.SelectInvoice((long)type.Value).FirstOrDefault();
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
            var partners = PartnersManager.GetPartners().Where(s => string.IsNullOrEmpty(filter) || s.FullName.ToLower().Contains(filter.ToLower()) || (!string.IsNullOrEmpty(s.Mobile) && s.Mobile.ToLower().Contains(filter.ToLower()) || (!string.IsNullOrEmpty(s.Email) && s.Email.ToLower().Contains(filter.ToLower())) || (!string.IsNullOrEmpty(s.ClubSixteenId) && s.ClubSixteenId.ToLower().Contains(filter.ToLower())))).ToList();
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
    }
}
