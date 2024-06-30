using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Shared.Implementations
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<System.Windows.Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as System.Windows.Window;
            if (window != null) action(window);
        }

        public static IntPtr GetWindowHandle(this System.Windows.Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
    [TemplatePart(Type = typeof(Image), Name = ImageName)]
    [TemplatePart(Type = typeof(Button), Name = MinButtonName)]
    [TemplatePart(Type = typeof(Button), Name = MaxButtonName)]
    [TemplatePart(Type = typeof(Button), Name = CloseButtonName)]
    public partial class Window : System.Windows.Window, IChangeWindowstate
    {
        private const string ImageName = "Icon";
        private const string MinButtonName = "MinButton";
        private const string MaxButtonName = "MaxButton";
        private const string CloseButtonName = "CloseButton";
        private WindowState _oldWindowState;
        public EventHandler<CancelEventArgs> CanCloseWindow;
        static Window()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(typeof(Window)));
        }

        public Window()
        {
            Loaded += WindowLoaded;
        }

        //protected override void OnLocationChanged(EventArgs e)
        //{
        //    var w = this;
        //    var handle = w.GetWindowHandle();
        //    var screen = System.Windows.Forms.Screen.FromHandle(handle);
        //    w.MaxHeight = (SystemParameters.PrimaryScreenHeight>screen.WorkingArea.Height) ? SystemParameters.PrimaryScreenHeight : double.PositiveInfinity;
        //    w.MaxWidth = (SystemParameters.PrimaryScreenWidth > screen.WorkingArea.Width) ? SystemParameters.PrimaryScreenWidth : double.PositiveInfinity;
        //}
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AddHandler(KeyDownEvent, (KeyEventHandler)OnHotKeysCommand, true);

            var icon = Template.FindName(ImageName, this) as Image;
            if (icon != null)
            {
                icon.MouseUp += IconMouseUp;
                icon.MouseLeftButtonDown += IconMouseLeftButtonDown;
            }

            var minButton = Template.FindName(MinButtonName, this) as Button;
            if (minButton != null) minButton.Click += MinButtonClick;

            var maxButton = Template.FindName(MaxButtonName, this) as Button;
            if (maxButton != null) maxButton.Click += MaxButtonClick;

            var closeButton = Template.FindName(CloseButtonName, this) as Button;
            if (closeButton != null) closeButton.Click += CloseButtonClick;
        }

        //public static bool IsWindowOnAnyScreen(Window wnd, Point wndPoint, double WindowSizeX, double WindowSizeY, bool AutoAdjustWindow)
        //{
        //    var Screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(wnd).Handle);

        //    bool LeftSideTest = false, TopSideTest = false, BottomSideTest = false, RightSideTest = false;

        //    if (wndPoint.X >= Screen.WorkingArea.Left)
        //        LeftSideTest = true;

        //    if (wndPoint.Y >= Screen.WorkingArea.Top)
        //        TopSideTest = true;

        //    if ((wndPoint.Y + WindowSizeY) <= Screen.WorkingArea.Bottom)
        //        BottomSideTest = true;

        //    if ((wndPoint.X + WindowSizeX) <= Screen.WorkingArea.Right)
        //        RightSideTest = true;

        //    if (LeftSideTest && TopSideTest && BottomSideTest && RightSideTest)
        //        return true;
        //    else
        //    {
        //        if (AutoAdjustWindow)
        //        {
        //            if (!LeftSideTest)
        //                wndPoint.X = wndPoint.X - (wndPoint.X - Screen.WorkingArea.Left);

        //            if (!TopSideTest)
        //                wndPoint.Y = wndPoint.Y - (wndPoint.Y - Screen.WorkingArea.Top);

        //            if (!BottomSideTest)
        //                wndPoint.Y = wndPoint.Y - ((wndPoint.Y + WindowSizeY) - Screen.WorkingArea.Bottom);

        //            if (!RightSideTest)
        //                wndPoint.X = wndPoint.X - ((wndPoint.X + WindowSizeX) - Screen.WorkingArea.Right);
        //        }
        //    }

        //    return false;
        //}
        //public static bool IsPointVisibleOnAScreen(Point p)
        //{
        //    foreach (Screen s in Screen.AllScreens)
        //    {
        //        if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
        //            return true;
        //    }
        //    return false;
        //}

        //public static System.Drawing.Rectangle PointScreenBounds(Point p)
        //{
        //    foreach (Screen s in Screen.AllScreens)
        //    {
        //        if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
        //            return s.Bounds;
        //    }
        //    return new System.Drawing.Rectangle(0, 0, 0, 0);
        //}

        //public static bool IsWindowFullyVisible(Window w)
        //{
        //    return IsPointVisibleOnAScreen(new Point(w.Left, w.Top)) && IsPointVisibleOnAScreen(new Point(w.Left + w.Width, w.Top)) && IsPointVisibleOnAScreen(new Point(w.Left, w.Top + w.Height)) && IsPointVisibleOnAScreen(new Point(w.Left + w.Width, w.Top + w.Height));
        //}
        private static void OnHotKeysCommand(object sender, KeyEventArgs keyEventArgs)
        {
            var control = sender as FrameworkElement;
            if (control == null) return;

            var command = (ICommand)control.GetValue(HotKeysCommandProperty);
            if (command == null) return;

            command.Execute(keyEventArgs);
        }

        private void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
                sender.ForWindowFromTemplate(SystemCommands.CloseWindow);
        }

        private void IconMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null) return;

            var point = element.PointToScreen(new Point(element.ActualWidth / 2, element.ActualHeight));
            sender.ForWindowFromTemplate(w => SystemCommands.ShowSystemMenu(w, point));
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ((System.Windows.Window)sender).SizeChanged += WindowSizeChanged;
            //WindowSizeChanged((System.Windows.Window)sender);
            HideMaximizeBox();
        }

        //private static void WindowSizeChanged(System.Windows.Window w)
        //{
        //    var handle = w.GetWindowHandle();
        //    var containerBorder = (Border)w.Template.FindName("PART_Container", w);
        //    var border = (Border)w.Template.FindName("PART_Border", w);
        //    var screen = System.Windows.Forms.Screen.FromHandle(handle);

        //    if (w.WindowState == WindowState.Maximized)
        //    {
        //        border.BorderThickness = new Thickness(0, 0, 0, 0);
        //        //var paddingLeft = 7 - containerBorder.BorderThickness.Left;
        //        //var paddingTop = 7 - containerBorder.BorderThickness.Top;
        //        //var paddingRight = 7 - containerBorder.BorderThickness.Right;
        //        //var paddingBottom = 5 - containerBorder.BorderThickness.Bottom;
        //        //containerBorder.Padding = new Thickness(paddingLeft, paddingTop, paddingRight, paddingBottom);
        //        //w.MaxHeight = screen.WorkingArea.Height + 12 + w.BorderThickness.Top + w.BorderThickness.Bottom;
        //        //containerBorder.Height = screen.WorkingArea.Height + 12 + w.BorderThickness.Top;
        //        //w.MaxWidth = screen.WorkingArea.Width + 14 + w.BorderThickness.Left + w.BorderThickness.Right;
        //        //containerBorder.Width = screen.WorkingArea.Width + 14;

        //        border.Margin = new Thickness(-containerBorder.BorderThickness.Left, -containerBorder.BorderThickness.Top, -containerBorder.BorderThickness.Right, -containerBorder.BorderThickness.Bottom);
        //        SetWindowPos(w.GetWindowHandle(), w.GetWindowHandle(), screen.WorkingArea.Left, screen.WorkingArea.Top, screen.WorkingArea.Width, screen.WorkingArea.Height, 0x0004);
        //        //w.Left = screen.WorkingArea.Left;
        //        //w.Top = screen.WorkingArea.Top;
        //        //if (screen.Primary)
        //        //{

        //        //w.Top = SystemParameters.WorkArea.Top;
        //        //w.MaxHeight = SystemParameters.WorkArea.Height + 12 + w.BorderThickness.Top + w.BorderThickness.Bottom;
        //        //containerBorder.Height = SystemParameters.WorkArea.Height + 12 + w.BorderThickness.Top + w.BorderThickness.Bottom;
        //        //w.Left = SystemParameters.WorkArea.Left;
        //        //w.Width = SystemParameters.FullPrimaryScreenWidth;
        //        //}
        //        //else
        //        //{
        //        //    w.MaxHeight = Double.PositiveInfinity;
        //        //    containerBorder.Height = double.NaN;
        //        //}

        //        //w.MaxWidth = screen.Primary ? containerBorder.Width = 1610: Double.PositiveInfinity;
        //        // Make sure window doesn't overlap with the taskbar.
        //    }
        //    else
        //    {
        //        //containerBorder.Height = double.NaN;
        //        //containerBorder.Width = double.NaN;
        //        //containerBorder.MaxWidth = SystemParameters.VirtualScreenWidth;
        //        containerBorder.Padding = new Thickness(0);
        //        if (w.Top == SystemParameters.WorkArea.Top && w.Height == SystemParameters.WorkArea.Height && screen.Primary)
        //        {
        //            //snapped in primary screen
        //            border.BorderThickness = new Thickness(0, 0, 1, 0);

        //        }
        //        else if (w.Top == SystemParameters.WorkArea.Top && w.Height == SystemParameters.PrimaryScreenHeight && !screen.Primary)
        //        {
        //            //snapped in secondary screen
        //            border.BorderThickness = new Thickness(1, 0, 0, 0);
        //        }
        //        else
        //        {
        //            //not snapped
        //            border.BorderThickness = new Thickness(1, 1, 1, 1);
        //        }
        //    }
        //}
        public void ChangeWindowState(WindowState state)
        {
            _oldWindowState = WindowState;
            WindowState = state;
        }
        public void RestoreWindowState()
        {
            WindowState = _oldWindowState;
        }
        private static void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var w = ((System.Windows.Window)sender);
            //WindowSizeChanged(w);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && CloseOnEscape) Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (CanCloseWindow != null)
            {
                CanCloseWindow.Invoke(this, e);
                if (e.Cancel) return;
            }

            if (PreventWindowClose)
                e.Cancel = true;
            else
                base.OnClosing(e);
        }
        private static void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.CloseWindow);
        }

        private static void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(SystemCommands.MinimizeWindow);
        }

        private static void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                if (w.WindowState == WindowState.Maximized)
                    SystemCommands.RestoreWindow(w);
                else
                    SystemCommands.MaximizeWindow(w);
            });
        }

        private void HideMaximizeBox()
        {
            if (!HideMaxButton) return;
            var hwnd = new WindowInteropHelper(this).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, value & ~WS_MAXIMIZEBOX);
        }

        public static bool RestoreWindowShell(System.Windows.Window window)
        {
            IntPtr hwnd = window.GetWindowHandle();
            return ShowWindow(hwnd, SW_SHOW);
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);        
    }

    public interface IChangeWindowstate
    {
        void ChangeWindowState(WindowState state);
        void RestoreWindowState();
    }
}
