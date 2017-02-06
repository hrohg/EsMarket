using System;

namespace ES.Business.DataUtilities
{
    public abstract class Disposable : IDisposable
    {
        private bool _isDisposed;

        ~Disposable()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                DisposeCore();
            }

            _isDisposed = true;
        }

        protected abstract void DisposeCore();
    }
}
