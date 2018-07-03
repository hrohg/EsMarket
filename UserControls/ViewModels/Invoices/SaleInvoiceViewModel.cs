using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CashReg;
using CashReg.Helper;
using CashReg.Interfaces;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Business.Helpers;
using ES.Data.Models;
using ES.Business.Managers;
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
using ProductModel = ES.Data.Models.ProductModel;
using SelectItemsManager = UserControls.Helpers.SelectItemsManager;

namespace UserControls.ViewModels.Invoices
{
    [Serializable]
    public class SaleInvoiceViewModel : InvoiceViewModel
    {
        #region Internal properties

        #endregion

        #region External properties

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

        public override bool AddBySingle
        {
            get { return base.AddBySingle; }
            set
            {
                base.AddBySingle = value;
                ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle = value;
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
                ApplicationManager.Settings.IsEcrActivated = value;
            }
        }
        public string EcrButtonTooltip { get { return IsEcrActivated ? "ՀԴՄ ակտիվ է" : "ՀԴՄ պասիվ է"; } }

        #endregion IsEcrActivated

        #region IsPrintTicket
        public bool CanPrintTicket
        {
            get
            {
                return !string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
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
                ApplicationManager.Settings.IsPrintSaleTicket = value;
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
                return UseExtPos ? "Օգտագործել արտաքին POS տերմինալ" : "Օգտագործել ՀԴՄ ներքին POS տերմինալ";
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceViewModel()
            : base(InvoiceType.SaleInvoice)
        {

        }
        public SaleInvoiceViewModel(InvoiceType type)
            : base(type)
        {

        }
        public SaleInvoiceViewModel(Guid id)
            : base(id)
        {

        }
        #endregion

        #region Internal methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
            FromStocks = StockManager.GetStocks(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSaleStocks.ToList()).ToList();
            AddBySingle = ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle;
            PrintEcrTicketCommand = new RelayCommand(OnPrintEcrTicket, CanPrintEcrTicket);
            CheckForExistingQuantityCommand = new RelayCommand(OnCheckForExistingQuantity, CanCheckForExistingQuantity);
            IsPrintTicket = ApplicationManager.Settings.IsPrintSaleTicket;
            IsEcrActivated = ApplicationManager.Settings.IsEcrActivated;
            UseExtPos = ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.UseExtPos;
        }

        protected override void SetPrice()
        {
            foreach (var item in InvoiceItems)
            {
                item.Price = GetProductPrice(item.Product);
            }
            RaisePropertyChanged("InvoiceItems");
            if (InvoiceItem != null)
            {
                InvoiceItem.Price = GetProductPrice(InvoiceItem.Product);
            }
            RaisePropertyChanged("InvoiceItem");
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
            PrintManager.PrintDataGrid(ticket, ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
        }

        protected override InvoiceItemsModel GetInvoiceItem(string code)
        {
            var invoiceItem = base.GetInvoiceItem(code);
            invoiceItem.Price = GetProductPrice(invoiceItem.Product);
            return invoiceItem;
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageBox.Show(message, "Անբավարար միջոցներ");
                return false;
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
            var ecrManager = new EcrServer(ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig);
            ecrManager.OnError += delegate(Exception ex) { MessageManager.OnMessage(ex.ToString()); };

            var products = InvoiceItems.Select(ii => (IEcrProduct)new EcrProduct(ii.Code, ii.Description, ii.Mu)
            {
                Price = ii.Product.Price ?? 0,
                Qty = ii.Quantity ?? 0,
                Discount = ii.Discount,
                DiscountType = ii.Discount != null && ii.Discount > 0 ? 1 : (int?)null,
                Dep = ii.Product.TypeOfTaxes == default(TypeOfTaxes) || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.All(s => s.IsActive && s.CashierDepartment.Type != (int)ii.Product.TypeOfTaxes) ?
                    ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id :
                    ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Single(s => s.IsActive && s.CashierDepartment.Type == (int)ii.Product.TypeOfTaxes).CashierDepartment.Id,
                AdgCode = ii.Product.HcdCs
            }).ToList();
            var invoicePaid = new EcrPaid
            {
                PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                PartialAmount = 0,
                PrePaymentAmount = (double)(InvoicePaid.ReceivedPrepayment ?? 0),
                UseExtPos = UseExtPos
            };
            var responceReceiptViewModel = ecrManager.PrintReceipt(products, invoicePaid, Partner.TIN);
            var message = responceReceiptViewModel != null ?
                new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageTypeEnum.Success)
                : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageTypeEnum.Warning);
            MessageManager.OnMessage(message);
        }
        private bool CanCheckForExistingQuantity(object o)
        {
            return InvoiceItems.Any(s => s.Quantity > 0);
        }
        private void OnCheckForExistingQuantity(object o)
        {
            InvoicesManager.CheckForQuantityExisting(InvoiceItems.ToList(), FromStocks.Select(s => s.Id).ToList());
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
                product = new ProductsManager().GetProductsByCodeOrBarcode(item.Code);
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
                    var result = MessageBox.Show(
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
                var exProduct = new ProductsManager().GetProductsByCodeOrBarcode(product.Code);
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

        private void CheckForPrize(InvoiceModel invoice)
        {
            if (!InvoicesManager.CheckForPrize(invoice)) return;
            MessageBox.Show(string.Format("Շնորհավորում ենք դուք շահել եք։ \n Ապրանքագիր։ {0} \n Ամսաթիվ։ {1} \n Պատվիրատու։ {2}", invoice.InvoiceNumber, invoice.ApproveDate, invoice.RecipientName), "Շահում", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
            return base.CanAddInvoiceItem(o);
        }

        protected override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                MessageBox.Show("Ապրանքի ավելացումը հնարավոր չէ:");
                return;
            }
            ++count;

            if (!SetQuantity(AddBySingle))
            {
                return;
            }
            base.OnAddInvoiceItem(o);
            //InvoicePaid.Paid = InvoiceItems.Sum(s => s.Amount);
            //RaisePropertyChanged("InvoicePaid");
        }

        protected override decimal GetProductPrice(EsProductModel product)
        {
            decimal price = product != null ? (product.Price ?? 0) : 0;
            if (Partner == null || product == null) { return price; }

            price = base.GetProductPrice(product);
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
        private bool IsPaidValid
        {
            get { return InvoicePaid.IsPaid && InvoicePaid.Change <= (InvoicePaid.Paid ?? 0) && Partner != null && (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - Partner.Debit && (InvoicePaid.ReceivedPrepayment ?? 0) <= Partner.Credit; }
        }

        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && IsPaidValid;
        }

        protected override void OnApprove(object o)
        {
            if (!CanApprove(o) && IsModified)
            {
                MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                return;
            }

            IsLoading = true;
            PrepareToApprove();
            //Approve Invoice
            new Thread(() => OnApproveAsync(false)).Start();
        }

        protected virtual void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;

            //Paid
            var cashDesk = InvoicePaid.Paid > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks).SingleOrDefault() : null;
            InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;

            var bankAccount = InvoicePaid.ByCheck > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts).SingleOrDefault() : null;
            InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
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
                ResponseReceiptModel responceReceiptModel = null;
                if (IsEcrActivated)
                {
                    responceReceiptModel = RegisterEcrTicket();
                    if (responceReceiptModel == null)
                    {
                        IsLoading = false;
                        MessageManager.OnMessage("ՀԴՄ կտրոնի տպումը ձախողվել է:", MessageTypeEnum.Warning);
                        return;
                    }
                }

