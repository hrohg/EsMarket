using System;
using System.Windows.Input;
using System.Windows.Media;
using ES.Common.Helpers;

namespace ES.Common.ViewModels.Base
{
    public class PaneViewModel : ViewModelBase
    {
        public delegate void CloseModel(PaneViewModel vm);

        public event CloseModel OnClosed;

        #region External properties
        
        #region Title
        private string _title;
        public virtual string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }
        #endregion

        #region IsFloating

        private bool _isFloating;

        public virtual bool IsFloating
        {
            get { return _isFloating; }
            set
            {
                if (value != _isFloating)
                {
                    _isFloating = value;
                    RaisePropertyChanged("IsFloating");
                }
            }
        }
        #endregion IsFloating

        #region CanFloat

        private bool _canFloat;

        public virtual bool CanFloat
        {
            get { return _canFloat; }
            set
            {
                if (value != _canFloat)
                {
                    _canFloat = value;
                    RaisePropertyChanged("CanFloat");
                }
            }
        }
        #endregion CanFloat

        #region IsModified
        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                RaisePropertyChanged("IsModified");
            }
        }
        #endregion IsModified

        public Guid Id { get; set; }
        #region Closable

        private bool _isClosable;

        public bool IsClosable
        {
            get { return _isClosable; }
            set
            {
                if (value != _isClosable)
                {
                    _isClosable = value;
                    RaisePropertyChanged("IsClosable");
                    RaisePropertyChanged("CanClose");
                }
            }
        }
        #endregion Closable

        public ImageSource IconSource
        {
            get;
            protected set;
        }

        #region ContentId

        private string _contentId = null;
        public string ContentId
        {
            get { return _contentId; }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    RaisePropertyChanged("ContentId");
                }
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected = false;
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region IsActive

        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        #endregion

        #endregion External properties

        #region Contructors
        public PaneViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            CloseCommand = new RelayCommand(OnClose, CanClose);
        }

        protected virtual bool CanClose(object o)
        {
            return IsClosable;
        }
        protected virtual void OnClose(object o)
        {
            if(!CanClose(o)) return;
            var handle = OnClosed;
            if (handle != null) handle(this);
        }
        #endregion Internal methods

        #region External methods
        #endregion External methods

        #region Commands
        public virtual ICommand CloseCommand { get; private set; }
        public virtual ICommand CloseButThisCommand { get; private set; }
        #endregion Commands
    }
}
