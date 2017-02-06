using System;
using System.Windows.Input;
using UserControls.ViewModels;

namespace UserControls.Commands
{
    class EsUserChangeCommand:ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public EsUserChangeCommand(EsUserViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private EsUserViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanEdit;
        }

        
        public void Execute(object parameter)
        {
            _viewModel.ChangePassword();
        }
    }
}
