using System;
using System.Windows.Input;
using UserControls.ViewModels.StockTakeings;

namespace UserControls.Commands
{
    #region Add StoCkTakingItem command
    class AddStoCkTakingItemCommand : ICommand
    {
        private readonly StockTakeViewModel _viewModel;
        public AddStoCkTakingItemCommand(StockTakeViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public bool CanExecute(object param)
        {
            return _viewModel.CanAddStockTakingItem();
        }
        public void Execute(object param)
        {
            _viewModel.AddStockTakingItem();
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
    #endregion
    #region Remove StockTakingItem command

    public class RemoveStockTakingItemCommand : ICommand
    {
        private StockTakeViewModel _viewModel;
        public RemoveStockTakingItemCommand(StockTakeViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _viewModel.CanRemoveStockTakingItem();
        }

        public void Execute(object parameter)
        {
            _viewModel.RemoveStockTakingItem();
        }
    }
    #endregion
    #region Get unavailable product items

    public class UnavailableProductItemsCommand : ICommand
    {
        private readonly StockTakeViewModel _viewModel;
        public UnavailableProductItemsCommand(StockTakeViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object value)
        {
            return _viewModel.CanGetUnavailableProductItems();
        }

        public void Execute(object value)
        {
            _viewModel.GetUnavailableProductItems();
        }
    }
    #endregion
    #region Export to Excel
    public class ExportToExcelCommand : ICommand
    {
        private readonly StockTakeViewModel _viewModel;

        public ExportToExcelCommand(StockTakeViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object value)
        {
            return _viewModel.CanExportToExcel();
        }
        public void Execute(object value)
        {
            _viewModel.ExportToExcel();
        }
    }
    #endregion
    #region View detile
    public class ViewDetileCommand : ICommand
    {
        private readonly StockTakeViewModel _viewModel;
        public ViewDetileCommand(StockTakeViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object value)
        {
            return _viewModel.CanExportToExcel();
        }

        public void Execute(object value)
        {
            _viewModel.ViewDetile();
        }
    }
    #endregion
}
