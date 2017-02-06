using System;
using System.Windows.Input;
using UserControls.ViewModels;

namespace UserControls.Commands
{
    public class AccountingRecordsCommands:ICommand
    {
        #region Private properties
        private readonly AccountingRecordsViewModel _viewModel;
        #endregion

        public AccountingRecordsCommands(AccountingRecordsViewModel viewModel)
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
            return _viewModel.CanExecute();
        }

        public void Execute(object value)
        {
            _viewModel.Execute();
        }
    }

    public class RecordRenewCommands : ICommand
    {
         #region Private properties
        private readonly AccountingRecordsViewModel _viewModel;
        #endregion

        public RecordRenewCommands(AccountingRecordsViewModel viewModel)
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
            return _viewModel.CanRenewRecord();
        }

        public void Execute(object value)
        {
            _viewModel.RecordRenew();
        }
    }

    public class EditSubAccountingCommands : ICommand
    {
        private AccountingPlanViewModel _viewModel;

        public EditSubAccountingCommands(AccountingPlanViewModel viewModel)
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
            return _viewModel.CanEditSubAccounting();
        }

        public void Execute(object value)
        {
            _viewModel.EditSubAccountingPlan();
        }
    }
}
