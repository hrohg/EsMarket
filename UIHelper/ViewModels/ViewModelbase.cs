using ES.Common.ViewModels.Base;

namespace UIHelper.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChanged
    {
        protected object Sync = new object();
    }    
}
