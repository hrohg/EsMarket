using System;
using System.Windows.Input;
using UserControls.ViewModels.Partners;

namespace UserControls.Commands
{
    public class PartnerNewCommand : ICommand
    {
        private readonly PartnerViewModel _viewModel;

        public PartnerNewCommand(PartnerViewModel viewModel)
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
            return _viewModel.CanNewPartner();
        }
        public void Execute(object value)
        { _viewModel.GetNewPartner();}
    }
    public class PartnerAddCommand:ICommand
    {
        private readonly PartnerViewModel _viewModel;
        public PartnerAddCommand(PartnerViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        #region ICommand implements
        public  event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object value)
        {
            return _viewModel.CanAddPartner();
        }

        public void Execute(object value)
        {
            _viewModel.AddPartner();
        }
        #endregion
    }
    public class PartnerEditCommand : ICommand
    {
        private readonly PartnerViewModel _viewModel;
        public PartnerEditCommand(PartnerViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        #region ICommand implements
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object obj)
        {
            return _viewModel.CanEditPartner();
        }
        public void Execute(object obj)
        { _viewModel.EditPartner();}
        #endregion
    }
    public class PartnerRemoveCommand : ICommand
    {
        private readonly PartnerViewModel _viewModel;
        public PartnerRemoveCommand(PartnerViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        #region ICommand implements
        public event EventHandler CanExecuteChanged { 
            add { CommandManager.RequerySuggested += value; } 
            remove { CommandManager.RequerySuggested -= value; } }
        public bool CanExecute(object obj)
        {
            return _viewModel.CanRemovePartner();
        }
        public void Execute(object obj)
        { _viewModel.RemovePartner();}
        #endregion
    }
}
