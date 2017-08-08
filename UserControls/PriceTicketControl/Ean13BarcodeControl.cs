using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using ES.Common.Helpers;
using Zen.Barcode;

namespace UserControls.PriceTicketControl
{
    public class Ean13BarcodeControl : FrameworkElement
    {
        private static readonly BarcodeDraw BarcodeDraw = BarcodeDrawFactory.CodeEan13WithChecksum;
        static Ean13BarcodeControl()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(Ean13BarcodeControl), new FrameworkPropertyMetadata(true));
        }

        public static readonly DependencyProperty BarMinWidthProperty = DependencyProperty.Register("BarMinWidth", typeof(double), typeof(Ean13BarcodeControl), new PropertyMetadata((double)1));
        public double BarMinWidth
        {
            get { return (double)GetValue(BarMinWidthProperty); }
            set { SetValue(BarMinWidthProperty, value); }
        }
        public static readonly DependencyProperty BarMaxWidthProperty = DependencyProperty.Register("BarMaxWidth", typeof(double), typeof(Ean13BarcodeControl), new PropertyMetadata((double)3));
        public double BarMaxWidth
        {
            get { return (double)GetValue(BarMaxWidthProperty); }
            set { SetValue(BarMaxWidthProperty, value); }
        }
        public static readonly DependencyProperty BarHeightProperty = DependencyProperty.Register("BarHeight", typeof(double), typeof(Ean13BarcodeControl), new PropertyMetadata((double)60));
        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }

        #region Barcode Image
        // Using a DependencyProperty as the backing store for Barcode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarcodeImageProperty = DependencyProperty.Register("BarcodeImage", typeof(PictureBox), typeof(Ean13BarcodeControl));
        public PictureBox BarcodeImage
        {
            get { return (PictureBox)GetValue(BarcodeImageProperty); }
            set { SetValue(BarcodeImageProperty, value); }
        }
        #endregion Barcode Image

        public string Barcode
        {
            get { return (string)GetValue(BarcodeProperty); }
            set { SetValue(BarcodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Barcode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BarcodeProperty = DependencyProperty.Register("Barcode", typeof(string), typeof(Ean13BarcodeControl), new PropertyMetadata(null, BarcodePropertyChangedCallback));

        private static void BarcodePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctl = sender as Ean13BarcodeControl;
            if (ctl == null) return;
            
            ctl.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            //var width = 40;// ActualWidth;
            //var height = (int) ActualHeight;
            //var size = new Rect(0, 0, width, ActualHeight);
            //drawingContext.DrawRectangle(Brushes.White, null, size);
            //if (!string.IsNullOrEmpty(Barcode))
            //{
            //    BarcodeDraw.Draw(drawingContext, Barcode, new BarcodeMetrics1d(1, 2, 40), size);
            //}
            var width = ActualWidth;
            var height = (int)ActualHeight;
            var size = new Rect(0, 0, width, height);
            drawingContext.DrawRectangle(null, null, size);
            if (MinHeight == 0) MinHeight = BarHeight;
            if (MaxHeight == 0) MaxHeight = BarHeight;
            if (!string.IsNullOrEmpty(Barcode))
            {
                BarcodeDraw.Draw(drawingContext, Barcode, new BarcodeMetrics1d(BarMinWidth, BarMaxWidth, MinHeight, MaxHeight), size);
            }
        }

        private void CreateBarcode(DrawingContext dc)
        {
            var ean13 = new Ean13(Barcode);

            Graphics g = BarcodeImage.CreateGraphics();
            
            g.FillRectangle(new SolidBrush(System.Drawing.SystemColors.Control), new Rectangle(0, 0, (int)Width, (int)BarHeight));
            
            ean13.Scale = 1; // scale;
            ean13.DrawEan13Barcode(g, new System.Drawing.Point(0, 0));
            g.Dispose();
        }
    }
}
