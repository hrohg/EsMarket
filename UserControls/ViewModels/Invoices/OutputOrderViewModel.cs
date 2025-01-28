using ES.Business.Managers;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.Utilits;
using ES.Data.Models;
using ES.DataAccess.Models;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets;
using UserControls.Views.ReceiptTickets.Views;
using UserControls.Views.Windows;

namespace UserControls.ViewModels.Invoices
{
    public class OutputOrderViewModel : InvoiceViewModel
    {
        #region Intrnal properties
        private SaleInvoiceUserWindow _saleInvoiceUserWindow;
        #endregion Internal propertirs

        #region External properties
        public override bool AddBySingle
        {
            get { return base.AddBySingle; }
            set
            {
                base.AddBySingle = value;
                ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle = value;
            }
        }
        public override bool DenyChangeQuantity
        {
            get { return ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller) || base.DenyChangeQuantity; }
        }
        public override bool DenyChangePrice
        {
            get { return !ApplicationManager.IsInRole(UserRoleEnum.SaleManager) && !ApplicationManager.IsInRole(UserRoleEnum.Manager) || ApplicationManager.IsInRole(UserRoleEnum.JuniorSeller) || base.DenyChangePrice; }
        }
        public override StockModel FromStock
        {
            get { return base.FromStock; }
            set
            {
                base.FromStock = value;
                Invoice.ProviderName = value != null ? value.FullName : string.Empty;
            }
        }
        public override StockModel ToStock
        {
            get { return base.ToStock; }
            set
            {
                base.ToStock = value;
            }
        }
        public override bool IsActive
        {
            get => base.IsActive;
            set
            {
                base.IsActive = value;
            }
        }
        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                base.IsSelected = value;
                MonitorInfo screen = ScreenUtilits.GetSecondaryScreens().SingleOrDefault(s => s.DisplayName == ApplicationManager.Settings.SettingsContainer.MemberSettings.SecondaryScreenName);

