using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CustomControls
{
    [TemplatePart(Name = "PART_PolarPanel", Type = typeof(PolarPanel))]
    public class MainWindowProcessControl : ContentControl
    {
        private PolarPanel _panel;
        private TextBlock _message;
        private TextBlock _count;
        private static Dictionary<PolarPanel, int> currentAnimatingCircles = new Dictionary<PolarPanel, int>();
        private static HashSet<PolarPanel> panels = new HashSet<PolarPanel>();

        private static ColorAnimation _animation = new ColorAnimation(Color.FromArgb(255, 31, 59, 83),
                                                                      Colors.Transparent,
                                                                      new Duration(new TimeSpan(0, 0, 0, 1)));
        private static Brush _brush = new SolidColorBrush();

        const int POLARPANEL_DEFAULT_WIDTH = 21;
        const int POLARPANEL_DEFAULT_RADIUS = 31;

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

            _panel = (PolarPanel) GetTemplateChild("PART_PolarPanel");
            _message = (TextBlock) GetTemplateChild("PART_Message");
            _count = (TextBlock)GetTemplateChild("PART_Count"); 

            _panel.Loaded -= PolarPanel_Loaded;
            _panel.IsVisibleChanged -= PolarPanel_IsVisibleChanged;

            _panel.Loaded += PolarPanel_Loaded;
            _panel.IsVisibleChanged += PolarPanel_IsVisibleChanged;
        }

        public static readonly DependencyProperty IsContentProcessingProperty =
            DependencyProperty.Register("IsContentProcessing", typeof(bool), typeof(MainWindowProcessControl));

        public bool IsContentProcessing
        {
            get { return (bool)GetValue(IsContentProcessingProperty); }
            set { SetValue(IsContentProcessingProperty, value); }
        }

        public static readonly DependencyProperty PolarPanelWidthProperty =
            DependencyProperty.Register("PolarPanelWidth", typeof(int), typeof(MainWindowProcessControl), new PropertyMetadata(POLARPANEL_DEFAULT_WIDTH));

        public int PolarPanelWidth
        {
            get { return (int)GetValue(PolarPanelWidthProperty); }
            set { SetValue(PolarPanelWidthProperty, value); }
        }

        public static readonly DependencyProperty PolarPanelRadiusProperty =
            DependencyProperty.Register("PolarPanelRadius", typeof(int), typeof(MainWindowProcessControl), new PropertyMetadata(POLARPANEL_DEFAULT_RADIUS));

        public int PolarPanelRadius
        {
            get { return (int)GetValue(PolarPanelRadiusProperty); }
            set { SetValue(PolarPanelRadiusProperty, value); }
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(string), typeof(MainWindowProcessControl), new PropertyMetadata(OnCountChanged));

        public string Count
        {
            get { return (string)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MainWindowProcessControl), new PropertyMetadata(""));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        private static void OnCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindowProcessControl windowProcessControl = (MainWindowProcessControl)o;
            DoubleAnimation fade = new DoubleAnimation
            {
                From = 9,
                To = 12,
                Duration = TimeSpan.FromMilliseconds(30),
            };

            Storyboard.SetTarget(fade, windowProcessControl._count);
            Storyboard.SetTargetProperty(fade, new PropertyPath(FontSizeProperty));

            var sb = new Storyboard();
            sb.Children.Add(fade);
            sb.Begin();
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
}