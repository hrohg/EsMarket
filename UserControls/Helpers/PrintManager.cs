using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using Shared.Helpers;
using UserControls.Views.PrintPreview;
using UserControls.Views.PrintPreview.Views;

namespace UserControls.Helpers
{
    public class PrintManager
    {

        #region Constructors
        public PrintManager()
        {

        }
        #endregion

        #region Internal methods
        private static FixedDocument GetFixedDocument(FrameworkElement toPrint, PrintDialog printDialog)
        {
            PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
            if (capabilities.PageImageableArea == null) return null;
            Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
            Size visibleSize = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
            FixedDocument fixedDoc = new FixedDocument();

            //If the toPrint visual is not displayed on screen we neeed to measure and arrange it  
            toPrint.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            toPrint.Arrange(new Rect(new Point(0, 0), toPrint.DesiredSize));

            toPrint.UpdateLayout();
            //  
            Size size = toPrint.DesiredSize;
            //Will assume for simplicity the control fits horizontally on the page  
            double yOffset = 0;
            while (yOffset < size.Height)
            {
                VisualBrush vb = new VisualBrush(toPrint);
                vb.Stretch = Stretch.None;
                vb.AlignmentX = AlignmentX.Left;
                vb.AlignmentY = AlignmentY.Top;
                vb.ViewboxUnits = BrushMappingMode.Absolute;
                vb.TileMode = TileMode.None;
                vb.Viewbox = new Rect(0, yOffset, visibleSize.Width, visibleSize.Height);
                PageContent pageContent = new PageContent();
                FixedPage page = new FixedPage();
                ((IAddChild)pageContent).AddChild(page);
                fixedDoc.Pages.Add(pageContent);
                page.Width = pageSize.Width;
                page.Height = pageSize.Height;
                Canvas canvas = new Canvas();
                FixedPage.SetLeft(canvas, capabilities.PageImageableArea.OriginWidth);
                FixedPage.SetTop(canvas, capabilities.PageImageableArea.OriginHeight);
                canvas.Width = visibleSize.Width;
                canvas.Height = visibleSize.Height;
                canvas.Background = vb;
                page.Children.Add(canvas);
                yOffset += visibleSize.Height;
            }
            return fixedDoc;
        }
        private static void ShowPrintPreview(FixedDocument fixedDoc)
        {
            Window wnd = new Window();
            DocumentViewer viewer = new DocumentViewer();
            viewer.Document = fixedDoc;
            wnd.Content = viewer;
            wnd.ShowDialog();
        }
        #endregion

