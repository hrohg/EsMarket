using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CashReg;
using CashReg.Models;
using ES.Business.ExcelManager;
using ES.Business.FileManager;
using ES.Data.Model;
using ES.Data.Models;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using Shared.Helpers;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using InvoicePaid = CashReg.Helper.InvoicePaid;
using ProductModel = ES.Business.Models.ProductModel;

namespace UserControls.ViewModels.Invoices
{
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
                SetPartnerPrice();
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceViewModel(EsUserModel user, EsMemberModel member)
            : base(user, member)
        {
            Initialize();
        }
        public SaleInvoiceViewModel(Guid id, EsUserModel user, EsMemberModel member)
            : base(id, user, member)
        {
            Initialize();
        }
        #endregion

        #region Internal methods

        private void Initialize()
        {
            var xml = new XmlManager();
            FromStocks = StockManager.GetStocks(xml.GetItemsByControl(XmlTagItems.SaleStocks).Select(s => HgConvert.ToInt64(s.Value)).ToList(), Member.Id).ToList();
            Invoice.InvoiceTypeId = (int)InvoiceType.SaleInvoice;
            if (Partner == null)
            {
                var provideDefault = ApplicationManager.CashManager.GetEsDefaults(DefaultControls.Customer);
                Partner = provideDefault == null ? ApplicationManager.CashManager.GetPartners.FirstOrDefault() : ApplicationManager.CashManager.GetPartners.FirstOrDefault(s => s.Id == provideDefault.ValueInGuid);
            } IsModified = false;
            SaleBySingle = ApplicationManager.SaleBySingle;
            PrintEcrTicketCommand = new RelayCommand(OnPrintEcrTicket, CanPrintEcrTicket);
        }

