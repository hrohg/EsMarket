using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CustomControls
{
    [TemplatePart(Name = "PART_PolarPanel", Type = typeof(PolarPanel))]
    public class ProcessingContentControl : ContentControl
    {
        private PolarPanel _panel;
        private static Dictionary<PolarPanel, int> currentAnimatingCircles = new Dictionary<PolarPanel, int>();
        private static HashSet<PolarPanel> panels = new HashSet<PolarPanel>();

        private static ColorAnimation _animation = new ColorAnimation(Color.FromArgb(255, 31, 59, 83),
                                                                      Colors.Transparent,
                                                                      new Duration(new TimeSpan(0, 0, 0, 1)));
        private static Brush _brush = new SolidColorBrush();

        const int POLARPANEL_DEFAULT_WIDTH = 22;
        const int POLARPANEL_DEFAULT_RADIUS = 44;

        private static DispatcherTimer _timer;

        private static DispatcherTimer Timer
        {
            get
            {
                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromMilliseconds(80);
                    _timer.Tick += Timer_Tick;
                }

                return _timer;
            }
        }

        /// <summary>
        /// Overide to get the visuals from the control template
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _panel = (PolarPanel)GetTemplateChild("PART_PolarPanel");

            _panel.Loaded -= PolarPanel_Loaded;
            _panel.IsVisibleChanged -= PolarPanel_IsVisibleChanged;

            _panel.Loaded += PolarPanel_Loaded;
            _panel.IsVisibleChanged += PolarPanel_IsVisibleChanged;
        }

        public static readonly DependencyProperty IsContentProcessingProperty =
            DependencyProperty.Register("IsContentProcessing", typeof(bool), typeof(ProcessingContentControl));

        public bool IsContentProcessing
        {
            get { return (bool)GetValue(IsContentProcessingProperty); }
            set { SetValue(IsContentProcessingProperty, value); }
        }

        public static readonly DependencyProperty PolarPanelWidthProperty = 
            DependencyProperty.Register("PolarPanelWidth", typeof(int), typeof(ProcessingContentControl), new PropertyMetadata(POLARPANEL_DEFAULT_WIDTH));

        public int PolarPanelWidth
        {
            get { return (int)GetValue(PolarPanelWidthProperty); }
            set { SetValue(PolarPanelWidthProperty, value); }
        }

        public static readonly DependencyProperty PolarPanelRadiusProperty = 
            DependencyProperty.Register("PolarPanelRadius", typeof(int), typeof(ProcessingContentControl), new PropertyMetadata(POLARPANEL_DEFAULT_RADIUS));

        public int PolarPanelRadius
        {
            get { return (int)GetValue(PolarPanelRadiusProperty); }
            set { SetValue(PolarPanelRadiusProperty, value); }
        }

        private void PolarPanel_Loaded(object sender, RoutedEventArgs e)
        {
            PolarPanel panel = sender as PolarPanel;

            if (panel != null)
            {
                if (panel.Dispatcher.CheckAccess())
                {
                    foreach (Shape shape in panel.Children)
                    {
                        if (shape.Fill != null)
                            break;

                        if (_brush.CheckAccess())
                            shape.Fill = _brush.Clone();
                    }

                    OnPanelVisibilityChanged(panel, panel.IsVisible);
                }
            }
        }

        private void PolarPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PolarPanel panel = sender as PolarPanel;

            if (panel != null && panel.Dispatcher.CheckAccess())
            {
                bool visible = (bool) e.NewValue;

                OnPanelVisibilityChanged(panel, visible);
            }
        }

        private void OnPanelVisibilityChanged(PolarPanel panel, bool visible)
        {
            if (visible)
                Animate(panel);
            else
                StopAnimating(panel);
        }

        private void Animate(PolarPanel panel)
        {
            if (currentAnimatingCircles.ContainsKey(panel))
                return;

            panels.Add(panel);
            currentAnimatingCircles[panel] = -1;
            if (!Timer.IsEnabled)
                _timer.Start();
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            foreach (PolarPanel panel in panels)
            {
                if (panel.CheckAccess())
                {
                    int circleIndex = currentAnimatingCircles[panel] + 1;
                    if (circleIndex >= panel.Children.Count)
                        circleIndex = 0;

                    currentAnimatingCircles[panel] = circleIndex;
                    Shape element = panel.Children[circleIndex] as Shape;

                    if (element != null)
                    {
                        SolidColorBrush brush = element.Fill as SolidColorBrush;
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, _animation);
                    }
                }
            }
        }

        private void StopAnimating(PolarPanel panel)
        {
            currentAnimatingCircles.Remove(panel);
            panels.Remove(panel);

            foreach (Shape shape in panel.Children)
            {
                if (shape.Fill != null)
                    shape.Fill.BeginAnimation(SolidColorBrush.ColorProperty, null);
            }

            if (currentAnimatingCircles.Count == 0)
                Timer.Stop();
        }
    }

    public class PolarPanel : Panel
    {
        [AttachedPropertyBrowsableForChildren]
        [Category("Layout")]
        public static double GetAngle(DependencyObject obj)
        {
            return (double)obj.GetValue(AngleProperty);
        }

        public static void SetAngle(DependencyObject obj, double value)
        {
            obj.SetValue(AngleProperty, value);
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.RegisterAttached("Angle", typeof(double), typeof(PolarPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), ValidateAngle);

        [AttachedPropertyBrowsableForChildren]
        public static double GetRadius(DependencyObject obj)
        {
            return (double)obj.GetValue(RadiusProperty);
        }

        public static void SetRadius(DependencyObject obj, double value)
        {
            obj.SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.RegisterAttached("Radius", typeof(double), typeof(PolarPanel), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure), ValidateRadius);

        protected override Size MeasureOverride(Size availableSize)
        {
            Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            Rect bounds = new Rect();

            foreach (UIElement element in InternalChildren)
            {
                Point location = PolarToCartesian(GetAngle(element), GetRadius(element));

                element.Measure(infiniteSize);
                Rect elementBounds = CreateCenteralizeRect(location, element.DesiredSize);
                bounds.Union(elementBounds);
            }

            double width = Math.Max(Math.Abs(bounds.Left), Math.Abs(bounds.Right)),
                   height = Math.Max(Math.Abs(bounds.Top), Math.Abs(bounds.Bottom));

            return new Size(width * 2, height * 2);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Point panelCenter = new Point(finalSize.Width / 2, finalSize.Height / 2);

            foreach (UIElement element in InternalChildren)
            {
                Point location = PolarToCartesian(GetAngle(element), GetRadius(element));
                location += (Vector)panelCenter;

                Rect elementBounds = CreateCenteralizeRect(location, element.DesiredSize);

                element.Arrange(elementBounds);
            }

            return finalSize;
        }

        private static Rect CreateCenteralizeRect(Point point, Size size)
        {
            return new Rect(
                point.X - size.Width / 2,
                point.Y - size.Height / 2,
                size.Width,
                size.Height);
        }

        private static Point PolarToCartesian(double angle, double radius)
        {
            angle *= (-Math.PI / 180);
            return new Point(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle));
        }

        private static bool ValidateAngle(object value)
        {
            return !double.IsNaN((double)value) && !double.IsInfinity((double)value);
        }

        private static bool ValidateRadius(object value)
        {
            return !double.IsNaN((double)value) && !double.IsInfinity((double)value);
        }
    }
}