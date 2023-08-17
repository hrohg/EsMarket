using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CashReg.Helper;
using CashReg.Interfaces;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using CashDeskManager = UserControls.Managers.CashDeskManager;
using ProductModel = ES.Data.Models.Products.ProductModel;
using SelectItemsManager = ES.Business.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    /// <summary>
    /// ReturnFromInvoiceViewModel
    /// </summary>
    public class ReturnFromInvoiceViewModel : PurchaseInvoiceViewModel
    {
        #region Internal properties
        private PartnerModel _buyer;
        #endregion Internal properties

        #region External properties
        public bool CanChangePartner { get { return !InvoiceItems.Any(); } }
        public PartnerModel Buyer { get { return _buyer ?? Partner; } set { _buyer = value; RaisePropertyChanged(() => Buyer); } }
        #endregion External properties

        #region Constructors

        public ReturnFromInvoiceViewModel()
            : base(InvoiceType.ReturnFrom) { }

        public ReturnFromInvoiceViewModel(Guid id)
            : base(id)
        {

        }

        #endregion Constructors

        #region Internal methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Title = "Ետ վերադարձ";
            AddBySingle = ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle;
            Buyer = Partner;
        }
        protected override void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnInvoiceItemsChanged(sender, e);
            RaisePropertyChanged("CanChangePartner");
        }
        protected override decimal GetProductPrice(ProductModel product)
        {
            return product != null ? product.Price ?? 0 : 0;

            //var price = (product != null) ? (product.Price ?? 0) * (product.Discount != null && product.Discount > 0 ?
            //1 - (product.Discount ?? 0) / 100 : 1 - (Partner != null && Partner.Discount != null ? Partner.Discount ?? 0 : 0) / 100) : 0;
            //price = Math.Max(price, product.DealerPrice ?? 0);
            //switch (Partner.PartnersTypeId)
            //{
            //    case (short)PartnerType.Dealer:
            //        return (product.HasDealerPrice ? product.DealerPrice : product.Price) ?? 0;
            //    default:
            //        return product.Price ?? 0;
            //}
        }
        protected override void OnPartnerChanged()
        {
            base.OnPartnerChanged();
            Invoice.RecipientName = Partner != null ? Partner.FullName : null;
            Invoice.Discount = Partner != null ? Partner.Discount : null;
            OnInvoiceItemsPropertyChanged(null, null);
            RaisePropertyChanged(() => Buyer);
        }
        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);

            decimal total = 0;
            foreach (InvoiceItemsModel invoiceItem in InvoiceItems)
            {
                decimal discount = 0;
                if (Partner != null && (Partner.PartnersTypeId != (short)PartnerType.Dealer || !invoiceItem.Product.HasDealerPrice))
                    discount = Invoice.Discount == 0 ? 0 : invoiceItem.Discount ?? Invoice.Discount ?? 0;

                var price = (invoiceItem.Price ?? 0) * (1 - discount / 100);
                total += price * (invoiceItem.Quantity ?? 0);
            }
            if (Partner != null) InvoicePaid.Paid = InvoicePaid.Total = Invoice.Total = total;
        }
        protected override void OnAddItemsFromInvoice(object o)
        {
            var items = InvoicesManager.GetInvoices(InvoicesManager.Convert(InvoiceTypeEnum.SaleInvoice), DateTime.Today.AddMonths(-1));
            if (items == null) return;
            items = items.Where(s => s.ApproveDate != null && Partner != null && s.PartnerId == Partner.Id).ToList();

            var invoice = Helpers.SelectItemsManager.SelectInvoice(items, OrderInvoiceBy.ApprovedDate, false).FirstOrDefault();
            AddItemsFromInvoice(invoice);
        }
        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
            if (Buyer == null || InvoiceItem == null) return;
            var lastSellProductItem = InvoicesManager.GetLastSellPrice(InvoiceItem.ProductId, Buyer.Id);
            if (lastSellProductItem != null)
            {
                InvoiceItem.Price = lastSellProductItem.Price;
                //InvoiceItem.Discount = lastSellProductItem.Discount;
            }
            else
            {
                if (InvoiceItem.Product != null) InvoiceItem.Price = InvoiceItem.Product.CostPrice;
                InvoiceItem.Discount = 0;
                MessageBox.Show(string.Format("Տվյալ ապրանքը ({0}) վերջին մեկ ամսվա վաճառքների մեջ չի հայտնաբերվել:", InvoiceItem.Description), "Սխալ ապրանք", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o)
                && InvoicePaid.IsPaid
                && InvoicePaid.Change == 0
                && Partner != null
                && (InvoicePaid.AccountsReceivable ?? 0) >= Partner.Debit - (Partner.MaxDebit ?? 0)
                && ApplicationManager.IsInRole(UserRoleEnum.SeniorSeller);
        }
        protected override void OnApprove(object o)
        {
            if (!CanApprove(o)) return;

            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.ProviderName = Partner.FullName;

            if (ToStock == null)
            {
                ToStock = SelectItemsManager.SelectStocks(StockManager.GetStocks(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSaleStocks.ToList()).ToList()).FirstOrDefault();

            }
            if (ToStock == null)
            {
                MessageManager.ShowMessage("Պահեստ ընտրված չէ: Խնդրում ենք խմբագրել նոր պահեստ:", "Գործողության ընդհատում", MessageBoxImage.Error);
                return;
            }
            Invoice.ToStockId = ToStock.Id;
            Invoice.RecipientName = ToStock.FullName;
            if (InvoicePaid.ByCash > 0)
            {
                var cashDesk = Helpers.SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks).SingleOrDefault();
                InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;
            }
            if (InvoicePaid.ByCheck > 0)
            {
                var bankAccount = Helpers.SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts).SingleOrDefault();
                InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
            }

            var invoice = InvoicesManager.ApproveInvoice(Invoice, InvoiceItems.ToList(), new List<StockModel> { ToStock }, InvoicePaid);
            if (invoice == null)
            {
                Invoice.AcceptDate = Invoice.ApproveDate = null;
                MessageManager.OnMessage("Գործողությունը ձախողվել է:", MessageTypeEnum.Warning);
                return;
            }
            Invoice = invoice;
            LoadInvoice();
            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
            MessageManager.OnMessage(string.Format("Ապրանքագիր {0} հաստատված է։", Invoice.InvoiceNumber), MessageTypeEnum.Success);
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            OnApprove(null);
        }
        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl =
                new PurchaseInvoiceLargeView(new PurchaseInvocieTicketViewModel(Invoice, InvoiceItems.ToList(),
                    StockManager.GetStock(Invoice.ToStockId), InvoicePaid));
            PrintManager.PrintPreview(ctrl, "Print purchase invoice", true);
        }

        protected override void OnImportInvoice(ExportImportEnum importFrom)
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
            ProductModel product;
            var invoice = ExcelImportManager.ImportSaleInvoice(filePath, true);
            if (invoice == null) return;
            var invoiceItems = invoice.Item2;
            foreach (var item in invoiceItems)
            {
                var products = ProductsManager.GetProductsByCodeOrBarcode(item.Code);
                product = Helpers.SelectItemsManager.SelectProduct(products).FirstOrDefault();
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
                    MessageManager.ShowMessage(string.Format("{0}({1}) ապրանքը գրանցված չէ:", item.Description, item.Code), "Անհայտ ապրանք");
                    return;
                }
                var exProducts = ProductsManager.GetProductsByCodeOrBarcode(product.Code);
                var exProduct = Helpers.SelectItemsManager.SelectProduct(exProducts).FirstOrDefault();
                InvoiceItem = new InvoiceItemsModel
                {
                    ProductId = product.Id,
                    Product = product,
                    Code = product.Code,
                    Description = product.Description,
                    //Price =  exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price:item.Price,
                    Discount = exProduct != null && exProduct.Price != null && exProduct.Price > 0 && exProduct.Price > item.Price ? Math.Round((decimal)(100 - 100 * item.Price / exProduct.Price), 2) : (decimal?)0.00,
                    Quantity = item.Quantity
                };
                if (InvoiceItem.Discount == 0) InvoiceItem.Discount = null;
                InvoiceItem.Price = exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price * (100 - (InvoiceItem.Discount ?? 0)) / 100 : item.Price;
                InvoiceItem.Quantity = item.Quantity;
                OnAddInvoiceItem();
            }
        }
        #endregion Internal methods

        #region External Methods


        #endregion

        #region Commands

        private ICommand _printEcrTicketCommand;
        public ICommand PrintEcrTicketCommand { get { return _printEcrTicketCommand ?? (_printEcrTicketCommand = new RelayCommand(OnPrintEcrTicket, CanPrintEcrTicket)); } }

        private bool CanPrintEcrTicket(object obj)
        {
            return InvoiceItems.Any();
        }

        private void OnPrintEcrTicket(object obj)
        {
            var ecrManager = EcrManager.EcrServer;
            var invoicePaid = new EcrPaid
            {
                PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                PartialAmount = 0,
                PrePaymentAmount = (double)(InvoicePaid.ReceivedPrepayment ?? 0),
                UseExtPos = false
            };
            ResponseReceiptModel responceReceiptViewModel = null;
            if (InvoiceItems.All(s => s.Quantity > 0))
            {
                var products = InvoiceItems.Select(s => new TotalModel
                {
                    rpid = InvoiceItems.IndexOf(s).ToString(),
                    adg = s.Product.HcdCs,
                    gc = s.Code,
                    gn = s.Description,
                    mu = s.Mu,
                    qty = (s.Quantity != null ? s.Quantity.ToString() : "0"),
                    p = s.Product != null && s.Product.Price != null ? s.Product.Price.ToString() : "0",
                    tt = ((s.Quantity ?? 0) * (s.Price ?? 0)).ToString(CultureInfo.InvariantCulture),

                    dsc = s.Discount.ToString(),
                    adsc = "0",
                    dsct = s.Discount != null && s.Discount > 0 ? "1" : null
                }).ToList();


                responceReceiptViewModel = ecrManager.PrintReceiptReturnTicket(products, null);
            }
            else
            {
                var returnItems = InvoiceItems.Where(s => s.Quantity > 0).Select(s => (IEcrReturnItem)(new ReturnItem(InvoiceItems.IndexOf(s), (double)(s.Quantity ?? 0)))).ToArray();
                ecrManager.PrintReceiptReturnTicket(returnItems, null);
            }
            var message = responceReceiptViewModel != null ?
                new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageTypeEnum.Success)
                : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageTypeEnum.Warning);
            MessageManager.OnMessage(message);
        }

        private ICommand _clearQuantitiesCpmmand;
        public ICommand ClearQuantitiesCommand { get { return _clearQuantitiesCpmmand ?? (_clearQuantitiesCpmmand = new RelayCommand(OnClearQuantities, CanClearQuanities)); } }

        private bool CanClearQuanities(object obj)
        {
            return InvoiceItems.Any(s => s.Quantity > 0);
        }

        private void OnClearQuantities(object obj)
        {
            foreach (var invoiceItemsModel in InvoiceItems)
            {
                invoiceItemsModel.Quantity = 0;
            }
        }
        private ICommand _getBuyerCommand;
        public ICommand GetBuyerCommand { get { return _getBuyerCommand ?? (_getBuyerCommand = new RelayCommand<PartnerType>(OnGetBuyer, CanGetBuyer)); } }
        private bool CanGetBuyer(PartnerType partnertype)
        {
            return !InvoiceItems.Any();
        }
        private void OnGetBuyer(PartnerType partnerType)
        {
            var buyers = PartnersManager.GetPartner(partnerType);
            Buyer = SelectItemsManager.SelectPartners(buyers, false, "Ընտրել գնորդ").FirstOrDefault();
        }
        #endregion Commands
    }

    /// <summary>
    /// ReturnToInvoiceViewModel
    /// </summary>
    public class ReturnToInvoiceViewModel : SaleInvoiceViewModel
    {
        #region Internal properties
        #endregion Internal properties

        #region External properties

        #endregion External properties

        #region Constructors

        public ReturnToInvoiceViewModel()
            : base(InvoiceType.ReturnTo)
        {

        }

        public ReturnToInvoiceViewModel(Guid id)
            : base(id)
        {

        }

        #endregion Constructors

        #region Internal methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
            Title = "Վերադարձ մատակարարին";
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            PreviewAddInvoiceItem(o);
        }
        protected override decimal GetProductPrice(ProductModel product)
        {
            return product != null ? (product.CostPrice ?? 0) : 0;
        }
        protected override void PreviewAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            if (!SetQuantity(AddBySingle))
            {
                return;
            }
            //var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());
            if (!Invoice.PartnerId.HasValue) return;
            if (InvoiceItem.Quantity != null && InvoiceItem.Quantity != 0)
            {
                base.PreviewAddInvoiceItem(o);
                return;
                var quantity = InvoiceItem.Quantity ?? 0;
                var saleInvoiceItems = InvoicesManager.GetPurchaseInvoiceByProductId(InvoiceItem.ProductId, (Guid)Invoice.PartnerId, quantity);
                foreach (var invoiceItem in saleInvoiceItems)
                {
                    InvoiceItem.Quantity = Math.Min(invoiceItem.Quantity ?? 0, quantity);
                    quantity -= InvoiceItem.Quantity.Value;
                    invoiceItem.Price = invoiceItem.Price;
                    invoiceItem.ProductItemId = invoiceItem.ProductItemId;
                    base.PreviewAddInvoiceItem(o);
                    if (quantity > 0)
                    {
                        InvoiceItem = new InvoiceItemsModel(Invoice, invoiceItem.Product);
                    }
                }
            }
        }
        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
            //var productItems = SelectItemsManager.SelectProductItems(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSaleStocks, true);
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (exCount == 0 || InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageManager.ShowMessage(message, "Անբավարար միջոցներ");
            }

            if ((InvoiceItem.Quantity == null || InvoiceItem.Quantity == 0))
            {
                if (addSingle && exCount >= 1)
                {
                    InvoiceItem.Quantity = 1;
                }
                else
                {
                    InvoiceItem.Quantity = GetAddedItemCount(exCount, false);
                }
            }

            return InvoiceItem.Quantity != null && InvoiceItem.Quantity > 0;
        }
        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);
            InvoicePaid.Paid = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));
        }
        protected override bool IsPaidValid()
        {
            return InvoicePaid.IsPaid &&
       InvoicePaid.Change <= (InvoicePaid.Paid ?? 0) &&
       Partner != null
       //&&(InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - Partner.Debit 
       //&&(InvoicePaid.ReceivedPrepayment ?? 0) <= Partner.Credit
       ;
        }
        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && ApplicationManager.IsInRole(UserRoleEnum.Manager);
        }

        protected override void OnApproveAsync(bool closeOnExit)
        {
            try
            {
                if (IsModified && !Save())
                {
                    IsLoading = false;
                    return;
                }

                IsLoading = true;
                PrepareToApprove();
                //Open Cash desk
                if (!string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort) || !string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveCashDeskPrinter) && InvoicePaid.ByCash > 0)
                {
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, CashDeskManager.OpenCashDesk);
                }
                var invoice = InvoicesManager.ApproveInvoice(Invoice, InvoiceItems.ToList(), FromStocks, InvoicePaid);
                if (invoice == null || Application.Current == null)
                {
                    Invoice.AcceptDate = Invoice.ApproveDate = null;
                    MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                    IsLoading = false;
                    return;
                }
                if (closeOnExit && invoice.ApproveDate != null && Application.Current != null)
                {
                    OnClose();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => ReloadApprovedInvoice(invoice)));
                }
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
            }
            IsLoading = false;
        }
        protected override void OnImportInvoice(ExportImportEnum importFrom)
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
            ProductModel product;
            var invoice = ExcelImportManager.ImportSaleInvoice(filePath);
            if (invoice == null) return;
            var invoiceItems = invoice.Item2;
            foreach (var item in invoiceItems)
            {
                var products = ProductsManager.GetProductsByCodeOrBarcode(item.Code);
                product = Helpers.SelectItemsManager.SelectProduct(products).FirstOrDefault();
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
                    MessageManager.ShowMessage(string.Format("{0}({1}) ապրանքը գրանցված չէ:", item.Description, item.Code), "Անհայտ ապրանք");
                    return;
                }
                var exProducts = ProductsManager.GetProductsByCodeOrBarcode(product.Code);
                var exProduct = Helpers.SelectItemsManager.SelectProduct(exProducts).FirstOrDefault();
                InvoiceItem = new InvoiceItemsModel
                {
                    ProductId = product.Id,
                    Product = product,
                    Code = product.Code,
                    Description = product.Description,
                    //Price =  exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price:item.Price,
                    Discount = exProduct != null && exProduct.Price != null && exProduct.Price > 0 && exProduct.Price > item.Price ? Math.Round((decimal)(100 - 100 * item.Price / exProduct.Price), 2) : (decimal?)0.00,
                    Quantity = item.Quantity
                };
                if (InvoiceItem.Discount == 0) InvoiceItem.Discount = null;
                InvoiceItem.Price = exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price * (100 - (InvoiceItem.Discount ?? 0)) / 100 : item.Price;
                OnAddInvoiceItem();
                InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
                RaisePropertyChanged("InvoicePaid");
            }
        }
        #endregion Internal methods

        #region External methods


        #endregion External methods
    }
}
