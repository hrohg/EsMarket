using ES.Common.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Documents.Serialization;
using System.Windows.Media;
using System.Windows.Xps;

namespace Es.Market.Tools.Helpers
{
    public class PrintManager
    {
        public static void PrintEx(FrameworkElement objectToPrint)
        {
            if (objectToPrint == null) return;
            PrintDocumentImageableArea area = null;
            PageRangeSelection selection = PageRangeSelection.AllPages;
            PageRange range = new PageRange();

            try
            {
                XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(objectToPrint.Name, ref area, ref selection, ref range);
                if (area == null) return;
                Thickness margins = new Thickness(area.MediaSizeHeight > 200 ? 96 : 0);
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
            catch(Exception ex)
            {
                MessageManager.OnMessage(ex.Message);
            }
        }
        private void PrintCollection(List<Label> list)
        {
            FlowDocument fd = new FlowDocument();
            foreach (var item in list) //<- put your collection here
            {
                fd.Blocks.Add(new Paragraph(new Run(item.ToString())));
            }

            PrintDialog pd = new PrintDialog();
            if (pd.ShowDialog() != true) return;

            fd.PageHeight = pd.PrintableAreaHeight;
            fd.PageWidth = pd.PrintableAreaWidth;

            IDocumentPaginatorSource idocument = fd as IDocumentPaginatorSource;

            pd.PrintDocument(idocument.DocumentPaginator, "Printing Flow Document...");
        }

    }
}
