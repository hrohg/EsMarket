using System.Globalization;
using System.Windows.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace RealEstateApp.CustomControls
{
    /// <summary>
    /// A draggable, dockable, expandable panel class.
    /// </summary>
    public class DragDockPanel : AnimatedHeaderedContentControl
    {
        #region Private members
        /// <summary>
        /// Stores the current z-index
        /// </summary>
        private static int currentZIndex = 1; 

        /// <summary>
        /// Is dragging flag.
        /// </summary>
        private bool dragging;

        /// <summary>
        /// Stores the last drag position.
        /// </summary>
        private Point downPosition;

        /// <summary>
        /// Ignore the last uncheck event flag.
        /// </summary>
        private bool ignoreUnCheckedEvent;

        private bool ignoreHOevent;

        private bool ignoreCollapse;

        /// <summary>
        /// Dragging enabled flag.
        /// </summary>
        private bool draggingEnabled = true;

        private string fullHeader;
        private TextBlock header;

        #endregion
        
        /// <summary>
        /// Drag dock panel constructor.
        /// </summary>
        public DragDockPanel()
        {
            this.DefaultStyleKey = typeof(DragDockPanel);
        }

        #region Public events
        /// <summary>
        /// The drag started event.
        /// </summary>
        public event DragEventHander DragStarted;

        /// <summary>
        /// The drag moved event.
        /// </summary>
        public event DragEventHander DragMoved;

        /// <summary>
        /// The drag finished event.
        /// </summary>
        public event DragEventHander DragFinished;

        /// <summary>
        /// The headeronly event.
        /// </summary>
        public event EventHandler HeaderOnly;

        /// <summary>
        /// The maxmised event.
        /// </summary>
        public event EventHandler Maximized;

        /// <summary>
        /// The minimised event.
        /// </summary>
        public event EventHandler Minimized;

        /// <summary>
        /// The normalized event.
        /// </summary>
        public event EventHandler Normalized;
        #endregion

        #region Public members
        /// <summary>
        /// Gets or sets a value indicating whether the dragging is enabled.
        /// </summary>
        public bool DraggingEnabled
        {
            get { return this.draggingEnabled; }
            set { this.draggingEnabled = value; }
        }

        #region PanelHeaderOnly
        public bool PanelHeaderOnly
        {
            get { return (bool)GetValue(PanelHeaderOnlyProperty); }
            set { SetValue(PanelHeaderOnlyProperty, value); }
        }

        public static readonly DependencyProperty PanelHeaderOnlyProperty =
            DependencyProperty.Register("PanelHeaderOnly", typeof(bool),
            typeof(DragDockPanel), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHeaderOnly));

        private static void OnHeaderOnly(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragDockPanel panel = (DragDockPanel)d;
            ToggleButton ho = panel.HeaderOnlyToggle;

            if (ho != null)
            {
                ho.IsChecked = (bool)e.NewValue;
            }
        }
        #endregion

        #region PanelFullSize
        public bool PanelFullSize
        {
            get { return (bool)GetValue(PanelFullSizeProperty); }
            set { SetValue(PanelFullSizeProperty, value); }
        }

        public static readonly DependencyProperty PanelFullSizeProperty =
            DependencyProperty.Register("PanelFullSize", typeof(bool),
            typeof(DragDockPanel), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFullSize));

        private static void OnFullSize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragDockPanel panel = (DragDockPanel)d;
            ToggleButton fullSizeToggle = panel.FullSizeToggle;

            if (fullSizeToggle != null)
            {
                fullSizeToggle.IsChecked = (bool)e.NewValue;
            }
        }
        #endregion

        #region PanelMaximized
        public bool PanelMaximized
        {
            get { return (bool)GetValue(PanelMaximizedProperty); }
            set { SetValue(PanelMaximizedProperty, value); }
        }

        public static readonly DependencyProperty PanelMaximizedProperty =
            DependencyProperty.Register("PanelMaximized", typeof(bool),
            typeof(DragDockPanel), new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,OnMaximize));

        private static void OnMaximize(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragDockPanel panel = (DragDockPanel)d;
            ToggleButton maximizeToggle = panel.GetTemplateChild("PART_MaximizeToggle") as ToggleButton;
            {
                if (maximizeToggle != null)
                {
                    panel.ignoreUnCheckedEvent = true;
                    panel.ignoreCollapse = true;
                    maximizeToggle.IsChecked = (bool)e.NewValue;
                }
            }
        } 
        #endregion

        #region PanelMinimized
        public bool PanelMinimized
        {
            get { return (bool)GetValue(PanelMinimizedProperty); }
            set { SetValue(PanelMinimizedProperty, value); }
        }

        public static readonly DependencyProperty PanelMinimizedProperty =   
        DependencyProperty.Register("PanelMinimized", 
                                     typeof(bool), 
                                     typeof(DragDockPanel), 
                                     new FrameworkPropertyMetadata(false,   
                                                           FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        #region PanelNormal
        public bool PanelNormal
        {
            get { return (bool)GetValue(PanelNormalProperty); }
            set { SetValue(PanelNormalProperty, value); }
        }

        public static readonly DependencyProperty PanelNormalProperty =   
        DependencyProperty.Register("PanelNormal", 
                                     typeof(bool), 
                                     typeof(DragDockPanel),
                                     new FrameworkPropertyMetadata(false,   
                                                           FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PanelNormalChanged));

        private static void PanelNormalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragDockPanel panel = (DragDockPanel) d;

            if ((bool)e.NewValue && panel.Normalized != null)
            {
                panel.PanelFullSize = false;
                panel.Normalized(panel, new EventArgs());
            }
        }

        #endregion

        #region AllowResize
        /// <summary>
        /// Показывать ли кнопки сворачивания\разворачивания панели.
        /// </summary>
        public bool AllowResize
        {
            get { return (bool)GetValue(AllowResizeProperty); }
            set { SetValue(AllowResizeProperty, value); }
        }

        public static readonly DependencyProperty AllowResizeProperty =
            DependencyProperty.Register("AllowResize", typeof(bool), typeof(DragDockPanel), new FrameworkPropertyMetadata(true));

        #endregion

        #region AllowHeaderOnly
        public bool AllowHeaderOnly
        {
            get { return (bool)GetValue(AllowHeaderOnlyProperty); }
            set { SetValue(AllowHeaderOnlyProperty, value); }
        }

        public static readonly DependencyProperty AllowHeaderOnlyProperty =
            DependencyProperty.Register("AllowHeaderOnly", typeof(bool),
            typeof(DragDockPanel), new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion
        #endregion

        public ToggleButton MaximizeToggle;
        public ToggleButton FullSizeToggle;
        public ToggleButton HeaderOnlyToggle;

        /// <summary>
        /// Gets called once the template is applied so we can fish out the bits
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElement gripBar =
                this.GetTemplateChild("PART_GripBar") as FrameworkElement;

            if (gripBar != null)
            {
                gripBar.MouseLeftButtonDown += GripBar_MouseLeftButtonDown;
                gripBar.MouseMove += GripBar_MouseMove;
                gripBar.MouseLeftButtonUp += GripBar_MouseLeftButtonUp;
                gripBar.MouseDown += GripBar_MouseDown;
            }

            MaximizeToggle = this.GetTemplateChild("PART_MaximizeToggle") as ToggleButton;

            if (MaximizeToggle != null)
            {
                MaximizeToggle.Checked += MaximizeToggle_Checked;
                MaximizeToggle.Unchecked += MaximizeToggle_Unchecked;
            }

            FullSizeToggle = this.GetTemplateChild("PART_FullSizeToggle") as ToggleButton;

            if (FullSizeToggle != null)
            {
                FullSizeToggle.Checked += FullSize_Checked;
                FullSizeToggle.Unchecked += FullSize_Unchecked;
            }

            HeaderOnlyToggle = this.GetTemplateChild("PART_HeaderOnlyToggle") as ToggleButton;

            if (HeaderOnlyToggle != null)
            {
                HeaderOnlyToggle.Checked += HeaderOnly_Checked;
                HeaderOnlyToggle.Unchecked += HeaderOnly_Unchecked;
            }

            ContentPresenter presenter = this.GetTemplateChild("PART_Header") as ContentPresenter;
            if (presenter != null && presenter.Content is TextBlock)
            {
                header = presenter.Content as TextBlock;
                var dp = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
                dp.AddValueChanged(header, HeaderChanged);
                if (string.IsNullOrEmpty(fullHeader)) fullHeader = header.Text;
            }
        }

        private void HeaderChanged(object sender, EventArgs e)
        {
            CalculateHeaderWidth();
        }

        public void CalculateHeaderWidth()
        {
            if (fullHeader == null) return;

            if (!header.Text.Contains("...") && fullHeader != header.Text)
                fullHeader = header.Text;

            double margin = AllowHeaderOnly && PanelMinimized ? 75 : 55;
            double actualWidth = (Width - margin) > 15 ? Width - margin : 15;
            double measured = MeasureWidth(fullHeader);
            if (measured > actualWidth)
            {
                string text = "";
                int charIndex = 0;
                foreach(char c in fullHeader)
                {
                    text += c;
                    charIndex++;
                    if (charIndex < fullHeader.Length && MeasureWidth(text) > actualWidth)
                    {
                        header.Text = text + "...";
                        break;
                    }
                }
            }
            else
            {
                header.Text = fullHeader;
            }
        }

        public void SetSize(double left, double top, double width, double height)
        {
            if (UseFloatSize)
            {
                Canvas.SetLeft(this, left);
                Canvas.SetTop(this, top);

                Width = width > 0 ? width : 0;
                Height = height > 0 ? height : 0;
            }
            else
            {
                Canvas.SetLeft(this, System.Math.Floor(left));
                Canvas.SetTop(this, System.Math.Floor(top));

                Width = width > 0 ? System.Math.Floor(width) : 0;
                Height = height > 0 ? System.Math.Floor(height) : 0;
            }
        }

        private double MeasureWidth(string text)
        {
            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                 new Typeface(header.FontFamily, header.FontStyle, header.FontWeight, header.FontStretch),
                 header.FontSize, Brushes.Black);

            return ft.Width;
        }

        #region Maximize events
        /// <summary>
        /// Fires the minimised event.
        /// </summary>
        /// <param name="sender">The maximised toggle.</param>
        /// <param name="e">Routed event args.</param>
        public void MaximizeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!ignoreCollapse) ignoreUnCheckedEvent = false;

            if (!ignoreUnCheckedEvent)
            {
                PanelMaximized = false;
                PanelFullSize = false;

                if (Minimized != null)
                {
                    Minimized(this, e);
                }
            }
            else
            {
                ignoreUnCheckedEvent = false;
            }
        }

        /// <summary>
        /// Fires the maximised event.
        /// </summary>
        /// <param name="sender">The maximised toggle.</param>
        /// <param name="e">Routed event args.</param>
        public void MaximizeToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Bring the panel to the front
            Panel.SetZIndex(this, currentZIndex++);

            this.ignoreUnCheckedEvent = false;

            PanelMaximized = true;
            
            if (this.Maximized != null)
            {
                this.Maximized(this, e);
                ignoreCollapse = false;
            }
        }
        #endregion

        #region FullSize events
        public void FullSize_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!ignoreCollapse) ignoreUnCheckedEvent = false;

            if (!this.ignoreUnCheckedEvent)
            {
                PanelFullSize = false;                

                if (this.Maximized != null)
                {
                    this.Maximized(this, e);
                }
            }
            else
            {
                this.ignoreUnCheckedEvent = false;
            }
        }

        public void FullSize_Checked(object sender, RoutedEventArgs e)
        {
            // Bring the panel to the front
            Panel.SetZIndex(this, currentZIndex++);

            this.ignoreUnCheckedEvent = false;
                        
            PanelFullSize = true;

            if (Maximized != null)
            {
                Maximized(this, e);
                ignoreCollapse = false;
            }
        }
        #endregion

        #region Dragging events
        /// <summary>
        /// Starts the dragging.
        /// </summary>
        /// <param name="sender">The grip bar.</param>
        /// <param name="e">Mouse button event args.</param>
        private void GripBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.draggingEnabled)
            {
                // Bring the panel to the front
                Canvas.SetZIndex(this, currentZIndex++);

                // Capture the mouse
                ((FrameworkElement)sender).CaptureMouse();

                // Store the start position
                this.downPosition = e.GetPosition(sender as UIElement);

                // Set dragging to true
                this.dragging = true;

                // Fire the drag started event
                if (this.DragStarted != null)
                {
                    this.DragStarted(this, new DragEventArgs(0, 0, e));
                }
            }
        }

        /// <summary>
        /// Ends the dragging.
        /// </summary>
        /// <param name="sender">The grip bar.</param>
        /// <param name="e">Mouse button event args.</param>
        private void GripBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.draggingEnabled)
            {
                // Capture the mouse
                ((FrameworkElement)sender).ReleaseMouseCapture();

                // Set dragging to true
                this.dragging = false;

                Point position = e.GetPosition(sender as UIElement);

                // Fire the drag finished event
                if (this.DragFinished != null)
                {
                    this.DragFinished(this, new DragEventArgs(position.X - this.downPosition.X, position.Y - this.downPosition.Y, e));
                }
            }
        }

        /// <summary>
        /// Drags the panel (if the panel is being dragged).
        /// </summary>
        /// <param name="sender">The grip bar.</param>
        /// <param name="e">Mouse event args.</param>
        private void GripBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.dragging)
            {
                Point position = e.GetPosition(sender as UIElement);

                // Move the panel
                Canvas.SetLeft(
                    this,
                    Canvas.GetLeft(this) + position.X - this.downPosition.X);

                Canvas.SetTop(
                    this,
                    Canvas.GetTop(this) + position.Y - this.downPosition.Y);

                // Fire the drag moved event
                if (this.DragMoved != null)
                {
                    this.DragMoved(this, new DragEventArgs(position.X - this.downPosition.X, position.Y - this.downPosition.Y, e));
                }
            }
        }

        private void GripBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && AllowResize)
            {
                dragging = false;
                GripBar_MouseLeftButtonUp(sender, e);

                if (!PanelMaximized)
                {
                    PanelMaximized = true;
                    this.Maximized(this, e);
                }
                else
                {
                    PanelMaximized = false;
                    PanelFullSize = false;
                    this.Minimized(this, e); 
                }
            }
        }
        #endregion

        #region HeaderOnly events
        public void HeaderOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            PanelHeaderOnly = false;

            if (this.HeaderOnly != null)
            {
                this.HeaderOnly(this, e);
            }
        }

        public void HeaderOnly_Checked(object sender, RoutedEventArgs e)
        {
            // Bring the panel to the front
            Panel.SetZIndex(this, currentZIndex++);
            
            PanelHeaderOnly = true;

            if (HeaderOnly != null)
            {
                HeaderOnly(this, e);                
            }
        }
        #endregion
    }
}