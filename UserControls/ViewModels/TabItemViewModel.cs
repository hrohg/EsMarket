using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Common.Helpers;
using ES.Common.Managers;
using UserControls.Interfaces;

namespace UserControls.ViewModels
{
    public class TabItemViewModel : ITabItem
    {
        #region Internal properties
        private string _title;
        private string _description;
        private bool _isModified;
        private bool _isLoading;
        #endregion //Internal properties

        #region External properties
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                if (value == _isLoading) return;
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; OnPropertyChanged("Description"); }
        }

        public bool IsModified
        {
            get
            {
                return _isModified;
            }
            set
            {
                if (value == _isModified) return;
                _isModified = value;
                OnPropertyChanged("IsModified");
            }
        }
        #endregion //External properties

        #region Constants
        #endregion

        #region Constructors

        public TabItemViewModel()
        {
            Initialize();
        }

        #endregion //Constructors

        #region Internal methods
        private void Initialize()
        {
            CloseCommand = new RelayCommand(OnClose);
        }

        private void OnClose(object o)
        {
            ApplicationManager.OnTabItemClose(o as TabItem);
        }
        #endregion //Internal methods

        #region External methods
        #endregion //External methods

        #region Commands
        public ICommand CloseCommand { get; protected set; }
        #endregion //Commands

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }



}