        #region External methods
        public static void Print(StackPanel ctrl)
        {
            var pDialog = new PrintDialog();
            try
            {
                var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                ctrl.MaxHeight = printableArea.Height;
                ctrl.MaxWidth = printableArea.Width;
                ctrl.Measure(printableArea);
                ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
                ctrl.UpdateLayout();
                pDialog.PageRangeSelection = PageRangeSelection.AllPages;
                pDialog.PrintVisual(ctrl, "Print");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static void Print(StackPanel ctrl, string title, bool showDialog)
        {
            PrintEx(ctrl); return;
            var pDialog = new PrintDialog
            {
                PageRangeSelection = PageRangeSelection.AllPages,
                UserPageRangeEnabled = true
            };

            //fd.PageHeight = pDialog.PrintableAreaHeight;
            //fd.PageWidth = pDialog.PrintableAreaWidth;
            //fd.PagePadding = new Thickness(50);
            //fd.ColumnGap = 0;
            //fd.ColumnWidth = pDialog.PrintableAreaWidth;

            //var doc = GetFixedDocument(ctrl, pDialog);
            //if (doc != null && (!showDialog || pDialog.ShowDialog() == true))
            //{
            //    IDocumentPaginatorSource dps = doc;
            //    var documentPaginator = dps.DocumentPaginator;
            //    if (documentPaginator.IsPageCountValid)
            //    {
            //        pDialog.PrintDocument(documentPaginator, title);
            //    }
            //}
            //ShowPrintPreview(doc);

            try
            {
                var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                PrintCapabilities capabilities = pDialog.PrintQueue.GetPrintCapabilities(pDialog.PrintTicket);
                if (capabilities.PageImageableArea == null) return;
                double scale = capabilities.PageImageableArea.ExtentWidth / ctrl.ActualWidth;
                ctrl.Arrange(new Rect(0, 0, ctrl.ActualWidth * scale, ctrl.ActualHeight * scale));
                ctrl.UpdateLayout();



                if (!showDialog || pDialog.ShowDialog() == true)
                {
                    pDialog.PrintVisual(ctrl, "Print");

                    Thickness margins = new Thickness(96);

                    PrintDocumentImageableArea area = null;
                    PageRangeSelection selection = PageRangeSelection.AllPages;
                    PageRange range = new PageRange();

                    XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(pDialog.PrintQueue.Name, ref area, ref selection, ref range);
                    SerializerWriterCollator batchPrinter = xpsdw.CreateVisualsCollator();
                    Size outputSize = new Size(
                    area.MediaSizeWidth,
                    area.MediaSizeHeight);


                    FrameworkElement element = ctrl; //scrollViewer.Content as FrameworkElement; 
                    bool originalClipToBounds = element.ClipToBounds;
                    Geometry originalClip = element.Clip;

                    Size elementSize = new Size(outputSize.Width, element.ActualHeight);

                    double currHeight = outputSize.Height;
                    int pageCounter = 1;
                    while (currHeight < element.ActualHeight)
                    {
                        if (pDialog.PageRange.PageFrom <= pageCounter && pDialog.PageRange.PageTo >= pageCounter)
                        {
                            elementSize.Height -= outputSize.Height;
                            element.Margin = new Thickness(0, -currHeight, 0, 0);
                            element.Clip = new RectangleGeometry(new Rect(new Point(0, currHeight), outputSize));
                            element.Measure(elementSize);
                            element.Arrange(new Rect(new Point(0, 0), elementSize));

                            pDialog.PrintVisual(element, "Print");
                        }
                        currHeight += outputSize.Height;
                        pageCounter++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void Print(Control ctrl, bool showPrintDialog = false)
        {
            var pDialog = new PrintDialog { UserPageRangeEnabled = true };

            var doc = GetFixedDocument(ctrl, pDialog);
            try
            {
                //var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                //ctrl.MaxHeight = printableArea.Height;
                //ctrl.MaxWidth = printableArea.Width;
                //ctrl.Measure(printableArea);
                //ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
                //ctrl.UpdateLayout();

                if (!showPrintDialog || pDialog.ShowDialog() == true)
                {
                    pDialog.PageRangeSelection = PageRangeSelection.UserPages;
                    //pDialog.PrintVisual(ctrl, "Print");
                    var documentPaginator = doc.DocumentPaginator;
                    if (documentPaginator.IsPageCountValid)
                    {
                        pDialog.PrintDocument(documentPaginator, "Print document");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion


        public static void PrintPreview(UserControl uctrl, string title, bool isShowPrintDialog)
        {
            var pp = new UiPrintPreview(uctrl);
            pp.DataContext = new PrintPreviewViewModel(pp, title, isShowPrintDialog);
            pp.Show();
        }
        public static void Print(UserControl uctrl)
        {
            var pDialog = new PrintDialog();
            pDialog.UserPageRangeEnabled = true;
            pDialog.UserPageRangeEnabled = true;
            if (pDialog.ShowDialog() == true)
            {
                pDialog.PageRangeSelection = PageRangeSelection.UserPages;
                pDialog.PrintVisual(uctrl, "Barcode");
            }
        }

        //public static void Print(FrameworkElement ctrl, bool showDialog = false)
        //{
        //    if(ctrl==null) return;

        //    PrintDialog pDialog = new PrintDialog();
        //    pDialog.UserPageRangeEnabled = true;
        //    pDialog.PageRangeSelection = PageRangeSelection.UserPages;

        //        var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
        //        ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
        //        ctrl.UpdateLayout();


        //        PrintCapabilities capabilities = pDialog.PrintQueue.GetPrintCapabilities(pDialog.PrintTicket);
        //        if (capabilities.PageImageableArea == null) return;
        //        //get scale of the print wrt to screen of WPF visual
        //        double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / ctrl.ActualWidth, capabilities.PageImageableArea.ExtentHeight / ctrl.ActualHeight);

        //        //Transform the Visual to scale
        //        ctrl.LayoutTransform = new ScaleTransform(scale, scale);

        //        //get the size of the printer page
        //        Size sz = new Size(ctrl.ActualWidth * scale, ctrl.ActualHeight * scale); //(8.5 * 96.0, 11.0 * 96.0);

        //        //update the layout of the visual to the printer page size.
        //        ctrl.Measure(sz);
        //        ctrl.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
        //        ctrl.UpdateLayout();


        //        var dataGrid = ctrl.FindChild<DataGrid>();
        //        foreach (var dataGridColumn in dataGrid.Columns)
        //        {
        //            var width = dataGridColumn.Width;
        //            dataGridColumn.Width = 0;
        //            dataGridColumn.Width = width.UnitType == DataGridLengthUnitType.Star ? dataGrid.ActualWidth - dataGrid.Columns.Sum(s => s.Width.DesiredValue) : width.DesiredValue;
        //        }
        //        dataGrid.UpdateLayout();
        //    pDialog.PrintVisual(ctrl, "Print");

        //}

        public static void PrintEx(FrameworkElement objectToPrint)
        {

            PrintDocumentImageableArea area = null;
            PageRangeSelection selection = PageRangeSelection.AllPages;
            PageRange range = new PageRange();
            XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(objectToPrint.Name, ref area, ref selection, ref range);
            if(area==null) return;
            Thickness margins = new Thickness(area.MediaSizeHeight>200?96:0);
            if (xpsdw != null)
            {
                double leftMargin = area.MediaSizeWidth - area.ExtentWidth - area.OriginWidth;// margins.Left - area.OriginWidth; 
                double topMargin = margins.Top - area.OriginHeight;
                double rightMargin = area.MediaSizeWidth - area.ExtentWidth - area.OriginWidth;// margins.Right - (area.MediaSizeWidth - area.ExtentWidth - area.OriginWidth); 
                double bottomMargin = margins.Bottom - (area.MediaSizeHeight - area.ExtentHeight - area.OriginHeight);
                Size outputSize = new Size(area.MediaSizeWidth - leftMargin - rightMargin, area.MediaSizeHeight - topMargin - bottomMargin);

                FrameworkElement element = objectToPrint; //scrollViewer.Content as FrameworkElement; 
                bool originalClipToBounds = element.ClipToBounds;
                Geometry originalClip = element.Clip;
                Thickness originalMargin = element.Margin;
                Size elementSize = new Size(outputSize.Width, element.ActualHeight);
                element.ClipToBounds = true;
                element.Clip = new RectangleGeometry(new Rect(outputSize));
                element.Measure(elementSize);
                element.Arrange(new Rect(new Point(leftMargin, topMargin), elementSize));

                SerializerWriterCollator batchPrinter = xpsdw.CreateVisualsCollator();
                batchPrinter.BeginBatchWrite();
                double currHeight = 0;
                int pageCounter = 1;
                while (currHeight < element.ActualHeight)
                {

                    if (selection == PageRangeSelection.AllPages || pageCounter >= range.PageFrom && pageCounter <= range.PageTo)
                    {
                        element.Margin = new Thickness(0, -currHeight, 0, 0);
                        element.Clip = new RectangleGeometry(new Rect(new Point(0, currHeight), outputSize));
                        element.Measure(elementSize);
                        element.Arrange(new Rect(new Point(leftMargin, topMargin), elementSize));
                        batchPrinter.Write(element);
                        elementSize.Height -= elementSize.Height > outputSize.Height ? outputSize.Height : 0;
                    }
                    pageCounter++;
                    currHeight += outputSize.Height;
                }
                batchPrinter.EndBatchWrite();
                element.ClipToBounds = originalClipToBounds;
                element.Clip = originalClip;
                element.Margin = originalMargin;
            }
        }

        private PageContent generatePageContent(System.Drawing.Bitmap bmp, int top, int bottom, double pageWidth, double PageHeight, System.Printing.PrintCapabilities capabilities)
        {
            FixedPage printDocumentPage = new FixedPage();
            printDocumentPage.Width = pageWidth;
            printDocumentPage.Height = PageHeight;

            int newImageHeight = bottom - top;
            System.Drawing.Bitmap bmpPage = bmp.Clone(new System.Drawing.Rectangle(0, top,
                   bmp.Width, newImageHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a new bitmap for the contents of this page
            Image pageImage = new Image();
            BitmapSource bmpSource =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmpPage.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, newImageHeight));

            pageImage.Source = bmpSource;
            pageImage.VerticalAlignment = VerticalAlignment.Top;

            // Place the bitmap on the page
            printDocumentPage.Children.Add(pageImage);

            PageContent pageContent = new PageContent();
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(printDocumentPage);

            FixedPage.SetLeft(pageImage, capabilities.PageImageableArea.OriginWidth);
            FixedPage.SetTop(pageImage, capabilities.PageImageableArea.OriginHeight);

            pageImage.Width = capabilities.PageImageableArea.ExtentWidth;
            pageImage.Height = capabilities.PageImageableArea.ExtentHeight;
            return pageContent;
        }


        public static void PrintOnActivePrinter(UserControl ctrl, string activePrinter)
        {
            //var printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;
            try
            {
                PrintDialog pDialog = new PrintDialog();
                //var printServer = new PrintServer(printerName);
                pDialog.PrintQueue = new PrintServer("\\\\192.168.0.42").GetPrintQueue("Canon MF3200 Series");
                //pDialog.PrintQueue = new System.Printing.PrintQueue(new System.Printing.PrintServer(), "Canon MF3200 Series");
                //ctrl.MaxHeight = pDialog.PrintableAreaHeight;
                //ctrl.MaxWidth = pDialog.PrintableAreaWidth;


                var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
                ctrl.UpdateLayout();


                PrintCapabilities capabilities = pDialog.PrintQueue.GetPrintCapabilities(pDialog.PrintTicket);
                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / ctrl.ActualWidth, capabilities.PageImageableArea.ExtentHeight / ctrl.ActualHeight);

                //Transform the Visual to scale
                ctrl.LayoutTransform = new ScaleTransform(scale, scale);

                //get the size of the printer page
                Size sz = new Size(ctrl.ActualWidth * scale, ctrl.ActualHeight * scale); //(8.5 * 96.0, 11.0 * 96.0);

                //update the layout of the visual to the printer page size.
                ctrl.Measure(sz);
                ctrl.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                ctrl.UpdateLayout();




                //ctrl.Width = printableArea.Width;

                //ctrl.Arrange(new Rect(new Point(), printableArea));
                //ctrl.Measure(printableArea);



                var dataGrid = ctrl.FindChild<DataGrid>();
                foreach (var dataGridColumn in dataGrid.Columns)
                {
                    var width = dataGridColumn.Width;
                    dataGridColumn.Width = 0;
                    dataGridColumn.Width = width.UnitType == DataGridLengthUnitType.Star ? dataGrid.ActualWidth - dataGrid.Columns.Sum(s => s.Width.DesiredValue) : width.DesiredValue;
                }
                dataGrid.UpdateLayout();

                //dataGrid.Measure(printableArea);
                //dataGrid.Arrange(new Rect(new Point(0, 0), printableArea));
                //dataGrid.UpdateLayout();


                pDialog.PrintVisual(ctrl, "Print");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static void PrintDataGrid(UserControl ctrl, string printer)
        {
            try
            {
                PrintDialog pDialog = new PrintDialog();
                var printerServer = !string.IsNullOrEmpty(printer) &&printer.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).Count() > 1 ? 
                    string.Format("\\\\{0}", printer.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).First()) : string.Empty;
                var ps = string.IsNullOrEmpty(printerServer) ? new PrintServer() : new PrintServer(printerServer);
                pDialog.PrintQueue = ps.GetPrintQueue(System.IO.Path.GetFileName(printer));

                var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
                ctrl.UpdateLayout();


                PrintCapabilities capabilities = pDialog.PrintQueue.GetPrintCapabilities(pDialog.PrintTicket);
                if (capabilities.PageImageableArea == null) return;
                //get scale of the print wrt to screen of WPF visual
                //double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / ctrl.ActualWidth, capabilities.PageImageableArea.ExtentHeight / ctrl.ActualHeight);
                double scale = printableArea.Width / ctrl.ActualWidth;
                //Transform the Visual to scale
                ctrl.LayoutTransform = new ScaleTransform(scale, scale);

                //get the size of the printer page
                Size sz = new Size(ctrl.ActualWidth * scale, ctrl.ActualHeight * scale); //(8.5 * 96.0, 11.0 * 96.0);

                //update the layout of the visual to the printer page size.
                ctrl.Measure(sz);
                ctrl.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                ctrl.UpdateLayout();


                var dataGrid = ctrl.FindChild<DataGrid>();
                foreach (var dataGridColumn in dataGrid.Columns)
                {
                    var width = dataGridColumn.Width;
                    dataGridColumn.Width = 0;
                    dataGridColumn.Width = width.UnitType == DataGridLengthUnitType.Star ? dataGrid.ActualWidth - dataGrid.Columns.Sum(s => s.Width.DesiredValue) : width.DesiredValue;
                }
                dataGrid.UpdateLayout();
                pDialog.PrintVisual(ctrl, "Print");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static void Print(UserControl ctrl, string printer)
        {
            try
            {
                PrintDialog pDialog = new PrintDialog();
                var printerServer = printer.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).Count() > 1 ? string.Format("\\\\{0}", printer.Split(new[] { @"\" }, StringSplitOptions.RemoveEmptyEntries).First()) : string.Empty;
                var ps = string.IsNullOrEmpty(printerServer) ? new PrintServer() : new PrintServer(printerServer);
                pDialog.PrintQueue = ps.GetPrintQueue(System.IO.Path.GetFileName(printer));

                var printableArea = new Size(pDialog.PrintableAreaWidth, pDialog.PrintableAreaHeight);
                ctrl.Arrange(new Rect(0, 0, printableArea.Width, printableArea.Height));
                ctrl.UpdateLayout();


                PrintCapabilities capabilities = pDialog.PrintQueue.GetPrintCapabilities(pDialog.PrintTicket);
                if (capabilities.PageImageableArea == null) return;
                //get scale of the print wrt to screen of WPF visual
                double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / ctrl.ActualWidth, capabilities.PageImageableArea.ExtentHeight / ctrl.ActualHeight);

                //Transform the Visual to scale
                ctrl.LayoutTransform = new ScaleTransform(scale, scale);

                //get the size of the printer page
                Size sz = new Size(ctrl.ActualWidth * scale, ctrl.ActualHeight * scale); //(8.5 * 96.0, 11.0 * 96.0);

                //update the layout of the visual to the printer page size.
                ctrl.Measure(sz);
                ctrl.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                ctrl.UpdateLayout();
                pDialog.PrintVisual(ctrl, "Print");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
