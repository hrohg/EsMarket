using System.Windows.Input;
using ES.Common.Helpers;
using ES.Common.Interfaces;

namespace ES.Common.ViewModels.Base
{
    public class UpdatableDocumentViewModel:DocumentViewModel, IUpdatable
    {
        private ICommand _updateCommand;

        public virtual void OnUpdate()
        {
            
        }
        public ICommand UpdateCommand { get { return _updateCommand ?? (_updateCommand = new RelayCommand(OnUpdate)); } }
    }
}
