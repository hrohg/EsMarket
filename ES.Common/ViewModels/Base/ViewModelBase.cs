using System.Collections.ObjectModel;

namespace ES.Common.ViewModels.Base
{
    public class ViewModelBase : NotifyPropertyChanged
    {
        protected object Sync = new object();
    }

    public class AvalonBasedViewModelBase : ViewModelBase
    {
        #region Internal properties
        private DocumentViewModelBase _activeTab;
        private bool _isInProgress;
        #endregion

        #region External fields
        public bool IsInProgress
        {
            get { return _isInProgress; }
            set
            {
                if (_isInProgress == value) return;
                _isInProgress = value;
                RaisePropertyChanged(() => IsInProgress);
            }
        }

        #region Avalon dock
        public ObservableCollection<DocumentViewModelBase> Documents { get; set; }
        public ObservableCollection<ToolsViewModel> Tools { get; set; }
        public DocumentViewModelBase ActiveTab
        {
            get { return _activeTab; }
            set
            {
                _activeTab = value;
                //RaisePropertyChanged(() => AddSingleVisibility);
            }
        }
        #endregion Avalon dock

        #endregion External fields

        public AvalonBasedViewModelBase() { Documents = new ObservableCollection<DocumentViewModelBase>(); }
    }
}
