using System;
using System.Windows.Input;
using ES.Business.Managers;
using Shared.Helpers;
using UserControls.ViewModels.Invoices;

namespace UserControls.Commands
{
    class InvoiceCommands : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public InvoiceCommands(InvoiceViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private InvoiceViewModel _viewModel;
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
            //_viewModel.EditProduct();
        }
    }
    class RemoveInvoiceItemCommands : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public RemoveInvoiceItemCommands(InvoiceViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private InvoiceViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanRemoveInvoiceItem();
        }
        public void Execute(object parameter)
        {
            _viewModel.RemoveInvoiceItem();
        }
    }
   
    #region Import invoice command
    
    #endregion
    #region Approve invoice command class
    public class ApproveMoveInvoiceCommands : ICommand
    {
        #region Properties
        private InvoiceViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion
        public ApproveMoveInvoiceCommands(InvoiceViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        #region Methods
        public bool CanExecute(object o)
        {
            return _viewModel.CanApproveMoveInvoice();
        }
        public void Execute(object o)
        {
            _viewModel.ApproveMoveInvoice();
        }
        #endregion
    }
    #endregion
    #region Approve invoice command class
    public class ApproveCloseInvoiceCommands : ICommand
    {
        #region Properties
        private InvoiceViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion
        public ApproveCloseInvoiceCommands(InvoiceViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        #region Methods
        public bool CanExecute(object o)
        {
            return _viewModel.CanApprove(o);
        }
        public void Execute(object o)
        {
            _viewModel.OnApproveAndClose(o);
        }
        #endregion
    }
#endregion
    #region AddItemsFromPurchaseInvoice
        public class AddItemsFromInvoiceCommands : ICommand
        {
            #region Properties
            private InvoiceViewModel _viewModel;
            private InvoiceType _type;
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            #endregion
            public AddItemsFromInvoiceCommands(InvoiceViewModel viewModel, InvoiceType type)
            {
                _viewModel = viewModel;
                _type = type;
            }
            #region Methods
            public bool CanExecute(object o)
            {
                return _viewModel.CanAddItemsFromPurchaseInvoice();
            }
            public void Execute(object o)
            {
                _viewModel.AddItemsFromExistingInvoice(_type);
            }
            #endregion
        }
    #endregion
    #region AddItemsFromStockInvoice
    public class AddItemsFromStocksCommand : ICommand
    {
        #region Properties
        private InvoiceViewModel _viewModel;
        private InvoiceType _type;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        #endregion
        public AddItemsFromStocksCommand(InvoiceViewModel viewModel, InvoiceType type)
        {
            _viewModel = viewModel;
            _type = type;
        }
        #region Methods
        public bool CanExecute(object o)
        {
            return _viewModel.CanAddItemsFromStocks();
        }
        public void Execute(object o)
        {
            _viewModel.AddItemsFromStocks();
        }
        #endregion
    }
    #endregion
   
    #region Export invoice to Excel rus command
    
    #endregion
    
}
