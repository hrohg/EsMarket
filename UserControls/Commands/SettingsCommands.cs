using System;
using System.Windows.Input;
using UserControls.ViewModels;

namespace UserControls.Commands
{
    #region Set sale from stock
    public class SetSaleFromStock:ICommand
    {
        private readonly SettingsViewModel _viewmodel ;

        public SetSaleFromStock(SettingsViewModel viewModel)
        {
            _viewmodel = viewModel;
        }
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object o)
        {
            return _viewmodel.CanSetSaleFromStock();
        }
        public void Execute(object o)
        { _viewmodel.SetSaleFromStock();}
    }
    #endregion
    #region Set cash desk

    public class SetCashDesk : ICommand
    {
        private readonly SettingsViewModel _viewModel;

        public SetCashDesk(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object o)
        {
            return _viewModel.CanSetChashDesk();
        }
        public void Execute(object o)
        {
            _viewModel.SetCashDesk();
        }
    }
    #endregion
    #region Set Default Settings

    public class SetApplicationSettings : ICommand
    {
        private readonly SettingsViewModel _viewModel;

        public SetApplicationSettings(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;

        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object o)
        {
            return _viewModel.CanSetApplicationSettings();
        }
        public void Execute(object o)
        {
            _viewModel.SetApplicationSettiongs();
        }
    }
    #endregion
    #region Set Printers
    public class SetPrintersCommand : ICommand
    {
        private readonly SettingsViewModel _viewModel;

        public SetPrintersCommand(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object o)
        {
            return true;
        }
        public void Execute(object o)
        {
            _viewModel.OnSetPrinters();
        }
    }
    #endregion
}
