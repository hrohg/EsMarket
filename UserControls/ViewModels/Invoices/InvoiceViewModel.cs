using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Data.Enumerations;
using ES.Data.Model;
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
        /// <summary>
        /// Initalize a new instance of the ProductViewModel class.
        /// </summary>
        #region Properties
        private const string DenayChangePriceProperty = "DenayChangePrice";
        protected const string IsExpiringProperty = "IsExpiring";



        protected const string ShortTitleProperty = "ShortTitle";

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


        private InvoiceItemsModel _invoiceItem;

        private AccountsReceivableModel _accountsReceivable;
        private InvoicePaid _invoicePaid = new InvoicePaid();

        protected string ProductSearchKey;

        #endregion

        /// <summary>
        /// InvoiceViewModel public properties
        /// </summary>

        #region InvoiceViewModel External properties
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
                RaisePropertyChanged("IsLoading");
            }
        }

        public bool DenayChangePrice { get { return (_roles.FirstOrDefault(s => s.RoleName == "Manager" || s.RoleName == "Director") == null) || InvoiceItem.Product==null; } }

        public InvoiceItemsModel InvoiceItem
        {
            get { return _invoiceItem; }
            set { _invoiceItem = value; RaisePropertyChanged("InvoiceItem"); RaisePropertyChanged(IsExpiringProperty); }
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
            IsModified = true;
            Initialize();
        }
        public InvoiceViewModel(Guid id, EsUserModel user, EsMemberModel member)
        {
            User = user;
            Member = member;
            Invoice = InvoicesManager.GetInvoice(id, ApplicationManager.Instance.GetEsMember.Id);
            Initialize();
        }
        #endregion

        #region Invoiceview model private methods

        private void Initialize()
        {

            IsClosable = true;
            SaleBySingle = ApplicationManager.BuyBySingle;
            BuyBySingle = ApplicationManager.BuyBySingle;
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
            SetInvoiceItemCommand = new RelayCommand<string>(OnSetInvoiceItem);
            //PrintInvoiceItemCommand = new PrintInvoiceItemCommands(this);
            //ExportInvoiceCommand = new ExportInvoiceItemCommands(this);

            //ExportInvoiceToExcelCommand = new ExportInvoiceToExcelCommands(this);
            //ExportInvoiceToXmlCommand = new ExportInvoiceToXmlCommands(this);
            //ExportInvoiceToExcelRusCommand = new ExportInvoiceToExcelRusCommands(this);
            //ExportInvoiceToXmlRusCommand = new ExportInvoiceToXmlRusCommands(this);

            ApproveMoveInvoiceCommand = new ApproveMoveInvoiceCommands(this);
            AddItemsFromPurchaseInvoiceCommand = new AddItemsFromInvoiceCommands(this, InvoiceType.PurchaseInvoice);
            AddItemsFromMoveInvoiceCommand = new AddItemsFromInvoiceCommands(this, InvoiceType.MoveInvoice);
            AddItemsFromStocksCommand = new AddItemsFromStocksCommand(this, InvoiceType.MoveInvoice);
            ApproveInvoiceAndCloseCommand = new ApproveCloseInvoiceCommands(this);



            GetPartnerCommand = new RelayCommand(OnGetPartner);
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
            return product!=null?
                (product.Price ?? 0) * (product.Discount > 0 ? 
                1 - (product.Discount ?? 0) / 100 : 1 - (Partner.Discount ?? 0) / 100):0;
        }
        protected virtual bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemCount(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList(), Member.Id);
            InvoiceItem.Quantity = addSingle ? 1 : GetAddedItemCount(exCount, true);
            return InvoiceItem.Quantity != null && !(InvoiceItem.Quantity <= 0);
        }

        public delegate void ShowProductsPane();
        public event ShowProductsPane ShowProducts;
        #region Set Partner
        protected virtual void OnGetPartner(object o)
        {
            var partners = o is PartnerType ? PartnersManager.GetPartner(ApplicationManager.Instance.GetEsMember.Id, (PartnerType)o) : PartnersManager.GetPartners(ApplicationManager.Instance.GetEsMember.Id);
            if (partners.Count == 0) return;
            var selectedItems = new SelectItems(partners.Select(s => new ItemsToSelect { DisplayName = s.FullName + " " + s.Mobile, SelectedValue = s.Id }).ToList(), false);
            selectedItems.ShowDialog();
            if (selectedItems.DialogResult == null || selectedItems.DialogResult != true || selectedItems.SelectedItems == null) return;
            SetPartner(partners.FirstOrDefault(s => selectedItems.SelectedItems.Select(t => t.SelectedValue).ToList().Contains(s.Id)));
        }

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

        #endregion

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
        protected override void OnClose(object o)
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
            CreateNewInvoiceItem(productItem);
            OnAddInvoiceItem(InvoiceItem);
        }
        public virtual void OnSetInvoiceItem(string code)
        {
            SetInvoiceItem(code);
            OnAddInvoiceItem(null);
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
            InvoiceItem = new InvoiceItemsModel();
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
                InvoiceItem.Price = GetPartnerPrice(product);
                InvoiceItem.Discount = product.Discount;
                InvoiceItem.Note = product.Note;
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
            if (!CanAddInvoiceItem(o)) { return; }
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
                    ToStock = stock;
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
        #endregion External methods

        #region Commands
        public ICommand RemoveInvoiceItemCommand { get; private set; }

        public ICommand SetInvoiceItemCommand { get; private set; }
        public ICommand AddInvoiceItemCommand { get { return new RelayCommand(OnAddInvoiceItem, CanAddInvoiceItem); } }
        public ICommand SaveInvoiceCommand { get { return new RelayCommand(OnSaveInvoice, CanSaveInvoice); } }
        public ICommand PrintInvoiceItemCommand { get; private set; }
        public ICommand ApproveInvoiceCommand { get { return new RelayCommand(OnApprove, CanApprove); } }

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

        public ICommand GetPartnerCommand { get; private set; }
        public ICommand GetProductCommand { get; private set; }
        public ICommand CleanInvoiceIemsCommand { get; private set; }
        #endregion Commands
    }
}
