using CashReg.Helper;
using CashReg.Interfaces;
using CashReg.Models;
using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Data.Enumerations;
using ES.Data.Models;
using ES.Data.Models.Products;
using Shared.Helpers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using CashDeskManager = UserControls.Managers.CashDeskManager;

namespace UserControls.ViewModels.Invoices
{
    [Serializable]
    public class SaleInvoiceViewModel : OutputOrderViewModel
    {
        #region Internal properties
        private bool _useExtPos;
        private bool _useDiscountBond;
        #endregion

        #region External properties

        public override string Description
        {
            get { return string.Format("{0}{1}", Title, Partner != null ? string.Format(" ({0})", Partner.FullName) : string.Empty); }
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
                RaisePropertyChanged(() => IsEcrActivated);
                RaisePropertyChanged(() => EcrButtonTooltip);
                RaisePropertyChanged(() => AllowCashReceivable);
                ApplicationManager.Settings.IsEcrActivated = value;
            }
        }
        public string EcrButtonTooltip { get { return IsEcrActivated ? "ՀԴՄ ակտիվ է" : "ՀԴՄ պասիվ է"; } }
        public string BondButtonTooltip { get { return UseDiscountBond ? "Կիրառել բոնուս" : "Բոնուս չի կիրառվում"; } }
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
        public bool UseDiscountBond
        {
            get { return _useDiscountBond && HasDiscountBond; }
            set
            {
                _useDiscountBond = value;
                Invoice.UseDiscountBond = value;
                RaisePropertyChanged(() => UseDiscountBond); RaisePropertyChanged(() => BondButtonTooltip);
                SetDiscountBond();
            }
        }
        public bool HasDiscountBond
        {
            get
            {
                return ApplicationManager.Settings.Branch != null && ApplicationManager.Settings.Branch.IsActive && ApplicationManager.Settings.Branch.UseDiscountBond && ApplicationManager.Settings.Branch.DefaultDiscount >= 0;
            }
        }


        public override bool AllowCashReceivable
        {
            get
            {
                return base.AllowCashReceivable && ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks.Any() && (string.IsNullOrEmpty(Partner.TIN) || !IsEcrActivated);
            }
        }
        public override bool AllowByCheckReceivable
        {
            get
            {
                return base.AllowByCheckReceivable && ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts.Any();
            }
        }
        #endregion

