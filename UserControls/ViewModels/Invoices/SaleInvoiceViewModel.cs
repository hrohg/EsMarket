using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CashReg;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Data.Models;
using ES.Business.Managers;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using CashDeskManager = UserControls.Managers.CashDeskManager;
using InvoicePaid = CashReg.Helper.InvoicePaid;
using ProductModel = ES.Business.Models.ProductModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    [Serializable]
    public class SaleInvoiceViewModel : InvoiceViewModel
    {
        #region Internal properties
        #endregion

        #region External properties

        public override string Title
        {
            get { return string.Format("Վաճառք{0}", Invoice != null && !string.IsNullOrEmpty(Invoice.InvoiceNumber) ? string.Format(" - {0}", Invoice.InvoiceNumber) : string.Empty); }
        }
        public override string Description
        {
            get { return string.Format("{0}{1}", Title, Partner != null ? string.Format(" ({0})", Partner.FullName) : string.Empty); }
        }
        public override PartnerModel Partner
        {
            get { return base.Partner; }
            set
            {
                Invoice.RecipientName = value != null ? value.FullName : null;
                base.Partner = value;
                SetPrice();
            }
        }

        #region IsEcrActivated
        private bool _isEcrActivated;
        public bool IsEcrActivated
        {
            get
            {
                return _isEcrActivated;
            }
            set
            {
                if (value == _isEcrActivated) return;
                _isEcrActivated = value;
                RaisePropertyChanged("IsEcrActivated");
                RaisePropertyChanged("EcrButtonTooltip");
            }
        }
        public string EcrButtonTooltip { get { return IsEcrActivated ? "ՀԴՄ ակտիվ է" : "ՀԴՄ պասիվ է"; } }

        #endregion IsEcrActivated

        #region IsPrintTicket
        public bool CanPrintTicket
        {
            get
            {
                return !string.IsNullOrEmpty(ApplicationManager.Settings.MemberSettings.ActiveSalePrinter);
            }
        }
        private bool _isPrintTicket;
        public bool IsPrintTicket
        {
            get
            {
                return _isPrintTicket;
            }
            set
            {
                if (value == _isPrintTicket) return;
                _isPrintTicket = value;
                RaisePropertyChanged("IsPrintTicket");
                RaisePropertyChanged("SalePrinterButtonTooltip");
                ApplicationManager.Settings.MemberSettings.IsPrintSaleTicket = value;
            }
        }
        public string SalePrinterButtonTooltip { get { return IsPrintTicket ? "Վաճառքի տպիչն ակտիվ է:" : "Վաճառքի տպիչը պասիվ է:"; } }

        #endregion IsPrintTicket

        public bool UseExtPos
        {
            get { return _useExtPos; }
            set
            {
                _useExtPos = value;
                RaisePropertyChanged("UseExtPos");
                RaisePropertyChanged("PosButtonTooltip");
            }
        }
        public string PosButtonTooltip
        {
            get
            {
                return UseExtPos ? "Օգտագործել արտաքին POS տերմինալ" : "Օգտագործել ներքին POS տերմինալ";
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceViewModel()
        {
            Initialize();
        }
        public SaleInvoiceViewModel(Guid id)
            : base(id)
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            FromStocks = StockManager.GetStocks(ApplicationManager.Settings.MemberSettings.ActiveSaleStocks.ToList()).ToList();
            Invoice.InvoiceTypeId = (int)InvoiceType.SaleInvoice;
            if (Partner == null)
            {
                var provideDefault = ApplicationManager.Instance.CashProvider.GetEsDefaults(DefaultControls.Customer);
                Partner = provideDefault == null ? ApplicationManager.Instance.CashProvider.GetPartners.FirstOrDefault() : ApplicationManager.Instance.CashProvider.GetPartners.FirstOrDefault(s => s.Id == provideDefault.ValueInGuid);
            } IsModified = false;
            AddBySingle = ApplicationManager.Settings.MemberSettings.SaleBySingle;
            PrintEcrTicketCommand = new RelayCommand(OnPrintEcrTicket, CanPrintEcrTicket);
            IsPrintTicket = ApplicationManager.Settings.MemberSettings.IsPrintSaleTicket;
            IsEcrActivated = ApplicationManager.Settings.MemberSettings.IsEcrActivated;
        }

        protected override void SetPrice()
        {
            foreach (var item in InvoiceItems)
            {
                item.Price = GetPartnerPrice(item.Product);
            }
            RaisePropertyChanged("InvoiceItems");
            if (InvoiceItem != null)
            {
                InvoiceItem.Price = GetPartnerPrice(InvoiceItem.Product);
            }
        }
        private void PrintInvoiceTicket(ResponseReceiptModel ecrResponseReceiptModel)
        {
            var ticket = new ReceiptTicketSmall(new SaleInvocieSmallTicketViewModel(ecrResponseReceiptModel)
            {
                Invoice = Invoice,
                InvoiceItems = InvoiceItems.ToList(),
                InvoicePaid = InvoicePaid
            });
            //new UiPrintPreview(ticket).ShowDialog();
            PrintManager.PrintDataGrid(ticket, ApplicationManager.Settings.MemberSettings.ActiveSalePrinter);
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (exCount == 0 || InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageBox.Show(message, "Անբավարար միջոցներ");
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

        protected override void OnPaidInvoice(object obj)
        {
            CashDeskManager.OpenCashDesk();
            base.OnPaidInvoice(obj);
        }
        private bool CanPrintEcrTicket(object o)
        {
            return string.IsNullOrEmpty(Invoice.RecipientTaxRegistration) && IsEcrActivated;
        }
        private void OnPrintEcrTicket(object o)
        {
            var ecrManager = new EcrServer(ApplicationManager.Settings.MemberSettings.EcrConfig);
            var products = InvoiceItems.Select(s => new CashReg.Helper.Product(s.Code, s.Description, s.Mu)
            {
                Price = s.Product.Price ?? 0,
                Qty = s.Quantity ?? 0,
                Discount = s.Discount,
                DiscountType = s.Discount != null && s.Discount > 0 ? 1 : (int?)null,
                AdgCode = s.Product.HcdCs
            }).ToList();
            var invoicePaid = new InvoicePaid
            {
                PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                PartialAmount = 0,
                PrePaymentAmount = 0
            };
            var responceReceiptViewModel = ecrManager.PrintReceipt(products, invoicePaid);
            var message = responceReceiptViewModel != null ?
                new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageTypeEnum.Success)
                : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageTypeEnum.Warning);
            MessageManager.OnMessage(message);
        }

        protected override void OnImportInvoice(ExportImportEnum importFrom)
        {
            InvoiceItems.Clear();
            var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ApplicationManager.Settings.MemberSettings.ImportingFilePath);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                IsLoading = false;
                return;
            }
            var memberSettings = ApplicationManager.Settings.MemberSettings;
            memberSettings.ImportingFilePath = Path.GetDirectoryName(filePath);
            MemberSettings.Save(ApplicationManager.Settings.MemberSettings, memberSettings.MemberId);
            ProductModel product;
            var invoice = ExcelImportManager.ImportSaleInvoice(filePath);
            if (invoice == null) return;
            var invoiceItems = invoice.Item2;
            foreach (var item in invoiceItems)
            {
                product = new ProductsManager().GetProductsByCodeOrBarcode(item.Code, Member.Id);
                if (product == null)
                {
                    product = new ProductModel(item.Code, Member.Id, User.UserId, true)
                    {
                        Id = item.ProductId,
                        Description = item.Description,
                        Mu = item.Mu,
                        HcdCs = item.Product != null ? item.Product.HcdCs : null,
                        Price = item.Price
                    };
                    var result =
                        MessageBox.Show(
                            string.Format("{0}({1}) ապրանքը գրանցված չէ: Ցանկանու՞մ եք գրանցել:", item.Description,
                                item.Code), "Ապրանքի գրանցում", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result != MessageBoxResult.Yes && result != MessageBoxResult.No)
                    {
                        return;
                    }

                    if (result == MessageBoxResult.Yes)
                    {
                        product = new ProductsManager().EditProduct(product);
                    }
                }
                var exProduct = new ProductsManager().GetProductsByCodeOrBarcode(product.Code, Member.Id);
                InvoiceItem = new InvoiceItemsModel
                {
                    ProductId = product.Id,
                    Product = product,
                    Code = product.Code,
                    Description = product.Description,
                    Mu = product.Mu,
                    //Price =  exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price:item.Price,
                    Discount = exProduct != null && exProduct.Price != null && exProduct.Price > 0 && exProduct.Price > item.Price ? Math.Round((decimal)(100 - 100 * item.Price / exProduct.Price), 2) : (decimal?)0.00,
                    Quantity = item.Quantity
                };
                if (InvoiceItem.Discount == 0) InvoiceItem.Discount = null;
                InvoiceItem.Price = exProduct != null && exProduct.Price != null && exProduct.Price > 0 ? exProduct.Price * (100 - (InvoiceItem.Discount ?? 0)) / 100 : item.Price;
                base.OnAddInvoiceItem(null);
                InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
                RaisePropertyChanged("InvoicePaid");
            }
        }
        #endregion

        #region External methods

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new SaleInvoiceView(new SaleInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId), InvoicePaid, Member));

            switch (printSize)
            {
                case PrintModeEnum.Normal:
                    PrintManager.PrintPreview(ctrl, "Print invoices", true);
                    break;
                case PrintModeEnum.Small:
                    PrintInvoiceTicket(null);
                    break;
                case PrintModeEnum.Large:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printSize", printSize, null);
            }
        }

        private int count = 0;
        private bool _useExtPos;

        public override bool CanAddInvoiceItem(object o)
        {
            ++count;
            return base.CanAddInvoiceItem(o) && FromStocks != null && FromStocks.Any();
        }

        protected override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            if (!SetQuantity(AddBySingle))
            {
                return;
            }
            base.OnAddInvoiceItem(o);
            //InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
            //RaisePropertyChanged("InvoicePaid");
        }

        protected override decimal GetPartnerPrice(EsProductModel product)
        {
            decimal price = product != null ? (product.Price ?? 0) : 0;
            if (Partner == null || product == null) { return price; }

            price = base.GetPartnerPrice(product);
            var dealerPrice = (product.DealerPrice ?? 0) * (1 - (product.DealerDiscount ?? 0) / 100);
            switch (Partner.PartnersTypeId)
            {
                case (long)PartnerType.Dealer:
                    price = product.HasDealerPrice ? dealerPrice : price;
                    break;
                default:
                    if (product.HasDealerPrice) price = Math.Max(price, dealerPrice);
                    break;
            }
            return price;
        }
        private bool IsPaid
        {
            get { return InvoicePaid.IsPaid && InvoicePaid.Change <= (InvoicePaid.Paid ?? 0) && Partner != null && (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - Partner.Debit && (InvoicePaid.ReceivedPrepayment ?? 0) <= Partner.Credit; }
        }

        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && IsPaid;
        }

        protected override void OnApprove(object o)
        {
            if (!CanApprove(o))
            {
                MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                return;
            }

            IsLoading = true;
            PrepareToApprove();
            //Approve Invoice
            new Thread(()=>OnApproveAsync(false)).Start();
        }

        private void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;

            var cashDesk = InvoicePaid.Paid > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.MemberSettings.SaleCashDesks).SingleOrDefault() : null;
            InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;

            var bankAccount = InvoicePaid.ByCheck > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.MemberSettings.SaleBankAccounts).SingleOrDefault() : null;
            InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
        }
        private void OnApproveAsync(bool closeOnExit)
        {
            try
            {
                OnSaveInvoice(null);
                if (IsModified)
                {
                    IsLoading = false;
                    return;
                }

                IsLoading = true;
                if (!RegisterEcrTicket())
                {
                    IsLoading = false;
                    return;
                }

                var invoice = InvoicesManager.ApproveSaleInvoice(Invoice, InvoiceItems.ToList(), FromStocks.Select(s => s.Id).ToList(), InvoicePaid);
                if (invoice == null || Application.Current==null)
                {
                    Invoice.AcceptDate = Invoice.ApproveDate = null;
                    MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                    IsLoading = false;
                    return;
                }
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => ReloadApprovedInvoice(invoice)));

                //CheckForPrize(Invoice);

                if (closeOnExit && Invoice.ApproveDate != null && Application.Current != null)
                {
                   Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>  OnClose(null)));
                }
            }
            catch (Exception ex)
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error)));
                IsLoading = false;
            }
        }

        private void ReloadApprovedInvoice(InvoiceModel invoice)
        {
            Invoice = invoice;
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id).OrderBy(s => s.Index));

            AccountsReceivable = new AccountsReceivableModel(Invoice.Id, Partner.Id, User.UserId, Invoice.MemberId, true);
            ApplicationManager.CashManager.UpdatePartners();
            Partner = ApplicationManager.CashManager.GetPartners.SingleOrDefault(s => s.Id == Partner.Id);

            IsModified = false;
            IsLoading = false;

            RaisePropertyChanged("InvoiceStateImageState");
            RaisePropertyChanged("InvoiceStateTooltip");
        }
        private bool RegisterEcrTicket()
        {
            try
            {
                ResponseReceiptModel responceReceiptModel = null;
                if (CanPrintEcrTicket(null))
                {
                    //todo
                    EcrServer ecrManager = null;

                    ecrManager = new EcrServer(ApplicationManager.Settings.MemberSettings.EcrConfig);
                    responceReceiptModel = ecrManager.PrintReceipt(InvoiceItems.Select(s => new CashReg.Helper.Product(s.Code, s.Description, s.Mu)
                    {
                        Price = s.Product.Price ?? 0,
                        Qty = s.Quantity ?? 0,
                        Discount = s.Discount ?? 0,
                        DiscountType = s.Discount != null ? 1 : (int?)null,
                        AdgCode = s.Product.HcdCs
                    }).ToList(), new InvoicePaid
                    {
                        PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                        PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                        PartialAmount = 0,
                        PrePaymentAmount = InvoicePaid.Prepayment != null ? (double)InvoicePaid.Prepayment : 0,
                        UseExtPos = UseExtPos
                    });


                    if (responceReceiptModel != null)
                    {
                        Invoice.RecipientTaxRegistration = responceReceiptModel.Rseq.ToString();
                        //todo approve invoice
                        MessageManager.OnMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptModel.Rseq, MessageTypeEnum.Success));
                    }
                    else
                    {
                        MessageManager.OnMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումը ձախողվել է:  {0}", ecrManager != null ? string.Format("{0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode) : string.Empty), MessageTypeEnum.Warning));
                        return false;
                    }
                }

                //Open Cash desk
                if (!string.IsNullOrEmpty(ApplicationManager.Settings.MemberSettings.CashDeskPort) && InvoicePaid.Paid > 0)
                {
                    CashDeskManager.OpenCashDesk();
                }

                //Print ticket
                if (IsPrintTicket && (!IsEcrActivated || ApplicationManager.Settings.MemberSettings.EcrConfig.UseExternalPrinter) && Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => PrintInvoiceTicket(responceReceiptModel)));
                }
            }
            catch (Exception ex)
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error)));
                // ignored
                return false;
            }
            return true;
        }
        public override void OnApproveAndClose(object o)
        {
            if (!CanApprove(o))
            {
                MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                return;
            }

            IsLoading = true;
            PrepareToApprove();
            //Approve Invoice
            new Thread(() => OnApproveAsync(true)).Start();
        }

        #endregion

        #region Commands

        public ICommand PrintEcrTicketCommand { get; private set; }

        #endregion
    }
}
