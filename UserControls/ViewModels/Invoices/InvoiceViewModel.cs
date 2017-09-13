using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.DataAccess.Models;
using Shared.Helpers;
using UserControls.Commands;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets.Views;
using ExcelImportManager = ES.Business.ExcelManager.ExcelImportManager;
using ProductModel = ES.Business.Models.ProductModel;
using ReceiptTicketViewModel = UserControls.Views.ReceiptTickets.SaleInvocieSmallTicketViewModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    public class InvoiceViewModel : InvoiceViewModelBase
    {

        #region Properties
        private const string DenayChangePriceProperty = "DenayChangePrice";
        protected const string IsExpiringProperty = "IsExpiring";

        protected const string ShortTitleProperty = "ShortTitle";

        #endregion

        #region Invoice view model private properties
        private List<MembersRoles> _roles = new List<MembersRoles>();
        private InvoiceItemsModel _invoiceItem;
        private AccountsReceivableModel _accountsReceivable;
        private InvoicePaid _invoicePaid = new InvoicePaid();

        protected string ProductSearchKey;
        #endregion

        #region InvoiceViewModel External properties

        public override bool IsModified
        {
            get { return base.IsModified; }
            set
            {
                base.IsModified = value;
                DisposeTimer();
                if (IsModified)
                {
                    _timer = new Timer(TimerElapsed, null, 60000, 60000);
                }
            }
        }

        #region Is loading
        private bool _isLoading;
        public override bool IsLoading
        {
            get
            {
                return _isLoading;
            }
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
            get
            {
                return _code;
            }
            set
            {
                if (value == _code) return;
                _code = value;
                RaisePropertyChanged("Code");
            }
        }

        #endregion Code

        public bool DenayChangePrice { get { return (_roles.FirstOrDefault(s => s.RoleName == "Manager" || s.RoleName == "Director") == null) || InvoiceItem.Product == null; } }

        public InvoiceItemsModel InvoiceItem
        {
            get { return _invoiceItem; }
            set
            {
                _invoiceItem = value;
                Code = InvoiceItem.Code;
                RaisePropertyChanged("InvoiceItem");
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
            }
        }
        public Visibility IsExpiring
        {
            get
            {
                return InvoiceItem != null && InvoiceItem.Product != null && InvoiceItem.Product.ExpiryDays != null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public AccountsReceivableModel AccountsReceivable { get { return _accountsReceivable; } set { _accountsReceivable = value; } }
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; }
        }
        public bool MoveBySingle { get; set; }
        private bool _addBySingle;
        public bool AddBySingle
        {
            get { return _addBySingle; }
            set { _addBySingle = value; RaisePropertyChanged("AddBySingle"); RaisePropertyChanged("AddSingleTooltip"); }
        }

        public string AddSingleTooltip { get { return AddBySingle ? "Ավելացնել մեկական" : "Ավելացնել բազմակի"; } }
        public int ProductCount
        {
            get
            {
                return InvoiceItems.Count;
            }
        }
        public decimal ProductQuantity { get { return InvoiceItems.Sum(s => s.Quantity ?? 0); } }
        public decimal InvoiceProfit { get { return Invoice.Amount - Invoice.Total; } }
        public decimal ActualPercentage { get { return InvoicePaid.Total.HasValue && InvoicePaid.Total != 0 ? InvoiceProfit * 100 / InvoicePaid.Total.Value : 100; } }

        public string PartnerFilter
        {
            get { return _partnerFilter; }
            set { _partnerFilter = value; RaisePropertyChanged("PartnerFilter"); }
        }

        public ImageState InvoiceStateImageState { get { return ImageHelper.GetInvoiceStateImage(Invoice.IsApproved); } }
        public string InvoiceStateTooltip { get { return Invoice.IsApproved ? string.Format("Հաստատված է {0}", Invoice.ApproveDate) : "Հաստատված չէ"; } }

        public string Notes
        {
            get { return Invoice.Notes; }
            set
            {
                Invoice.Notes = value; RaisePropertyChanged("Notes");
                IsModified = true;
            }
        }
        #endregion

        #region Constructors
        public InvoiceViewModel()
        {
            Invoice = new InvoiceModel(User, Member);
            Initialize();
        }
        public InvoiceViewModel(Guid id)
        {
            Invoice = InvoicesManager.GetInvoice(id, ApplicationManager.Instance.GetMember.Id);
            Initialize();
        }
        #endregion

        #region Invoiceview model private methods

        private void Initialize()
        {
            IsClosable = true;
            SetModels();
            SetICommands();
            IsModified = false;
            CanFloat = true;
        }
        protected override void OnInvoiceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemPropertyChanged(sender, e);

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
            RemoveInvoiceItemCommand = new RemoveInvoiceItemCommands(this);


            //PrintInvoiceItemCommand = new PrintInvoiceItemCommands(this);
            //ExportInvoiceCommand = new ExportInvoiceItemCommands(this);

            //ExportInvoiceToExcelCommand = new ExportInvoiceToExcelCommands(this);
            //ExportInvoiceToXmlCommand = new ExportInvoiceToXmlCommands(this);
            //ExportInvoiceToExcelRusCommand = new ExportInvoiceToExcelRusCommands(this);
            //ExportInvoiceToXmlRusCommand = new ExportInvoiceToXmlRusCommands(this);

            ApproveMoveInvoiceCommand = new ApproveMoveInvoiceCommands(this);
            AddItemsFromStocksCommand = new AddItemsFromStocksCommand(this, InvoiceType.MoveInvoice);
            ApproveInvoiceAndCloseCommand = new ApproveCloseInvoiceCommands(this);

            GetProductCommand = new RelayCommand(OnGetProduct);

            CleanInvoiceIemsCommand = new RelayCommand(OnCleanInvoiceItems, CanCleanInvoiceItems);
        }
        private void SetModels()
        {
            InvoiceItem = new InvoiceItemsModel(Invoice);
            if (Partner == null)
            {
                SetDefaultPartner();
            }
            _roles = ApplicationManager.Instance.UserRoles; //UsersManager.GetUserRoles(_user.UserId, memberId: _member.Id);
            RaisePropertyChanged(DenayChangePriceProperty);
        }
        private void SetDefaultPartner()
        {
            Partner = PartnersManager.GetDefaultParnerByInvoiceType(Invoice.MemberId, (InvoiceType)Invoice.InvoiceTypeId);
            var partners = ApplicationManager.Instance.CashProvider.GetPartners;
            switch (Invoice.InvoiceTypeId)
            {
                //case (long)InvoiceType.SaleInvoice:
                //    var customerDefault = ApplicationManager.CashManager.GetEsDefaults(DefaultControls.Customer);
                //    Partner = customerDefault == null ? partners.FirstOrDefault() : partners.FirstOrDefault(s => s.Id == customerDefault.ValueInGuid);
                //    break;
                case (long)InvoiceType.PurchaseInvoice:
                    var provideDefault = ApplicationManager.Instance.CashProvider.GetEsDefaults(DefaultControls.Provider);
                    Partner = provideDefault == null ? partners.FirstOrDefault() : partners.FirstOrDefault(s => s.Id == provideDefault.ValueInGuid);
                    break;
                default:
                    return;
            }
            if (Partner == null) { Partner = partners.FirstOrDefault(); }
        }
        protected virtual decimal GetPartnerPrice(EsProductModel product)
        {
            return product != null ?
                (product.Price ?? 0) * (product.Discount > 0 ?
                1 - (product.Discount ?? 0) / 100 : 1 - (Partner.Discount ?? 0) / 100) : 0;
        }
        protected virtual bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());
            InvoiceItem.Quantity = addSingle ? 1 : GetAddedItemCount(exCount, true);
            return InvoiceItem.Quantity != null && !(InvoiceItem.Quantity <= 0);
        }

        public delegate void ShowProductsPane();
        public event ShowProductsPane ShowProducts;
        #region Set Partner
        public void SetPartner(PartnerModel partner)
        {
            Partner = partner;
            RaisePropertyChanged("Description");
        }
        #endregion Set Partner
        protected virtual void OnGetProduct(object o)
        {
            var products = ApplicationManager.Instance.CashProvider.Products.OrderBy(s => s.Description);
            var selectedItems =
                new SelectItems(products.Select(s => new ItemsToSelect { DisplayName = string.Format("{0} ({1} {2})", s.Description, s.Code, s.Price), SelectedValue = s.Id }).ToList(), false);
            selectedItems.SearchKey = o is FiltersUsage && ((FiltersUsage)o) == FiltersUsage.WithFilters ? ProductSearchKey : string.Empty;
            var product = (selectedItems.ShowDialog() == true && selectedItems.SelectedItems != null)
                ? products.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id))
                : null;
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
        Timer _timer = null;
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
            new Thread(AutoSave).Start();
            DisposeTimer();
        }
        private void AutoSave()
        {
            //InvoicesManager.AutoSave(Invoice, InvoiceItems.ToList());


            string filePath = PathHelper.GetMemberTempInvoiceFilePath(Invoice.Id, ApplicationManager.Member.Id);
            if (string.IsNullOrEmpty(filePath)) return;
            FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, (InvoiceViewModel)this);
            }
            catch
            {

            }
            finally
            {
                fs.Close();
            }
        }

        private void DeleteAutoSaveFile()
        {
            DisposeTimer();
            string filePath = PathHelper.GetMemberTempInvoiceFilePath(Invoice.Id, ApplicationManager.Member.Id);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;
            File.Delete(filePath);
        }
        #endregion Auto save
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
                Price = GetPartnerPrice(productItem.Product),
                Discount = productItem.Product.Discount,
                Note = productItem.Product.Note
            };
            RaisePropertyChanged(IsExpiringProperty);
            RaisePropertyChanged("InvoiceItem");
        }

        public void OnSetProductItem(ProductItemModel productItem)
        {
            if (!IsSelected) return;
            CreateNewInvoiceItem(productItem);
            OnAddInvoiceItem(InvoiceItem);
        }

        public virtual void SetInvoiceItem(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return;
            }
            var product = new ProductsManager().GetProductsByCodeOrBarcode(code, Member.Id);
            decimal? count = null;
            if (product == null && code.Substring(0, 1) == "2" && code.Length == 13)
            {
                product = new ProductsManager().GetProductsByCodeOrBarcode(code.Substring(2, 5), Member.Id);
                count = HgConvert.ToDecimal(code.Substring(7, 5)) / 1000;
            }
            InvoiceItem = new InvoiceItemsModel(Invoice, product);
            if (product != null)
            {
                InvoiceItem.Quantity = count;
                InvoiceItem.Price = GetPartnerPrice(product);
            }
            else
            {
                MessageManager.OnMessage(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code), MessageTypeEnum.Warning);
                MessageBox.Show(string.Format("{0} կոդով ապրանք չի հայտնաբերվել:", code), "Սխալ ապա կոդ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            RaisePropertyChanged("InvoiceItem");
            RaisePropertyChanged(IsExpiringProperty);
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
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            var exInvocieItem = InvoiceItems.FirstOrDefault(s => s.ProductId == InvoiceItem.ProductId && s.ProductItemId == null && s.Price == InvoiceItem.Price);
            if (exInvocieItem != null)
            {
                exInvocieItem.Quantity += InvoiceItem.Quantity;
                SelectedInvoiceItem = exInvocieItem;
            }
            else
            {
                InvoiceItem.Index = InvoiceItems.Count + 1;
                InvoiceItems.Insert(InvoiceItems.Count, InvoiceItem);
                SelectedInvoiceItem = InvoiceItem;
            }

            InvoiceItem = new InvoiceItemsModel(Invoice);
            IsModified = true;
        }

        public void ExportInvoice(bool isCostPrice = false, bool isPrice = true)
        {
            ExcelExportManager.ExportInvoice(Invoice, InvoiceItems);
        }

        public virtual bool CanRemoveInvoiceItem()
        {
            return Invoice.ApproveDate == null && SelectedInvoiceItem != null && (ApplicationManager.Instance.UserRoles.Any(s => s.Id == (int)EsSettingsManager.MemberRoles.SeniorSeller || s.Id == (int)EsSettingsManager.MemberRoles.Manager));
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
            return Invoice.ApproveDate == null && InvoiceItems.Count != 0 && IsModified;
        }

        public virtual void OnSaveInvoice(object o)
        {
            Save();
            DeleteAutoSaveFile();
        }

        private bool Save()
        {
            if (!CanSaveInvoice(null))
            {
                MessageManager.OnMessage("Գործողությունը հնարավոր չէ իրականացնել:Գործողության ընդհատում:", MessageTypeEnum.Warning);
                return false;
            }
            if (InvoicesManager.SaveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems)))
            {
                Invoice = InvoicesManager.GetInvoice(Invoice.Id, Invoice.MemberId);
                IsModified = false;
                MessageManager.OnMessage("Գրանցումն իրականացել է հաջողությամբ:", MessageTypeEnum.Success);
                return true;
            }
            MessageManager.OnMessage("Գործողությունը հնարավոր չէ իրականացնել:Գործողության ընդհատում:", MessageTypeEnum.Warning);
            return false;
        }
        public virtual bool CanApprove(object o)
        {
            return Invoice.ApproveDate == null && InvoiceItems.Count != 0;
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
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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
                RaisePropertyChanged("InvoiceStateImageState");
                RaisePropertyChanged("InvoiceStateTooltip");
            }
            else
            {
                Invoice.ApproverId = null;
                Invoice.Approver = null;
                Invoice.ApproveDate = null;
                MessageBox.Show("Գործողությունն իրականացման ժամանակ տեղի է ունեցել սխալ:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            if (InvoicesManager.ApproveInvoice(Invoice, new List<InvoiceItemsModel>(InvoiceItems)) == null)
            {
                Invoice.ApproveDate = null;
                MessageBox.Show("Գործողությունը հնարավոր չէ իրականացնել:", "Գործողության ընդհատում", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Invoice = InvoicesManager.GetInvoice(Invoice.Id, Invoice.MemberId);
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index));
            IsModified = false;
            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
        }

        private void CheckForPrize(InvoiceModel invoice)
        {
            if (!InvoicesManager.CheckForPrize(invoice)) return;
            MessageBox.Show(string.Format("Շնորհավորում ենք դուք շահել եք։ \n Ապրանքագիր։ {0} \n Ամսաթիվ։ {1} \n Պատվիրատու։ {2}", invoice.InvoiceNumber, invoice.ApproveDate, invoice.RecipientName), "Շահում", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        #region AddItemsFromStocksCommands

        public bool CanAddItemsFromStocks()
        {
            return Invoice.ApproveDate == null;
        }

        public void AddItemsFromStocks()
        {
            var stock = SelectItemsManager.SelectStocks(StockManager.GetStocks(Member.Id), false).FirstOrDefault();
            if (stock == null) return;
            var invoiceItems = SelectItemsManager.SelectProductItemsFromStock(new List<long> { stock.Id });
            if (invoiceItems == null) return;
            foreach (var ii in invoiceItems)
            {
                var newInvocieItem = GetInvoiceItem(ii.Code);
                if (newInvocieItem == null) continue;
                newInvocieItem.Quantity = ii.Quantity;
                InvoiceItems.Add(newInvocieItem);
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
            }
            return base.Close();
        }
        #endregion External methods

        #region Commands

        public ICommand RemoveInvoiceItemCommand { get; private set; }

        #region Set invoice item command

        private ICommand _setInvoiceItemCommand;

        public ICommand SetInvoiceItemCommand
        {
            get { return _setInvoiceItemCommand ?? (_setInvoiceItemCommand = new RelayCommand<string>(OnSetInvoiceItem)); }
        }

        protected virtual void OnSetInvoiceItem(string code)
        {
            SetInvoiceItem(code);
            OnAddInvoiceItem(null);
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

        public ICommand ExportInvoiceToExcelCommand { get; private set; }
        public ICommand ExportInvoiceToXmlCommand { get; private set; }
        public ICommand ExportInvoiceToExcelRusCommand { get; private set; }
        public ICommand ExportInvoiceToXmlRusCommand { get; private set; }
        public ICommand ApproveMoveInvoiceCommand { get; private set; }

        #region Add Items From Invoice Command

        private ICommand _addItemsFromInvoiceCommand;

        public ICommand AddItemsFromInvoiceCommand
        {
            get { return _addItemsFromInvoiceCommand ?? (_addItemsFromInvoiceCommand = new RelayCommand<InvoiceType?>(OnAddItemsFromInvoice, CanAddItemsFromInvoice)); }
        }

        private bool CanAddItemsFromInvoice(InvoiceType? type)
        {
            return Invoice.ApproveDate == null;
        }

        private void OnAddItemsFromInvoice(InvoiceType? type)
        {
            if (!type.HasValue) return;
            var invoice = SelectItemsManager.SelectInvoice((long)type.Value).FirstOrDefault();
            if (invoice == null) return;
            var invoiceItems = SelectItemsManager.SelectInvoiceItems(invoice.Id);
            if (invoiceItems == null) return;
            foreach (var ii in invoiceItems.ToList())
            {
                var newInvocieItem = GetInvoiceItem(ii.Code);
                if (newInvocieItem == null) continue;
                newInvocieItem.Quantity = ii.Quantity;
                InvoiceItems.Add(newInvocieItem);
            }
        }

        #endregion Add Items From Invoice Command

        public ICommand AddItemsFromStocksCommand { get; private set; }
        public ICommand ApproveInvoiceAndCloseCommand { get; private set; }


        public ICommand GetProductCommand { get; private set; }
        public ICommand CleanInvoiceIemsCommand { get; private set; }

        #region Partner commands

        private ICommand _getPartnerCommand;
        public ICommand GetPartnerCommand { get { return _getPartnerCommand ?? (_getPartnerCommand = new RelayCommand<PartnerType>(OnGetPartner, CanGetPartner)); } }

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
        public ICommand FindPartnerCommand { get { return _findPartnerCommand ?? (_findPartnerCommand = new RelayCommand<string>(OnFindPartner, CanFindPartner)); } }

        private bool CanFindPartner(string filter)
        {
            return true; //!string.IsNullOrEmpty(PartnerFilter);
        }

        private void OnFindPartner(string filter)
        {
            var partners = PartnersManager.GetPartners().Where(s => string.IsNullOrEmpty(filter) ||
                s.FullName.ToLower().Contains(filter.ToLower()) ||
                (!string.IsNullOrEmpty(s.Mobile) && s.Mobile.ToLower().Contains(filter.ToLower()) ||
                (!string.IsNullOrEmpty(s.Email) && s.Email.ToLower().Contains(filter.ToLower())) ||
                (!string.IsNullOrEmpty(s.ClubSixteenId) && s.ClubSixteenId.ToLower().Contains(filter.ToLower())
                ))).ToList();
            if (SetPrtners(partners)) PartnerFilter = string.Empty;
        }

        private bool SetPrtners(List<PartnerModel> partners)
        {
            var partner = SelectItemsManager.SelectPartners(partners, false).FirstOrDefault();
            if (partner == null) return false;
            SetPartner(partner);
            return true;
        }
        #endregion Partner commands

        #region Paid command

        private ICommand _paidInvoiceCommand;
        public ICommand PaidInvoiceCommand { get { return _paidInvoiceCommand ?? (_paidInvoiceCommand = new RelayCommand(OnPaidInvoice, CanPaidInvoice)); } }

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
