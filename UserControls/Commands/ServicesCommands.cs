using System;
using System.Windows.Input;
using UserControls.ViewModels;

namespace UserControls.Commands
{
    public class NewServicesCommand:ICommand
    {
         /// <param name="viewModel"></param>
        public NewServicesCommand(ServicesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ServicesViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanCreateNewService();
        }

        
        public void Execute(object parameter)
        {
            _viewModel.CreateNewService();
        }
    }
    public class EditServicesCommand:ICommand
    {
         /// <param name="viewModel"></param>
        public EditServicesCommand(ServicesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ServicesViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanEditService();
        }

        
        public void Execute(object parameter)
        {
            _viewModel.EditService();
        }
    }
    public class RemoveServicesCommand:ICommand
    {
         /// <param name="viewModel"></param>
        public RemoveServicesCommand(ServicesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ServicesViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanRemoveService();
        }

        
        public void Execute(object parameter)
        {
            _viewModel.RemoveService();
        }
    }
}
