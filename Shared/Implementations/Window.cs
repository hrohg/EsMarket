using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Shared.Implementations
{
    public class EsWindow : Window
    {
        #region Contructors

        public EsWindow()
        {
            Initialize();
            SourceInitialized += Window_SourceInitialized;
            Loaded += OnLoaded;
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
        }

        #endregion

        #region Internal methods

        private void Initialize()
        {

        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {

            if (ActualHeight > SystemParameters.MaximizedPrimaryScreenHeight)
            {
                SizeToContent = SizeToContent.Manual;
                WindowState = WindowState.Maximized;
            }
            //else
            //{
            //    WindowState = WindowState.Normal;
            //    SizeToContent = SizeToContent.Height;
            //}
        }
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            // First, get the window handle
            IntPtr hwnd = (new WindowInteropHelper(this)).Handle;

            // Obtain the screen the windows will be displayed on
            IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            // If we've got the monitor...
            if (hMonitor != IntPtr.Zero)
            {
                MONITORINFO mi = new MONITORINFO();
                mi.cbSize = Marshal.SizeOf(mi);

                // ...and the monitor information, then we set the size constraint
                if (GetMonitorInfo(hMonitor, ref mi))
                {
                    MaxHeight = mi.rcWork.Bottom - mi.rcWork.Top;
                }
            }
        }

        #endregion


        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor, rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private const int MONITOR_DEFAULTTONEAREST = 2;
    }
}
