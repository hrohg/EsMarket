using System.Windows.Input;
using ES.Common.Helpers;

namespace ES.Common.ViewModels.Base
{
    public class TabViewModel : PaneViewModel
    {
        #region External properties

        #region Description
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }
        #endregion

        #region IsModified
        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    RaisePropertyChanged("IsModified");
                }
            }
        }
        #endregion 

        #region IsLoading
        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    RaisePropertyChanged("IsLoading");
                }
            }
        }
        #endregion

        #endregion External properties
        
        #region Constructors
        public TabViewModel()
        {
            //IconSource = ISC.ConvertFromInvariantString(@"pack://application:,,/Images/document.png") as ImageSource;
        }
        #endregion Constructors

        #region Commands
        #region SaveCommand
        RelayCommand _saveCommand = null;
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand((p) => OnSave(p), (p) => CanSave(p));
                }

                return _saveCommand;
            }
        }

        private bool CanSave(object parameter)
        {
            return IsModified;
        }

        private void OnSave(object parameter)
        {
            
        }

        #endregion

        //#region CloseCommand
        //RelayCommand _closeCommand = null;
        //public override ICommand CloseCommand
        //{
        //    get
        //    {
        //        if (_closeCommand == null)
        //        {
        //            _closeCommand = new RelayCommand((p) => OnClose(), (p) => CanClose());
        //        }

        //        return _closeCommand;
        //    }
        //}

        //private bool CanClose()
        //{
        //    return true;
        //}

        //private void OnClose()
        //{
            
        //}
        //#endregion
        #endregion Commands
    }
}