                if (IsSelected && screen != null)
                {
                    if (_saleInvoiceUserWindow == null)
                    {
                        _saleInvoiceUserWindow = new SaleInvoiceUserWindow(this, screen);
                    }
                    _saleInvoiceUserWindow.Show();
                }
                else
                {
                    if (_saleInvoiceUserWindow != null) _saleInvoiceUserWindow.Hide();
                }
            }
        }
        #endregion External properties

        #region Contructors
        protected OutputOrderViewModel(InvoiceType type)
            : base(type)
        {

        }

        protected OutputOrderViewModel(Guid id)
            : base(id)
        {
        }
        #endregion Constructors

        #region Internal methods
        private void OnInputEmark(InvoiceItemsModel invoiceModel)
        {
            var isAxciseItem = invoiceModel != null && invoiceModel.Product != null && Ecr.Manager.Helpers.MarkHelper.GetEmarkExciseList().Contains(invoiceModel.Product.HcdCs);
            if (isAxciseItem)
            {
                string eMark;
                while (invoiceModel.Quantity < invoiceModel.AdditionalData.EMarks.Count)
                {
                    WaitingEmark = true;
                    eMark = Ecr.Manager.Helpers.MarkHelper.ReadEmark(string.Format("Մուտքագրել հեռացվող ապրանքի նույնականացման կոդը ({0}):", Description));
                    WaitingEmark = false;
                    if (eMark == null) break;
                    invoiceModel.AdditionalData.EMarks.Remove(eMark);
                }
                while (invoiceModel.Quantity > invoiceModel.AdditionalData.EMarks.Count)
                {
                    eMark = GetInvoiceItemEMark(string.Format("Մուտքագրել ապրանքի նույնականացման կոդը ({0}):", Description));
                    if (eMark == null) break;
                    if (!string.IsNullOrWhiteSpace(eMark)) invoiceModel.AdditionalData.EMarks.Add(eMark);
                }
            }
        }
        protected string[] GetEmarks()
        {
            var eMarks = new List<string>();
            foreach (var invoiceItem in InvoiceItems)
            {
                var emarks = invoiceItem.AdditionalData.EMarks.ToList();
                eMarks.AddRange(emarks);
            }
            return eMarks.ToArray();
        }
        protected override void OnInitialize()
        {
            base.OnInitialize();
            if (FromStocks == null) FromStocks = StockManager.GetStocks(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSaleStocks.ToList()).ToList();
            AddBySingle = ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBySingle;
            InputEmarkCommand = new RelayCommand<InvoiceItemsModel>(OnInputEmark);
        }
        protected override void LoadInvoiceItem(InvoiceItemsModel invoiceItem)
        {
            var productPrice = GetProductPrice(invoiceItem.Product);
            if (invoiceItem.Price != productPrice)
            {
                if (MessageBox.Show(string.Format("{0} գինը {1} փոփոխվել է: Ցանկանու՞մ եք կիրառել նոր գինը {2}:", invoiceItem.Description, invoiceItem.Price, productPrice), "Գնի փոփոխություն", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    invoiceItem.Price = productPrice;
                }
            }
            base.LoadInvoiceItem(invoiceItem);
        }
        public override void SetExternalText(ExternalTextImputEventArgs e)
        {
            if (!WaitingEmark) base.SetExternalText(e);
            else { SendTextToFocusedElement(e.Text); }
        }
        protected override void OnPartnerChanged()
        {
            base.OnPartnerChanged();
        }
        protected override bool SetQuantity(bool addSingle)
        {
            var exCount = ProductsManager.GetProductItemQuantity(InvoiceItem.ProductId, FromStocks.Select(s => s.Id).ToList());

            if (InvoiceItem.Quantity > exCount)
            {
                InvoiceItem.Quantity = null;
                var message = string.Format("Անբավարար միջոցներ: Կոդ: {0} Տվյալ ապրանքատեսակից բավարար քանակ առկա չէ:", InvoiceItem.Code);
                MessageManager.OnMessage(new MessageModel(DateTime.Now, message, MessageTypeEnum.Warning));
                MessageManager.ShowMessage(message, "Անբավարար միջոցներ");
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

            return InvoiceItem.Quantity != null && InvoiceItem.Quantity > 0;
        }
        protected override bool CanApprove(object o)
        {
            return base.CanApprove(o) && FromStocks.Any();
        }
        protected override void OnApproveAsync(bool closeOnExit)
        {
            lock (_sync)
            {
                if (!CanApprove(null))
                {
                    MessageManager.OnMessage("Գործողության ձախողում:", MessageTypeEnum.Warning);
                    return;
                }

                IsLoading = true;
                PrepareToApprove();
            }
        }
        protected virtual void PrepareToApprove()
        {
            Invoice.ApproverId = ApplicationManager.GetEsUser.UserId;
            Invoice.Approver = ApplicationManager.GetEsUser.FullName;

            InvoicePaid.PartnerId = Invoice.PartnerId;
            Invoice.RecipientName = Partner.FullName;

            //Paid
            CashDesk cashDesk = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                cashDesk = InvoicePaid.Paid > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleCashDesks).SingleOrDefault() : null;
            }));
            InvoicePaid.CashDeskId = cashDesk != null ? cashDesk.Id : (Guid?)null;

            CashDesk bankAccount = null;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                bankAccount = InvoicePaid.ByCheck > 0 ? SelectItemsManager.SelectCashDesksByIds(ApplicationManager.Settings.SettingsContainer.MemberSettings.SaleBankAccounts).SingleOrDefault() : null;
            }));

            InvoicePaid.CashDeskForTicketId = bankAccount != null ? bankAccount.Id : (Guid?)null;
        }
        /// <summary>
        /// Set quantity and add invoice item
        /// </summary>
        /// <param name="o"></param>
        protected override void PreviewAddInvoiceItem(object o)
        {
            SetQuantity(AddBySingle);
            base.PreviewAddInvoiceItem(o);
        }
        /// <summary>
        /// Add invoice item and check for emarks
        /// </summary>
        /// <param name="invoiceItem"></param>
        protected override void OnAddInvoiceItem(InvoiceItemsModel invoiceItem)
        {
            OnInputEmark(invoiceItem);
            base.OnAddInvoiceItem(invoiceItem);
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
            PreviewAddInvoiceItem(o);
        }

        protected override void OnExportInvoice(ExportImportEnum exportTo)
        {
            switch (exportTo)
            {
                case ExportImportEnum.AccDocXml:
                    InvoicesManager.ExportInvoiceToXmlAccDoc(Invoice, InvoiceItems.ToList());
                    break;
                case ExportImportEnum.Xml:
                case ExportImportEnum.Excel:
                    base.OnExportInvoice(exportTo);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exportTo", exportTo, null);
            }
        }
        protected override void OnImportInvoice(ExportImportEnum importFrom)
        {
            switch (importFrom)
            {
                case ExportImportEnum.Xml:
                    ImportFromXml();
                    break;
                case ExportImportEnum.AccDocXml:
                    break;
                case ExportImportEnum.Excel:
                    ImportFromExcel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("importFrom", importFrom, null);
            }
        }

        protected override void OnPrintInvoice(PrintModeEnum printSize)
        {
            if (!CanPrintInvoice(printSize))
            {
                return;
            }
            //var list = CollectionViewSource.GetDefaultView(InvoiceItems).Cast<InvoiceItemsModel>().ToList();
            var ctrl = new SaleInvoiceView(new SaleInvocieTicketViewModel(Invoice, InvoiceItems.ToList(), StockManager.GetStock(Invoice.FromStockId), InvoicePaid, Member));

            switch (printSize)
            {
                case PrintModeEnum.Normal:
                    PrintManager.PrintPreview(ctrl, "Print invoices", true);
                    break;
                case PrintModeEnum.Small:
                case PrintModeEnum.Large:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printSize", printSize, null);
            }
        }

        private bool CheckBeforeApprove()
        {
            if (InvoiceItems.Any(s => s.Quantity == 0))
            {
                MessageBox.Show("0-յական քանակով ելքագրում արտոնված չէ։ Ստուգեք ելքագրվող ապրանքների քանակը։",
                    "Չարտոնված գործողություն", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        protected override void OnApprove(object o)
        {
            //Approve Invoice
            if (CheckBeforeApprove()) new Thread(() => OnApproveAsync(false)).Start();
        }

        #endregion Internal methods

        #region External methods
        public override void OnApproveAndClose(object o)
        {
            //Approve Invoice
            if (CheckBeforeApprove()) new Thread(() => OnApproveAsync(true)).Start();
        }
        public override bool Close()
        {
            if (_saleInvoiceUserWindow != null) _saleInvoiceUserWindow.Close();
            _saleInvoiceUserWindow = null;
            return base.Close();
        }
        #endregion External methods

        #region Commands
        public ICommand InputEmarkCommand { get; private set; }
        private ICommand _checkExistingCommand;
        public ICommand CheckExistingCommand
        {
            get { return _checkExistingCommand ?? (_checkExistingCommand = new RelayCommand(OnCheckExisting, CanCheckExisting)); }
        }

        private bool CanCheckExisting(object obj)
        {
            return CanApprove(null);
        }

        private void OnCheckExisting(object obj)
        {
            InvoicesManager.CheckForQuantityExisting(InvoiceItems.ToList(), FromStocks.Select(s => s.Id));
        }

        #endregion Commands
    }
}
