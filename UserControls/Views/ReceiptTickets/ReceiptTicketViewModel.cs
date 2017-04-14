using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Policy;
using CashReg.Models;
using ES.Data.Model;
using ES.Data.Models;
using Shared.Helpers;


namespace UserControls.Views.ReceiptTickets
{
    public class SaleInvocieSmallTicketViewModel : ResponseReceiptViewModel, INotifyPropertyChanged
    {
        #region Properties

        private const string TitleProperty = "Title";
        private const string FooterProperty = "Footer";
        #endregion
        #region Internal properties
        private InvoiceModel _invoice;
        private List<InvoiceItemsModel> _invoiceItems;
        private InvoicePaid _invoicePaid;
        private string _title = "Վաճառքի կտրոն";
        private string _footer = "Շնորհակալություն գնումների համար";
        #endregion
        #region External properties
        public EsMemberModel Member { get; set; }
        public EsUserModel User { get; set; }
        public InvoiceModel Invoice { get { return _invoice; } set { _invoice = value;  OnPropertyChanged("Invocie");} }
        public List<InvoiceItemsModel> InvoiceItems { get { return _invoiceItems; } set { _invoiceItems = value; OnPropertyChanged("InvoiceItems"); } }
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; OnPropertyChanged("InvoicePaid");}
        }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(TitleProperty); } }
        public string Footer { get { return _footer; } set { _footer = value; OnPropertyChanged(FooterProperty);} }
        public string Prize { get { return ResponseReceipt!=null && ResponseReceipt.Prize == (int)CashReg.Helper.Prize.Prize ? Shared.Helpers.Enumerations.GetEnumDescription(CashReg.Helper.Prize.Prize) : Shared.Helpers.Enumerations.GetEnumDescription(CashReg.Helper.Prize.NoPrize); } }
        public DateTime? Date { get {return Invoice != null ? Invoice.ApproveDate ?? Invoice.CreateDate : (DateTime?) null;} }
        public bool IsCheck { get { return InvoicePaid != null && InvoicePaid.ByCheck > 0; } }
        public bool IsAccountsReceivable { get { return InvoicePaid != null && InvoicePaid.AccountsReceivable > 0; } }
        public bool IsReceivedPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.Prepayment > 0; } }
        public bool IsReceiptTicket { get { return ResponseReceipt != null; }}
        public bool IsLottery { get { return ResponseReceipt != null && !string.IsNullOrEmpty(ResponseReceipt.Lottery); } }
        public bool IsFiscal { get { return ResponseReceipt != null && !string.IsNullOrEmpty(ResponseReceipt.Fiscal); } }
        public decimal Discount { get { return InvoicePaid != null ? Invoice.Amount - Invoice.Total:0; } }
        public bool IsReceiptExists { get { return ResponseReceipt != null; } }
        
        #endregion
        public SaleInvocieSmallTicketViewModel(ResponseReceiptModel responceReceiptModel): base(responceReceiptModel)
        {
            
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    public class SaleInvocieTicketViewModel : INotifyPropertyChanged
    {
        #region Properties

        private const string TitleProperty = "Title";
        private const string FooterProperty = "Footer";
        #endregion
        #region Internal properties
        private InvoiceModel _invoice;
        private List<InvoiceItemsModel> _invoiceItems;
        private InvoicePaid _invoicePaid;
        private string _title = "Գնման ապրանքագիր";
        private string _footer = "";
        #endregion
        #region External properties
        public EsMemberModel Member { get; set; }
        public EsUserModel User { get; set; }
        public InvoiceModel Invoice { get { return _invoice; } set { _invoice = value; OnPropertyChanged("Invocie"); } }
        public List<InvoiceItemsModel> InvoiceItems { get { return _invoiceItems; } set { _invoiceItems = value; OnPropertyChanged("InvoiceItems"); } }
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; OnPropertyChanged("InvoicePaid"); }
        }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(TitleProperty); } }
        public string Footer { get { return _footer; } set { _footer = value; OnPropertyChanged(FooterProperty); } }
        public DateTime? Date { get { return Invoice != null ? Invoice.ApproveDate ?? Invoice.CreateDate : (DateTime?)null; } }
        public bool IsCheck { get { return InvoicePaid != null && InvoicePaid.ByCheck > 0; } }
        public bool IsAccountsReceivable { get { return InvoicePaid != null && InvoicePaid.AccountsReceivable > 0; } }
        public bool IsReceivedPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.Prepayment > 0; } }
        public decimal Discount { get { return InvoicePaid != null ? Invoice.Amount - Invoice.Total : 0; } }
        public StockModel FromStock { get; set; }
        #endregion
        public SaleInvocieTicketViewModel(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, StockModel fromStock, InvoicePaid paid, EsMemberModel member)
        {
            Member = member;
            Invoice = invoice;
            InvoiceItems = invoiceItems;
            FromStock = fromStock;
            InvoicePaid = paid;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    public class PurchaseInvocieTicketViewModel: INotifyPropertyChanged
    {
        #region Properties

        private const string TitleProperty = "Title";
        private const string FooterProperty = "Footer";
        #endregion
        #region Internal properties
        private InvoiceModel _invoice;
        private List<InvoiceItemsModel> _invoiceItems;
        private InvoicePaid _invoicePaid;
        private string _title = "Գնման ապրանքագիր";
        private string _footer = "";
        #endregion
        #region External properties
        public EsMemberModel Member { get; set; }
        public EsUserModel User { get; set; }
        public InvoiceModel Invoice { get { return _invoice; } set { _invoice = value; OnPropertyChanged("Invocie"); } }
        public List<InvoiceItemsModel> InvoiceItems { get { return _invoiceItems; } set { _invoiceItems = value; OnPropertyChanged("InvoiceItems"); } }
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; OnPropertyChanged("InvoicePaid"); }
        }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(TitleProperty); } }
        public string Footer { get { return _footer; } set { _footer = value; OnPropertyChanged(FooterProperty); } }
        public DateTime? Date { get { return Invoice != null ? Invoice.ApproveDate ?? Invoice.CreateDate : (DateTime?)null; } }
        public bool IsCheck { get { return InvoicePaid != null && InvoicePaid.ByCheck > 0; } }
        public bool IsAccountsReceivable { get { return InvoicePaid != null && InvoicePaid.AccountsReceivable > 0; } }
        public bool IsReceivedPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.Prepayment > 0; } }
        public decimal Discount { get { return InvoicePaid != null ? Invoice.Amount - Invoice.Total : 0; } }
        public StockModel ToStock { get; set; }
        #endregion
        public PurchaseInvocieTicketViewModel(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, StockModel toStock, InvoicePaid paid)
        {
            Invoice = invoice;
            InvoiceItems = invoiceItems;
            ToStock = toStock;
            InvoicePaid = paid;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    public class MoveInvocieTicketViewModel : INotifyPropertyChanged
    {
        #region Properties

        private const string TitleProperty = "Title";
        private const string FooterProperty = "Footer";
        #endregion
        #region Internal properties
        private InvoiceModel _invoice;
        private List<InvoiceItemsModel> _invoiceItems;
        private string _title = "Տեղափոխման ապրանքագիր";
        private string _footer = "";
        #endregion
        #region External properties
        public EsMemberModel Member { get; set; }
        public EsUserModel User { get; set; }
        public InvoiceModel Invoice { get { return _invoice; } set { _invoice = value; OnPropertyChanged("Invocie"); } }
        public List<InvoiceItemsModel> InvoiceItems { get { return _invoiceItems; } set { _invoiceItems = value; OnPropertyChanged("InvoiceItems"); } }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(TitleProperty); } }
        public string Footer { get { return _footer; } set { _footer = value; OnPropertyChanged(FooterProperty); } }
        public DateTime? Date { get { return Invoice != null ? Invoice.ApproveDate ?? Invoice.CreateDate : (DateTime?)null; } }
        public StockModel FromStock { get; set; }
        public StockModel ToStock { get; set; }
        #endregion
        public MoveInvocieTicketViewModel(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems,StockModel fromStock, StockModel toStock)
        {
            Invoice = invoice;
            InvoiceItems = invoiceItems;
            FromStock = fromStock;
            ToStock = toStock;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