                var invoice = InvoicesManager.ApproveSaleInvoice(Invoice, InvoiceItems.ToList(), FromStocks.Select(s => s.Id).ToList(), InvoicePaid);
                if (invoice == null || Application.Current == null)
                {
                    Invoice.AcceptDate = Invoice.ApproveDate = null;
                    MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                    IsLoading = false;
                    return;
                }

                //Open Cash desk
                if (!string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort) && InvoicePaid.ByCash > 0)
                {
                    CashDeskManager.OpenCashDesk();
                }

                //Print ticket
                if (IsPrintTicket && (!IsEcrActivated || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.UseExternalPrinter) && Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => PrintInvoiceTicket(responceReceiptModel)));
                }


                //CheckForPrize(Invoice);
                MessageManager.OnMessage(string.Format("{0} ապրանքագիրը հաստատվել է հաջողությամբ: Գումար {1} վճարվել է {2}",Invoice.InvoiceNumber, invoice.Amount,  InvoicePaid.ByCash), MessageTypeEnum.Success);

                if (closeOnExit && invoice.ApproveDate != null)
                {
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, new Action(() => OnClose(null)));
                }
                else
                {
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, new Action(() => ReloadApprovedInvoice(invoice)));
                }
            }
            catch (Exception ex)
            {
                MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error);
            }
            IsLoading = false;
        }
        private ResponseReceiptModel RegisterEcrTicket()
        {
            ResponseReceiptModel responceReceiptModel = null;
            try
            {
                if (CanPrintEcrTicket(null))
                {
                    //todo
                    var items = InvoiceItems.Select(ii => (IEcrProduct)new EcrProduct(ii.Code, ii.Description, ii.Mu)
                    {
                        Price = ii.Product.Price ?? 0,
                        Qty = ii.Quantity ?? 0,
                        Discount = ii.Discount,
                        DiscountType = ii.Discount != null ? 1 : (int?)null,
                        Dep = ii.Product.TypeOfTaxes == default(TypeOfTaxes) || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.All(s => s.IsActive && s.CashierDepartment.Type != (int)ii.Product.TypeOfTaxes) ? ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id :
                              ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Single(s => s.IsActive && s.CashierDepartment.Type == (int)ii.Product.TypeOfTaxes).CashierDepartment.Id,

                        AdgCode = ii.Product.HcdCs
                    }).ToList();
                    var paid = new EcrPaid
                    {
                        PaidAmount = (double)(InvoicePaid.Paid ?? 0) + (double)(InvoicePaid.AccountsReceivable ?? 0),
                        PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                        PartialAmount = 0,
                        PrePaymentAmount = (double)(InvoicePaid.ReceivedPrepayment ?? 0),
                        UseExtPos = UseExtPos
                    };
                    //paid.PaidAmount = (double)items.Sum(s => s.DiscountType == null ? 
                    //    s.Price * s.Qty : s.DiscountType == 1 ? 
                    //    s.Price * s.Qty * (1 - (s.Discount ?? 0)) / 100: 
                    //    s.Price * s.Qty - (s.Discount ?? 0))
                    //    - paid.PaidAmountCard - paid.PartialAmount - paid.PrePaymentAmount;
                    var ecrManager = new EcrServer(ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig);
                    responceReceiptModel = ecrManager.PrintReceipt(items, paid, Partner.TIN);

                    if (responceReceiptModel != null)
                    {
                        Invoice.RecipientTaxRegistration = responceReceiptModel.Rseq.ToString();
                        MessageManager.OnMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptModel.Rseq, MessageTypeEnum.Success));
                    }
                    else
                    {
                        MessageManager.OnMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումը ձախողվել է:  {0}", string.Format("{0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode)), MessageTypeEnum.Warning));
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current != null)
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error)));
                // aborting
                return null;
            }
            return responceReceiptModel;
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
        public ICommand CheckForExistingQuantityCommand { get; private set; }
        #endregion
    }
}
