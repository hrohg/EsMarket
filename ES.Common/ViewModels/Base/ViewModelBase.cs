﻿using System.ComponentModel;

namespace ES.Common.ViewModels.Base
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected object Sync = new object();
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
