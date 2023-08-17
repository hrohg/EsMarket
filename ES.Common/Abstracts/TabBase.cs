using ES.Common.Interfaces;
using ES.Common.ViewModels.Base;
using System;

namespace ES.Common.Abstracts
{
    public abstract class TabBase : NotifyPropertyChanged, ILayoutDocumentTabItem, IDisposable
    {
        public event EventHandler IsActiveChanged;
        //public event EventHandler<IToolsPanelViewModel> ToolsPanelChanged;
        public event EventHandler LoadCompleted;
        //public event EventHandler<bool> Closed;//true if only closed tab (tab closed, but nod disposed, it still alive)

        private IToolsPanelViewModel _toolsPanel;
        private bool _isFloating;

        protected bool _isActive;

        public bool IsLoadComleted { get; set; }
        public bool IsDisposed { get; private set; }
        public int Index { get; set; }
        
        public virtual IToolsPanelViewModel ToolsPanel
        {
            get { return _toolsPanel; }
            set
            {
                if (Equals(value, _toolsPanel)) return;
                _toolsPanel = value;
                OnToolsPanelChanged(_toolsPanel);
            }
        }
        public virtual bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                if (value.Equals(_isActive)) return;
                _isActive = value;
                RaisePropertyChanged(() => IsActive);
                OnIsActiveChanged(_isActive);
                IsLoadComleted = false;
                //if (_isActive) EventAggregator.GetEvent<ActiveTabChangedEvent>().Publish(this);
            }
        }
        public bool IsFloating
        {
            get { return _isFloating; }
            set
            {
                if (value.Equals(_isFloating)) return;
                _isFloating = value;
                RaisePropertyChanged(() => IsFloating);
                if (!_isFloating) return;
                //EventAggregator.GetEvent<SurfaceFloatingEvent>().Publish(this);
            }
        }
        public void Dispose()
        {
            IsDisposed = true;
            //ToolsPanelChanged = null;
            IsActiveChanged = null;
            LoadCompleted = null;
            //Closed = null;
        }

        private void OnToolsPanelChanged(IToolsPanelViewModel toolsPanel)
        {
            //if (ToolsPanelChanged != null) ToolsPanelChanged.Invoke(this, toolsPanel);
        }
        protected virtual void OnIsActiveChanged(bool isActive)
        {
            //if (isActive) EventAggregator.GetEvent<StatusBarTextEvent>().Publish(string.Empty);

            if (IsActiveChanged != null) IsActiveChanged.Invoke(this, null);
        }
    }
}