        private void SetPartnerPrice()
        {
            foreach (var item in InvoiceItems)
            {
                item.Price = GetPartnerPrice(item.Product);
            }
            OnPropertyChanged("InvoiceItems");
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
            PrintManager.PrintDataGrid(ticket, ApplicationManager.SalePrinter);
        }
        protected override void OnGetProduct(object o)
        {
            base.OnGetProduct(o);
            OnAddInvoiceItem(o);
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemCount(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList(), Member.Id);
            if (exCount == 0)
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, "Անբավարար միջոցներ: Տվյալ ապրանքատեսակից առկա չէ:", MessageModel.MessageTypeEnum.Warning));
                return false;
            }
            if (InvoiceItem.Quantity == null || InvoiceItem.Quantity == 0)
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
            if (InvoiceItem.Quantity == null || InvoiceItem.Quantity <= 0)
            {
                return false;
            }
            if (!(InvoiceItem.Quantity > exCount)) return true;
            InvoiceItem.Quantity = null;
            ApplicationManager.MessageManager.OnNewMessage(new MessageModel(DateTime.Now, string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code), MessageModel.MessageTypeEnum.Warning));
            return false;
        }
        private bool CanPrintEcrTicket(object o)
        {
            return string.IsNullOrEmpty(Invoice.RecipientTaxRegistration) && ApplicationManager.IsEcrActivated;
        }
        private void OnPrintEcrTicket(object o)
        {
            var ecrManager = new EcrServer(ApplicationManager.EcrSettings);
            var products = InvoiceItems.Select(s => new CashReg.Helper.Product(s.Code, s.Description, s.Mu)
            {
                Price = s.Product.Price ?? 0,
                Qty = s.Quantity ?? 0,
                Discount = s.Discount,
                DiscountType = s.Discount != null && s.Discount > 0 ? 1 : (int?)null,
                AdgCode = s.Product.HcdCs
            }).ToList();
            var invoicePaid = new CashReg.Helper.InvoicePaid
            {
                PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                PartialAmount = 0,
                PrePaymentAmount = 0
            };
            var responceReceiptViewModel = ecrManager.PrintReceipt(products, invoicePaid);
            var message = responceReceiptViewModel != null ?
                new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageModel.MessageTypeEnum.Success)
                : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageModel.MessageTypeEnum.Warning);
            ApplicationManager.MessageManager.OnNewMessage(message);
        }

        protected override void OnImportInvoice(object o)
        {
            InvoiceItems.Clear();
            var filePath = FileManager.OpenExcelFile("Excel files(*.xls *.xlsx *․xlsm)|*.xls;*.xlsm;*․xlsx|Excel with macros|*.xlsm|Excel 97-2003 file|*.xls", "Excel ֆայլի բեռնում", ConfigSettings.GetConfig("ImportingFilePath"));
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                IsLoading = false;
                return;
            }
            ConfigSettings.SetConfig("ImportingFilePath", Path.GetDirectoryName(filePath));
            ProductModel product;
            var invoice = ExcelImportManager.ImportSaleInvoice(filePath);
            if(invoice==null) return;
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
            }
        }

        #endregion

        #region External methods

        protected override void OnPrintInvoice(PrintSizeEnum printSize)
        {
            if (!CanPrintInvoice(printSize)) { return; }
            var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new SaleInvoiceView(new SaleInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId, Invoice.MemberId), InvoicePaid, Member));

            switch (printSize)
            {
                case PrintSizeEnum.Normal:
                    PrintManager.PrintPreview(ctrl, "Print invoices", true);
                    break;
                case PrintSizeEnum.Small:
                    PrintInvoiceTicket(null);
                    break;
                case PrintSizeEnum.Large:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printSize", printSize, null);
            }
        }

        private int count = 0;
        public override bool CanAddInvoiceItem(object o)
        {
            ++count;
            return base.CanAddInvoiceItem(o) && FromStocks != null;
        }

        public override void OnAddInvoiceItem(object o)
        {
            if (!CanAddInvoiceItem(o))
            {
                return;
            }
            if (!SetQuantity(SaleBySingle))
            {
                return;
            }
            base.OnAddInvoiceItem(o);
        }

        public override void SetInvoiceItem(string code)
        {
            base.SetInvoiceItem(code);
            if (InvoiceItem.Product != null)
            {
                InvoiceItem.Price = InvoiceItem.Product.Price;
            }
        }

        private bool IsPaid
        {
            get { return InvoicePaid.IsPaid && InvoicePaid.Change <= (InvoicePaid.Paid ?? 0) && Partner != null && (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - (Partner.Debit ?? 0) && (InvoicePaid.ReceivedPrepayment ?? 0) <= (Partner.Credit ?? 0); }
        }

        public override bool CanApprove(object o)
        {
            return base.CanApprove(o) && IsPaid;
        }

        public override void OnApprove(object o)
        {
            if (!CanApprove(o))
            {
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողության ձախողում:", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            var xml = new XmlManager();
            Invoice.ApproverId = User.UserId;
            Invoice.Approver = User.FullName;

            InvoicePaid.CashDeskId = xml.GetItemsByControl(XmlTagItems.SaleCashDesks).Select(s => HgConvert.ToGuid(s.Value)).SingleOrDefault();
            InvoicePaid.CashDeskForTicketId = CashDeskManager.TryGetCashDesk(false, Invoice.MemberId).Select(s => s.Id).FirstOrDefault();
            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;
            var invoice = InvoicesManager.ApproveSaleInvoice(Invoice, InvoiceItems.ToList(), FromStocks.Select(s => s.Id), InvoicePaid);
            if (invoice == null)
            {
                Invoice.AcceptDate = Invoice.ApproveDate = null;
                ApplicationManager.MessageManager.OnNewMessage(new MessageModel("Գործողության ձախողում:", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            //CheckForPrize(Invoice);
            ResponseReceiptModel responceReceiptModel = null;
            if (ApplicationManager.IsEcrActivated && CanPrintEcrTicket(o))
            {
                //todo
                var ecrManager = new EcrServer(ApplicationManager.EcrSettings);
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
                    PrePaymentAmount = 0
                });
                if (responceReceiptModel != null)
                {
                    invoice.RecipientTaxRegistration = responceReceiptModel.Rseq.ToString();
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptModel.Rseq, MessageModel.MessageTypeEnum.Success));
                }
                else
                {
                    ApplicationManager.MessageManager.OnNewMessage(new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageModel.MessageTypeEnum.Warning));
                    //return false;
                }
            }
            IsModified = false;
            Invoice = invoice;
            if (!string.IsNullOrEmpty(ApplicationManager.SalePrinter) && (!ApplicationManager.IsEcrActivated || ApplicationManager.EcrSettings.UseExternalPrinter))
            {
                PrintInvoiceTicket(responceReceiptModel);
            }
            InvoiceItems = new ObservableCollection<InvoiceItemsModel>(InvoicesManager.GetInvoiceItems(Invoice.Id, Member.Id).OrderBy(s => s.Index));
            AccountsReceivable = new AccountsReceivableModel(Invoice.Id, Partner.Id, User.UserId, Invoice.MemberId, true);
            return;
        }

        public override void OnApproveAndClose(object o)
        {
            OnApprove(o);
            if (Invoice.ApproveDate != null)
            {
                OnClose(o);
            }
        }

        #endregion

        #region Commands

        public ICommand PrintEcrTicketCommand { get; private set; }

        #endregion
    }
}
