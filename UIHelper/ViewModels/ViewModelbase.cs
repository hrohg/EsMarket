using System.ComponentModel;

namespace UIHelper.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected object Sync = new object();
        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
