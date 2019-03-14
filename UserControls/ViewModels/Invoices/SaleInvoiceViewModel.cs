using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CashReg;
using CashReg.Helper;
using CashReg.Interfaces;
using CashReg.Models;
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

namespace UserControls.ViewModels.Invoices
{
    [Serializable]
    public class SaleInvoiceViewModel : OutputOrderViewModel
    {
        #region Internal properties
        private bool _useExtPos;
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
        #endregion Constructors

        #region Internal methods

        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (Invoice.Partner == null &&
                Invoice.InvoiceTypeId == (int)ES.Business.Managers.InvoiceTypeEnum.SaleInvoice)
                SetDefaultPartner(PartnersManager.GetDefaultParnerByInvoiceType((InvoiceType)Invoice.InvoiceTypeId) ??
                                  PartnersManager.GetDefaultPartner(PartnerType.None));
            PrintEcrTicketCommand = new RelayCommand(OnPrintEcrTicket, CanPrintEcrTicket);
            IsPrintTicket = ApplicationManager.Settings.IsPrintSaleTicket;
            IsEcrActivated = ApplicationManager.Settings.IsEcrActivated;
            UseExtPos = ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.UseExtPos;
            IsModified = false;
        }

        protected override void SetPrice()
        {
            base.SetPrice();
            SetDiscountBond();
        }

        private void SetDiscountBond()
        {
            var bond = ApplicationManager.Settings.SettingsContainer.MemberSettings.UseDiscountBond ? InvoiceItems.Sum(s => GetPartnerDiscountBond(s.Product)) : 0;
            InvoicePaid.DiscountBond = bond;
        }
        protected override void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnInvoiceItemsChanged(sender, e);
            SetDiscountBond();
        }

        private void PrintInvoiceTicket(ResponseReceiptModel ecrResponseReceiptModel)
        {
            var ticket = new ReceiptTicketSmall(new SaleInvoiceSmallTicketViewModel(ecrResponseReceiptModel)
            {
                Invoice = Invoice,
                InvoiceItems = InvoiceItems.ToList(),
                InvoicePaid = InvoicePaid
            });
            //PrintPreview
            //new UiPrintPreview(ticket).ShowDialog();

            //Print
            PrintManager.Print(ticket, ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
        }

        
        protected override void InvoiceLoadCompleted()
        {
            //SetPrice();
        }

        protected override bool CanRemoveInvoiceItem(object o)
        {
            return base.CanRemoveInvoiceItem(o)  && ApplicationManager.IsInRole(UserRoleEnum.Seller);
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
            var ecrManager = EcrManager.EcrServer;

            var products = InvoiceItems.Select(ii => (IEcrProduct)new EcrProduct(ii.Code, ii.Description, ii.Mu)
            {
                Price = ii.Product.Price ?? 0,
                Qty = ii.Quantity ?? 0,
                Discount = ii.Discount,
                DiscountType = ii.Discount != null && ii.Discount > 0 ? 1 : (int?)null,
                Dep = ii.Product.TypeOfTaxes == default(TypeOfTaxes) || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.All(s => s.IsActive && s.CashierDepartment.Type != (int)ii.Product.TypeOfTaxes) ? ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id : ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Single(s => s.IsActive && s.CashierDepartment.Type == (int)ii.Product.TypeOfTaxes).CashierDepartment.Id,
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
            var message = responceReceiptViewModel != null ? new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.ActionDescription, ecrManager.ActionCode), MessageTypeEnum.Warning);
            MessageManager.OnMessage(message);
        }

        //private void CheckForPrize(InvoiceModel invoice)
        //{
        //    if (!InvoicesManager.CheckForPrize(invoice)) return;
        //    MessageManager.ShowMessage(string.Format("Շնորհավորում ենք դուք շահել եք։ \n Ապրանքագիր։ {0} \n Ամսաթիվ։ {1} \n Պատվիրատու։ {2}", invoice.InvoiceNumber, invoice.ApproveDate, invoice.RecipientName), "Շահում", MessageBoxImage.Exclamation);
        //}
        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize))
            {
                return;
            }

            switch (printSize)
            {
                case PrintModeEnum.Normal:
                case PrintModeEnum.Large:
                    base.OnPrintInvoice(printSize);
                    break;
                case PrintModeEnum.Small:
                    PrintInvoiceTicket(null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printSize", printSize, null);
            }
        }

        protected override decimal GetProductPrice(ProductModel product)
        {

            if (Partner == null || product == null)
            {
                return 0;
            }

            var price = base.GetProductPrice(product);
            switch (Partner.PartnersTypeId)
            {
                case (long)PartnerType.Dealer:

                    break;
                default:
                    if (!ApplicationManager.Settings.SettingsContainer.MemberSettings.UseDiscountBond) price -= (price * (Partner != null ? (Partner.Discount ?? 0) / 100 : 0));
                    break;
            }
            return product.HasDealerPrice ? Math.Max(price, product.GetProductDealerPrice()) : price;
        }

        private decimal GetPartnerDiscountBond(ProductModel product)
        {
            var price = GetProductPrice(product);
            if (ApplicationManager.Settings.SettingsContainer.MemberSettings.UseDiscountBond) price -= (price * (Partner != null ? (Partner.Discount ?? 0) / 100 : 0));
            return (product.Price ?? 0) - (product.HasDealerPrice ? Math.Max(price, product.GetProductDealerPrice()) : price);
        }

        private bool IsPaidValid
        {
            get { return InvoicePaid.IsPaid && InvoicePaid.Change <= (InvoicePaid.Paid ?? 0) && Partner != null && (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - Partner.Debit && (InvoicePaid.ReceivedPrepayment ?? 0) <= Partner.Credit; }
        }
        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && IsPaidValid;
        }


        #endregion

        #region External methods

       protected override void OnApproveAsync(bool closeOnExit)
        {
           base.OnApproveAsync(closeOnExit);
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
                if (invoice == null)
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
                MessageManager.OnMessage(string.Format("{0} ապրանքագիրը հաստատվել է հաջողությամբ: Գումար {1} վճարվել է {2}", Invoice.InvoiceNumber, invoice.Total.ToString("N"), InvoicePaid.ByCash.ToString("N")), MessageTypeEnum.Success);

                if (closeOnExit && invoice.ApproveDate != null)
                {
                    OnClose();
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
                        Dep = ii.Product.TypeOfTaxes == default(TypeOfTaxes) || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.All(s => s.IsActive && s.CashierDepartment.Type != (int)ii.Product.TypeOfTaxes) ? ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id : ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Single(s => s.IsActive && s.CashierDepartment.Type == (int)ii.Product.TypeOfTaxes).CashierDepartment.Id,
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
                    var ecrManager = EcrManager.EcrServer;
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
                DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, new Action(() => MessageManager.OnMessage(ex.Message, MessageTypeEnum.Error)));
                // aborting
                return null;
            }
            return responceReceiptModel;
        }
        
        public override bool DenyChangePrice
        {
            get { return !ApplicationManager.IsInRole(UserRoleEnum.SaleManager) && base.DenyChangePrice; }
        }

        #endregion

        #region Commands
        public ICommand PrintEcrTicketCommand { get; private set; }
        #endregion
    }
}
