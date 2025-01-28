using CashReg.Models;
using ES.Data.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace UserControls.Views.ReceiptTickets
{
    public class SaleInvoiceSmallTicketViewModel : ResponseReceiptViewModel, INotifyPropertyChanged
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
        public InvoiceModel Invoice { get { return _invoice; } set { _invoice = value; OnPropertyChanged("Invoice"); } }
        public List<InvoiceItemsModel> InvoiceItems { get { return _invoiceItems; } set { _invoiceItems = value; OnPropertyChanged("InvoiceItems"); OnPropertyChanged("EmarkCount"); OnPropertyChanged("HasEmark"); } }
        public PartnerModel Partner => Invoice.Partner;
        public InvoicePaid InvoicePaid
        {
            get { return _invoicePaid; }
            set { _invoicePaid = value; OnPropertyChanged("InvoicePaid"); }
        }
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(TitleProperty); } }
        public string Footer { get { return _footer; } set { _footer = value; OnPropertyChanged(FooterProperty); } }
        public string Prize { get { return ResponseReceipt != null && ResponseReceipt.Prize == (int)CashReg.Helper.Prize.Prize ? Shared.Helpers.Enumerations.GetEnumDescription(CashReg.Helper.Prize.Prize) : Shared.Helpers.Enumerations.GetEnumDescription(CashReg.Helper.Prize.NoPrize); } }
        public DateTime? Date { get { return Invoice != null ? Invoice.ApproveDate ?? Invoice.CreateDate : (DateTime?)null; } }
        public string Department { get; set; }
        public bool IsCheck { get { return InvoicePaid != null && InvoicePaid.ByCheck > 0; } }
        public bool IsAccountsReceivable { get { return InvoicePaid != null && InvoicePaid.AccountsReceivable > 0; } }
        public bool IsReceivedPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
        public bool IsReceiptTicket { get { return ResponseReceipt != null; } }
        public bool IsLottery { get { return ResponseReceipt != null && !string.IsNullOrEmpty(ResponseReceipt.Lottery); } }
        public bool IsFiscal { get { return ResponseReceipt != null && !string.IsNullOrEmpty(ResponseReceipt.Fiscal); } }
        public decimal Discount { get { return InvoicePaid != null ? Invoice.Amount - Invoice.Total : 0; } }
        public bool IsReceiptExists { get { return ResponseReceipt != null; } }
        public int EmarkCount { get { return InvoiceItems.Sum(s => s.AdditionalData.EMarks.Count); } }
        public bool HasEmark { get { return EmarkCount > 0; } }
        public bool HasPartnerTin { get => !string.IsNullOrWhiteSpace(Invoice.Partner.TIN); }

        public ImageSource QrImage { get; set; }
        #endregion
        public SaleInvoiceSmallTicketViewModel(ResponseReceiptModel responceReceiptModel) : base(responceReceiptModel)
        {
            //QRCodeGenerator qrGenerator = new QRCodeGenerator();
            //QRCodeData qrCodeData = qrGenerator.CreateQrCode("string", QRCodeGenerator.ECCLevel.Q);
            //PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            if (ResponseReceipt != null && !string.IsNullOrEmpty(ResponseReceipt.Qr)) QrImage = GenerateQRCodeToBitmapImage(ResponseReceipt.Qr);
        }
        public static BitmapImage GenerateQRCodeToBitmapImage(string qrText)
        {
            // Step 1: Generate QRCodeData using QRCoder
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);

                // Step 2: Render QRCode to a Bitmap
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    using (Bitmap qrBitmap = qrCode.GetGraphic(20)) // Scale factor: 20
                    {
                        // Step 3: Convert Bitmap to BitmapImage
                        return ConvertBitmapToBitmapImage(qrBitmap);
                    }
                }
            }
        }

        private static BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Save Bitmap to MemoryStream
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;

                // Load MemoryStream into BitmapImage
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it cross-thread accessible

                return bitmapImage;
            }
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
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
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
    public class PurchaseInvocieTicketViewModel : INotifyPropertyChanged
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
        public bool IsPrepayment { get { return InvoicePaid != null && InvoicePaid.ReceivedPrepayment > 0; } }
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
        public MoveInvocieTicketViewModel(InvoiceModel invoice, List<InvoiceItemsModel> invoiceItems, StockModel fromStock, StockModel toStock)
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
