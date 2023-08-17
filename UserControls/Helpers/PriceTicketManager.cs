using System;
using System.Linq;
using System.Windows.Controls;
using Es.Market.Tools.Controls;
using ES.Business.Managers;
using ES.Common;
using UserControls.Controls;
using UserControls.PriceTicketControl;
using UserControls.PriceTicketControl.Helper;
using UserControls.PriceTicketControl.ViewModels;
using ProductModel = ES.Data.Models.Products.ProductModel;

namespace UserControls.Helpers
{
    public class PriceTicketManager
    {
        public static void PrintPriceTicket(PrintPriceTicketEnum? printPriceTicketEnum, ProductModel product, int count)
        {
            PrintManager.Print(GetPriceTag(printPriceTicketEnum, product), false);
        }

        public static void PrintPriceTicket(PrintPriceTicketEnum? printPriceTicketEnum, ProductModel product = null)
        {


            PrintManager.PrintPreview(GetPriceTag(printPriceTicketEnum, product), "Գնապիտակ", HgConvert.ToBoolean(printPriceTicketEnum));

            //if (HgConvert.ToBoolean(o))
            //{
            //    PrintManager.Print(ctrl, true);
            //}
            //else
            //{
            //    var pb = new UiPrintPreview(ctrl);
            //    pb.Show();
            //    pb.Print();
            //    pb.Close();
            //}
        }

        private static UserControl GetPriceTag(PrintPriceTicketEnum? printPriceTicketEnum, ProductModel product)
        {
            if (printPriceTicketEnum == null)
            {
                return null;
            }

            if (printPriceTicketEnum == PrintPriceTicketEnum.Normal)
            {
                new PriceTagViewDialog().Show();
                return null;
            }
            switch (printPriceTicketEnum)
            {
                case PrintPriceTicketEnum.Normal:
                    //new PriceTagViewDialog().Show();
                    return null;
                    break;
                case PrintPriceTicketEnum.PriceTag:
                    new LabelPrinter().Show();
                    return null;
                    break;
            }

            if (product == null)
            {
                product = SelectItemsManager.SelectProduct(ApplicationManager.CashManager.GetProducts().Where(s => !string.IsNullOrEmpty(s.Barcode)).ToList()).FirstOrDefault();
            }
            if (product == null) return null;


            UserControl priceTicket = null;
            switch (printPriceTicketEnum)
            {
                case PrintPriceTicketEnum.Normal:
                    break;
                case PrintPriceTicketEnum.Small:
                    priceTicket = new UctrlBarcodeWithText(new BarcodeViewModel(product.Code, product.Barcode, product.Description, product.Price, null));
                    break;
                case PrintPriceTicketEnum.Large:
                    priceTicket = new UctrlBarcodeX(new BarcodeViewModel(product.Code, product.Barcode, product.Description, product.Price, null));
                    break;
                case PrintPriceTicketEnum.LargePrice:
                    //var priceTicket = new PriceTicketDialog
                    //{
                    //    DataContext = new PriceTicketLargePriceVewModel(product.Code, product.Barcode, product.Description,product.Price, null)
                    //};
                    //priceTicket.Show();

                    //            Barcode barcode = new Barcode()
                    //{
                    //    IncludeLabel = true,
                    //    Alignment = AlignmentPositions.CENTER,
                    //    Width = 300,
                    //    Height = 100,
                    //    RotateFlipType = RotateFlipType.RotateNoneFlipNone,
                    //    BackColor = Brushes.White,
                    //    ForeColor = Brushes.Black,
                    //};

                    //var img = barcode.Encode(TYPE.EAN13, product.Barcode);

                    priceTicket = new UctrlPriceTicket(new PriceTicketLargePriceVewModel(product.Code, product.Barcode, product.Description, product.Price, null));
                    break;
                case PrintPriceTicketEnum.PriceOnly:
                    priceTicket = new UctrlPriceTicket(new PriceTicketVewModel(product.Code, product.Barcode, string.Format("{0} ({1})", product.Description, product.Code), product.Price, null));
                    break;
                case PrintPriceTicketEnum.PriceTag:
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("printPriceTicketEnum", printPriceTicketEnum, null);
            }

            return priceTicket;
        }
    }
}
