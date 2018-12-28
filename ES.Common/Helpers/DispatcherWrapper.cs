using System;
using System.Windows;
using System.Windows.Threading;
using ES.Common.Interfaces;
using ES.Common.Managers;

namespace ES.Common.Helpers
{
    public class DispatcherWrapper : IDispatcher
    {
        static object _syncLock = new object();
        static DispatcherWrapper _instance;
        Dispatcher _dispatcher;
        private bool _isUnitTesting;
        public static IDispatcher Instance
        {
            get
            {
                lock (_syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new DispatcherWrapper();
                    }
                }
                return _instance;
            }
        }
        public bool IsValid { get { return _dispatcher != null; } }

        public DispatcherWrapper()
        {
            if (Application.Current != null)
            {
                _dispatcher = Application.Current.Dispatcher;
            }
            else
            {
                //this is useful for unit tests where there is no application running 
                _dispatcher = Dispatcher.CurrentDispatcher;
                _isUnitTesting = true;
            }
        }
        public void Invoke(DispatcherPriority priority, Action callback)
        {
            if (!CheckAccess())
            {
                _dispatcher.Invoke(priority, callback);
            }
            else
            {
                callback.Invoke();
            }
        }
        public void BeginInvoke(DispatcherPriority priority, Action callback)
        {
            if (!_isUnitTesting)
                _dispatcher.BeginInvoke(priority, callback);
            else
                _dispatcher.Invoke(priority, callback);
        }

        public void BeginInvoke<T>(DispatcherPriority priority, Action<T> callback, object arg = null)
        {
            if (!_isUnitTesting)
                _dispatcher.BeginInvoke(priority, callback, arg);
            else
                _dispatcher.Invoke(priority, callback, arg);
        }
        public void VerifyAndBeginInvoke(DispatcherPriority priority, Action callback)
        {
            var stackTrace = Environment.StackTrace;
            if (!_isUnitTesting)
            {
                _dispatcher.BeginInvoke(priority, new Action(() =>
                {
                    try
                    {
                        callback.DynamicInvoke();
                    }
                    catch (Exception ex)
                    {
                        MessageManager.ShowMessage((ex.InnerException != null ? ex.InnerException.Message : string.Empty) + "Outer stack: " + stackTrace, "Invoke error", MessageBoxImage.Error);
                    }
                }));
            }
            else
            {
                _dispatcher.Invoke(priority, callback);
            }
        }

        public void BeginInvokeWhileNeeded(Action callback)
        {
            if (!CheckAccess())
            {
                if (!_isUnitTesting)
                    _dispatcher.BeginInvoke(DispatcherPriority.Send, callback);
                else
                    _dispatcher.Invoke(DispatcherPriority.Send, callback);
            }
            else
            {
                callback.Invoke();
            }
        }
        public bool CheckAccess()
        {
            return IsValid && _dispatcher.CheckAccess();
        }
    }
}