        #region Constructors
        public SaleInvoiceViewModel()
            : base(InvoiceType.SaleInvoice)
        {
            UseDiscountBond = HasDiscountBond;            
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

            //new System.Threading.Thread(() =>
            //{
            //    System.Threading.Thread.Sleep(5000);
            //    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Send, new Action(() => { SetExternalText(new ExternalTextImputEventArgs("740740")); }));
            //}).Start();
        }
        protected override void OnPartnerChanged()
        {
            if (Invoice.IsApproved) return;
            if (Partner != null)
            {
                Invoice.RecipientName = Partner.FullName;

                switch (Partner.PartnersTypeId)
                {
                    case (short)PartnerType.Dealer:
                        Invoice.Discount = Partner.Discount;
                        break;
                    default:
                        Invoice.Discount = Partner.Discount.HasValue ? Partner.Discount : UseDiscountBond ? (decimal)ApplicationManager.Settings.Branch.DefaultDiscount : (decimal?)null;
                        break;
                }
            }
            else
            {
                Invoice.RecipientName = null;
                Invoice.Discount = 0;
            }
            if (InvoiceItem != null && InvoiceItem.Product != null)
            {
                InvoiceItem.Price = GetProductPrice(InvoiceItem.Product);
                //InvoiceItem.Discount = GetProductDiscount(InvoiceItem.Product);
            }
            foreach (var invoiceItem in InvoiceItems)
            {
                if (invoiceItem.HasDiscount) continue;
                invoiceItem.Price = GetProductPrice(invoiceItem.Product);
                //if (invoiceItem.Discount.HasValue) invoiceItem.Price = Math.Max((invoiceItem.ProductPrice ?? 0) * (1 - invoiceItem.Discount.Value / 100), invoiceItem.Product.ProductDealerPrice);
                //Do not set a discount, because the price is indicated.
                //invoiceItem.Discount = GetProductDiscount(invoiceItem.Product);
            }
            OnInvoiceItemsPropertyChanged(null, null);
            //SetDiscountBond();
            base.OnPartnerChanged();
        }
        protected override void OnInvoiceItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnInvoiceItemsChanged(sender, e);
            if (!Invoice.IsApproved) SetDiscountBond();
        }
        protected override void OnInvoiceItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnInvoiceItemsPropertyChanged(sender, e);
            SetDiscountBond();
        }
        protected override decimal GetProductPrice(ProductModel product)
        {
            decimal price = 0;
            if (product == null) return price;
            if (Partner == null) return product.ProductPrice;
            var productDealerPrice = product.ProductDealerPrice;
            switch (Partner.PartnersTypeId)
            {
                case (short)PartnerType.Dealer:
                    return productDealerPrice;
                default:
                    if (Invoice.Discount != 0)
                    {
                        price = Math.Min((product.Price ?? 0) * (1 - (Invoice.Discount ?? 0) / 100), product.ProductPrice);
                        //
                        if (product.HasDealerPrice) price = Math.Max(productDealerPrice, price);
                    }
                    else
                    {
                        price = product.Price ?? 0;
                    }
                    return price;
            }
        }
        protected override void OnInvoiceItemChanged(InvoiceItemsModel invoiceItem)
        {
            base.OnInvoiceItemChanged(invoiceItem);

            //SetDiscountBond();
        }
        protected override bool SetQuantity(bool addSingle)
        {
            return base.SetQuantity(addSingle);
        }
        private void SetDiscountBond()
        {
            if (UseDiscountBond && Partner != null && Invoice.Discount == null)
            {
                //Invoice.Discount = (decimal)ApplicationManager.Settings.Branch.DefaultDiscount;
                RaisePropertyChanged("ActualPercentage");
                RaisePropertyChanged("InvoiceProfit");
            }
            //foreach (InvoiceItemsModel invoiceItem in InvoiceItems)
            //    //{
            //    //    decimal discount = 0;
            //    //    if (Partner != null && (Partner.PartnersTypeId != (short)PartnerType.Dealer || !invoiceItem.Product.HasDealerPrice))
            //    //        discount = Invoice.Discount == 0 ? 0 : invoiceItem.Discount ?? Invoice.Discount ?? 0;

            //    //    //var price = (invoiceItem.Price ?? 0) * (1 - (discount) / 100);
            //    //    ////if(invoiceItem.Product.HasDealerPrice) price = Math.Max(invoiceItem.Product.GetProductDealerPrice(), price);
            //    //    //total += price * (invoiceItem.Quantity ?? 0);
            //    //}
            //    total = InvoiceItems.Sum(s => s.Price ?? 0 * s.Quantity ?? 0);

            var amount = InvoiceItems.Sum(s => (s.ProductPrice ?? 0) * (s.Quantity ?? 0));
            var total = Invoice.Total = InvoiceItems.Sum(s => (s.Price ?? 0) * (s.Quantity ?? 0));

            Invoice.Amount = amount;
            InvoicePaid.Total = Invoice.Total = UseDiscountBond ? amount : total;
            Invoice.DiscountBond = UseDiscountBond ? amount - total : 0;
        }

        private void PrintInvoiceTicket(ResponseReceiptModel ecrResponseReceiptModel)
        {
            var ticket = new ReceiptTicketSmall(new SaleInvoiceSmallTicketViewModel(ecrResponseReceiptModel)
            {
                Invoice = Invoice,
                InvoiceItems = InvoiceItems.ToList(),
                InvoicePaid = InvoicePaid,
                Department = $"{ApplicationManager.Settings.SettingsContainer.MemberSettings.BranchSettings.Name}"
            });

            //PrintPreview
            //new UiPrintPreview(ticket).ShowDialog(); return;

            //Print
            PrintManager.Print(ticket, ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
        }

        protected override bool CanRemoveInvoiceItem(object o)
        {
            return base.CanRemoveInvoiceItem(o) && ApplicationManager.IsInRole(UserRoleEnum.Seller) && !ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller);
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
                price = ii.Product.Price ?? 0,
                qty = ii.Quantity ?? 0,
                discount = !UseDiscountBond ? ii.Discount : 0,
                discountType = !UseDiscountBond && ii.Discount != null && ii.Discount > 0 ? 1 : (int?)null,
                dep =
                ii.Product.TypeOfTaxes != default(TypeOfTaxes) && ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Any(s => s.IsActive && s.TypeOfTaxesId == (int)ii.Product.TypeOfTaxes) ?
                                                                        ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.First(s => s.IsActive && s.TypeOfTaxesId == (int)ii.Product.TypeOfTaxes).SelectedDepartmentId :
                                                                        ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id,
                adgCode = ii.Product.HcdCs
            }).ToList();

            //MessageManager.OnMessage(
            //    string.Format(" {0} {1} {2}", InvoiceItems.First().Product.TypeOfTaxes, 
            //    ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Where(s => s.IsActive).Select(s => s.SelectedDepartmentId).ToList().Aggregate<int, string>("", (text, next) => text += next + " "),
            //    ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Where(s => s.IsActive).Select(s => s.TypeOfTaxesId).ToList().Aggregate<int, string>("", (text, next) => text += next + " "))
            //    );
            //return;
            var invoicePaid = new EcrPaid
            {
                PaidAmount = InvoicePaid.Paid != null ? (double)InvoicePaid.Paid : 0,
                PaidAmountCard = InvoicePaid.ByCheck != null ? (double)InvoicePaid.ByCheck : 0,
                PartialAmount = 0,
                PrePaymentAmount = (double)(InvoicePaid.ReceivedPrepayment ?? 0),
                UseExtPos = UseExtPos
            };
            var responceReceiptViewModel = ecrManager.PrintReceiptTicket(products, invoicePaid, new EcrTickedAdditionalData { PartnerTin = Partner.TIN, EMarks = GetEmarks() });
            var message = responceReceiptViewModel != null ? new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptViewModel.Fiscal, MessageTypeEnum.Success) : new MessageModel("ՀԴՄ կտրոնի տպումը ձախողվել է:" + string.Format(" {0} ({1})", ecrManager.Result.Description, ecrManager.Result.Code), MessageTypeEnum.Warning);
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

        //private decimal GetPartnerDiscountBond(ProductModel product)
        //{
        //    var price = GetProductPrice(product);
        //    //if (ApplicationManager.Settings.Branch.UseDiscountBond)
        //        price -= (price * (Partner != null ? (Partner.Discount ?? 0) / 100 : 0));
        //    return (product.Price ?? 0) - (product.HasDealerPrice ? Math.Max(price, product.GetProductDealerPrice()) : price);
        //}

        protected virtual bool IsPaidValid()
        {
            return InvoicePaid.IsPaid &&
        (InvoicePaid.Change ?? 0) <= (InvoicePaid.Paid ?? 0) &&
        Partner != null &&
        (InvoicePaid.AccountsReceivable ?? 0) <= (Partner.MaxDebit ?? 0) - Partner.Debit &&
        (InvoicePaid.ReceivedPrepayment ?? 0) <= Partner.Credit;
        }
        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && IsPaidValid();
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
                else
                {
                    invoice.Amount = Invoice.Amount;
                    Invoice = invoice;
                }

                //Open Cash desk
                if (!string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.CashDeskPort) || !string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveCashDeskPrinter) && InvoicePaid.ByCash > 0)
                {
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, CashDeskManager.OpenCashDesk);
                }

                //Print ticket
                if (IsPrintTicket && (!IsEcrActivated || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.UseExternalPrinter) && Application.Current != null)
                {
                    DispatcherWrapper.Instance.BeginInvoke(DispatcherPriority.Normal, (() => PrintInvoiceTicket(responceReceiptModel)));
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
                        price = ii.Product.Price ?? 0,
                        qty = ii.Quantity ?? 0,
                        discount = !UseDiscountBond ? ii.Discount : 0,
                        discountType = !UseDiscountBond && ii.Discount > 0 ? 1 : (int?)null,
                        dep =
                        //ii.Product.TypeOfTaxes == default(TypeOfTaxes) || ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.All(s => s.IsActive && s.TypeOfTaxesId != (int)ii.Product.TypeOfTaxes) ?
                        //ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id :
                        //ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.First(s => s.IsActive && s.TypeOfTaxesId == (int)ii.Product.TypeOfTaxes).CashierDepartment.Id,
                        ii.Product.TypeOfTaxes != default(TypeOfTaxes) && ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.Any(s => s.IsActive && s.TypeOfTaxesId == (int)ii.Product.TypeOfTaxes) ?
                                                                        ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfigs.First(s => s.IsActive && s.TypeOfTaxesId == (int)ii.Product.TypeOfTaxes).SelectedDepartmentId :
                                                                        ApplicationManager.Settings.SettingsContainer.MemberSettings.EcrConfig.CashierDepartment.Id,
                        adgCode = ii.Product.HcdCs
                    }).ToList();
                    var paid = new EcrPaid
                    {
                        PaidAmount = (double)(InvoicePaid.Paid ?? 0),
                        PaidAmountCard = InvoicePaid.ByCheck.HasValue ? (double)InvoicePaid.ByCheck : 0,
                        PartialAmount = 0,
                        PrePaymentAmount = (double)(InvoicePaid.ReceivedPrepayment ?? 0) + (double)(InvoicePaid.AccountsReceivable ?? 0),
                        UseExtPos = UseExtPos
                    };
                    //paid.PaidAmount = (double)items.Sum(s => s.DiscountType == null ? 
                    //    s.Price * s.Qty : s.DiscountType == 1 ? 
                    //    s.Price * s.Qty * (1 - (s.Discount ?? 0)) / 100: 
                    //    s.Price * s.Qty - (s.Discount ?? 0))
                    //    - paid.PaidAmountCard - paid.PartialAmount - paid.PrePaymentAmount;
                    var ecrManager = EcrManager.EcrServer;

                    responceReceiptModel = ecrManager.PrintReceiptTicket(items, paid, new EcrTickedAdditionalData { PartnerTin = Partner.TIN, EMarks = GetEmarks() });

                    if (responceReceiptModel != null)
                    {
                        Invoice.RecipientTaxRegistration = responceReceiptModel.Rseq.ToString();
                        MessageManager.OnMessage(new MessageModel("ՀԴՄ կտրոնի տպումն իրականացել է հաջողությամբ:" + responceReceiptModel.Rseq, MessageTypeEnum.Success));
                    }
                    else
                    {
                        MessageManager.OnMessage(new MessageModel(string.Format("ՀԴՄ կտրոնի տպումը ձախողվել է:  {0}", string.Format("{0} ({1})", ecrManager.Result.Description, ecrManager.Result.Code)), MessageTypeEnum.Warning));
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

        public override bool Close()
        {
            return base.Close();
        }

        #endregion

        #region Commands
        public ICommand PrintEcrTicketCommand { get; private set; }
        #endregion
    }
}
