using System;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Market.ViewModels;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets.Views;

namespace ES.Market.Commands
{
    #region Other
    public class ExportProductsForScaleCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public ExportProductsForScaleCommand(ShellViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ShellViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }


        public void Execute(object parameter)
        {
            _viewModel.ExportProductsForScale(parameter);
        }
    }
    #endregion
    #region Help
    public class PrintSampleInvoiceCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return !string.IsNullOrEmpty(ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
        }
        public void Execute(object parameter)
        {
            PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(null), ApplicationManager.Settings.SettingsContainer.MemberSettings.ActiveSalePrinter);
        }
    }
    #endregion

#region Settings
    public class ChangeSettingsCommand : ICommand
    {
        private ShellViewModel _viewModel;
        /// <summary>
        /// Initialize a new instance of the SettingsCommand class.
        /// </summary>
        public ChangeSettingsCommand(ShellViewModel viewModel)
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
            return true;
        }
        public void Execute(object parameter)
        {
            _viewModel.OnManageSettings();
        }
    }
#endregion
}
