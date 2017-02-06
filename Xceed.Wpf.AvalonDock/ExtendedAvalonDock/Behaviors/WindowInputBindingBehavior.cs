using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Xceed.Wpf.AvalonDock.Commands;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace Xceed.Wpf.AvalonDock.ExtendedAvalonDock.Behaviors
{
    public static class WindowInputBindingsBehavior
    {
        public static readonly DependencyProperty WindowCloseCommandProperty = DependencyProperty.RegisterAttached(
            "WindowCloseCommand", typeof(ICommand), typeof(WindowInputBindingsBehavior), new UIPropertyMetadata(OnCloseCommandChanged));
        public static void SetWindowCloseCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(WindowCloseCommandProperty, value);
        }
        public static ICommand GetWindowCloseCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(WindowCloseCommandProperty);
        }
        public static readonly DependencyProperty WindowCloseKeyGestureProperty = DependencyProperty.RegisterAttached(
            "WindowCloseKeyGesture", typeof(KeyGesture), typeof(WindowInputBindingsBehavior), new UIPropertyMetadata(OnCloseCommandChanged));
        public static void SetWindowCloseKeyGesture(DependencyObject obj, KeyGesture value)
        {
            obj.SetValue(WindowCloseCommandProperty, value);
        }
        public static KeyGesture GetWindowCloseKeyGesture(DependencyObject obj)
        {
            return (KeyGesture)obj.GetValue(WindowCloseCommandProperty);
        }
        public static readonly DependencyProperty IsWindowHasOwnerProperty = DependencyProperty.Register(
            "IsWindowHasOwner", typeof(bool), typeof(WindowInputBindingsBehavior));
        public static void SetIsWindowHasOwner(DependencyObject obj, bool value)
        {
            obj.SetValue(IsWindowHasOwnerProperty, value);
        }
        public static bool GetIsWindowHasOwner(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsWindowHasOwnerProperty);
        }
        private static void OnCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window == null) return;
            window.Loaded -= WindowOnLoaded;
            window.Loaded += WindowOnLoaded;
            EmptyCommand = new RelayCommand((p) => { }, (p) => false);
        }
        private static void WindowOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var window = sender as Window;
            if (window == null) return;
            if ((bool)window.GetValue(IsWindowHasOwnerProperty) == false)
                ClearWindowOwner(window);

            SetWindowCloseInputBinding(window);
        }
        private static void ClearWindowOwner(Window window)
        {
            window.Owner = null;
            window.WindowStyle = WindowStyle.None;
            window.ShowInTaskbar = true;

            //var wndHelper = new WindowInteropHelper(window);
            //var exStyle = (int)ExtendedWindowStylesBehavior.GetWindowLong(wndHelper.Handle, ExtendedWindowStylesBehavior.GWL_EXSTYLE);
            //exStyle |= (int)ExtendedWindowStylesBehavior.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            //ExtendedWindowStylesBehavior.SetWindowLong(wndHelper.Handle, ExtendedWindowStylesBehavior.GWL_EXSTYLE, (IntPtr)exStyle);

            var command = (ICommand)window.GetValue(WindowCloseCommandProperty);
            var hwndSource = PresentationSource.FromVisual(window) as HwndSource;
            if (hwndSource != null)
            {
                if (command == null)
                {
                    hwndSource.RemoveHook(HwndSourceHookNoClose);
                    hwndSource.AddHook(HwndSourceHookNoClose);
                }
                else
                {
                    hwndSource.RemoveHook(HwndSourceHookClose);
                    hwndSource.AddHook(HwndSourceHookClose);
                }
            }
        }
        private static IntPtr HwndSourceHookClose(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == ExtendedWindowStylesBehavior.WM_CLOSE)
            {
                if (Application.Current.MainWindow == null) return IntPtr.Zero;
                var hwndSource = HwndSource.FromHwnd(hwnd);
                if (hwndSource != null)
                {
                    var window = hwndSource.RootVisual as LayoutFloatingWindowControl;
                    if (window != null && !window.InternalCloseFlag)
                    {
                        var command = window.GetValue(WindowCloseCommandProperty) as ICommand;
                        if (command != null)
                        {
                            hwndSource.RemoveHook(HwndSourceHookClose);
                            command.Execute(null);
                            if (!hwndSource.IsDisposed)
                                hwndSource.AddHook(HwndSourceHookClose);
                        }
                    }
                }
            }
            return IntPtr.Zero;
        }
        private static IntPtr HwndSourceHookNoClose(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == ExtendedWindowStylesBehavior.WM_CLOSE)
            {
                if (Application.Current.MainWindow == null) return IntPtr.Zero;
                var hwndSource = HwndSource.FromHwnd(hwnd);
                if (hwndSource != null)
                {
                    var window = hwndSource.RootVisual as LayoutAnchorableFloatingWindowControl;
                    if (window != null)
                    {
                        var model = window.Model as LayoutAnchorableFloatingWindow;
                        if (model != null && model.RootPanel != null)
                        {
                            var anchorableItem = window.SingleContentLayoutItem as LayoutAnchorableItem;
                            if (anchorableItem != null)
                            {
                                if (anchorableItem.LayoutElement != null && anchorableItem.LayoutElement.Parent != null)
                                    DockAllChildrens(anchorableItem.LayoutElement.Parent);
                            }
                            else
                            {
                                DockAllChildrens(model);
                            }
                            handled = true;
                        }
                    }
                }
            }
            else if (msg == ExtendedWindowStylesBehavior.WM_NCRBUTTONUP)/*disable context menu*/
            {
                handled = true;
            }
            return IntPtr.Zero;
        }
        private static void DockAllChildrens(ILayoutContainer model)
        {
            var anchorableItems = new List<LayoutAnchorableItem>();
            ExtendedLayoutAnchorable.GetAllAnchorableItems(model, anchorableItems);
            ExtendedLayoutAnchorable.OnDockLayoutAnchorableItems(anchorableItems);
        }
        public static ICommand EmptyCommand { get; set; }
        private static void SetWindowCloseInputBinding(Window window)
        {
            var command = (ICommand)window.GetValue(WindowCloseCommandProperty);
            var keyGesture = (KeyGesture)window.GetValue(WindowCloseKeyGestureProperty) ??
                             new KeyGesture(Key.F4, ModifierKeys.Alt);
            var newBinding = new KeyBinding(command ?? EmptyCommand, keyGesture);
            window.InputBindings.Add(newBinding);
        }
    }
}
