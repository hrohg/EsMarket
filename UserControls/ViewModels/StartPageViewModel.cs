using System.Windows.Input;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;

namespace UserControls.ViewModels
{
    public class StartPageViewModel : TabViewModel
    {
        #region Internal properties

        #endregion Internal properties

        #region External properties
        
        #endregion External properties
        
        #region Constructors

        public StartPageViewModel()
        {
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Գլխավոր";
            CanFloat = false;
            IsClosable = false;
            OpenInvocieCommand = new RelayCommand(OnOpenInvoice);
        }

        private void OnOpenInvoice(object o)
        {
            
        }
        #endregion Internal methods

        #region External methods

        #endregion External methods

        #region Commands
        public ICommand OpenInvocieCommand { get; private set; }
        #endregion Commands
    }
}
