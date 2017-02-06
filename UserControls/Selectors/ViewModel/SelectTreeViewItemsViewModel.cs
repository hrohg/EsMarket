using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ES.Common.Helpers;

namespace UserControls.Selectors.ViewModel
{
    public class SelectTreeViewItemsViewModel : INotifyPropertyChanged
    {
        #region Internal properties
        #endregion
        #region External properties
        public string SearchKey { get; set; }
        #endregion
        #region Constructors
        #endregion

        #region Internal methods
        private void OnCancel(object o)
        { }

        private void OnOk(object o)
        {

        }
        #endregion
        #region External methods

        #endregion
        #region Commands
        public ICommand CancelCommand { get { return new RelayCommand(OnCancel);} }
        public ICommand OkCommand { get { return new RelayCommand(OnOk); } }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
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
