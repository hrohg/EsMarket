using System;
using System.ComponentModel;

namespace ES.Common.Models
{
    public class ModelBase : INotifyPropertyChanged
    {

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
