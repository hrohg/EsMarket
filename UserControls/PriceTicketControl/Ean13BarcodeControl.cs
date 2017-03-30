using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Zen.Barcode;

namespace UserControls.PriceTicketControl
{
    public class Ean13BarcodeControl: FrameworkElement
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
            var size = new Rect(0, 0, ActualWidth, ActualHeight);
            drawingContext.DrawRectangle(Brushes.White, null, size);
            if (!string.IsNullOrEmpty(Barcode))
            {
                BarcodeDraw.Draw(drawingContext, Barcode, new BarcodeMetrics1d(BarMinWidth, BarMaxWidth, BarHeight), size);
            }
        }
    }
}
