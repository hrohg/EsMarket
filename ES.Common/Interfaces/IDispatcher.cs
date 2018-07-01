using System;
using System.Windows.Threading;

namespace ES.Common.Interfaces
{
    public interface IDispatcher
    {
        void Invoke(DispatcherPriority priority, Action callback);
        void BeginInvoke(DispatcherPriority priority, Action callback);
        void BeginInvoke<T>(DispatcherPriority priority, Action<T> callback, object arg);
        void VerifyAndBeginInvoke(DispatcherPriority priority, Action callback);
        void BeginInvokeWhileNeeded(Action callback);
        bool CheckAccess();
        bool IsValid { get; }
    }
}