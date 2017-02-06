using System;
using System.Diagnostics;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Market.ViewModels;
using UserControls.Helpers;
using UserControls.Views.ReceiptTickets.Views;

namespace ES.Shop.Commands
{
    public struct InvoiceCommandParameter
    {
        public InvoiceType Type { get; set; }
        public int? Count { get; set; }
    }
    public class RefreshCashCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public RefreshCashCommand(ShellViewModel viewModel)
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
            return _viewModel.CanUpdateCash();
        }


        public void Execute(object parameter)
        {
            _viewModel.UpdateCash();
        }
    }
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
    public class OpenCarculatorCommand : ICommand
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
            return true;
        }
        public void Execute(object parameter)
        {
            Process.Start("calc");
        }
    }
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
            return !string.IsNullOrEmpty(ApplicationManager.ActivePrinter);
        }
        public void Execute(object parameter)
        {
            PrintManager.PrintOnActivePrinter(new ReceiptTicketSmall(null), ApplicationManager.ActivePrinter);
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
            _viewModel.OnChangeSettings();
        }
    }
#endregion
}
