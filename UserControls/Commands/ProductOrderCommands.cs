using System;
using System.Windows.Input;
using UserControls.ViewModels.Products;

namespace UserControls.Commands
{
    public class ProductOrderEditCommands : ICommand
    {
        private ProductOrderViewModel _viewModel;
        public ProductOrderEditCommands(ProductOrderViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {

        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
