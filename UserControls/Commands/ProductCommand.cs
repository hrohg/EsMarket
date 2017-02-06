using System;
using System.Windows.Input;
using ES.Common;
using ES.Common.Enumerations;
using ES.Common;
using UserControls.ViewModels.Products;

namespace UserControls.Commands
{
    internal class ProductNewCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public ProductNewCommand(ProductViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ProductViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.CanClean();
        }


        public void Execute(object parameter)
        {
            _viewModel.NewProduct();
        }
    }
    internal class ProductCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public ProductCommand(ProductViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ProductViewModel _viewModel;
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
            _viewModel.OnEditProduct();
        }
    }
    internal class ProductDeleteCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public ProductDeleteCommand(ProductViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        private ProductViewModel _viewModel;
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
            _viewModel.DeleteProduct((Guid)parameter);
        }
    }
    public class ProductCodeChangeCommand : ICommand
    {
        private ProductViewModel _viewModel;

        public ProductCodeChangeCommand(ProductViewModel viewModel)
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
            return _viewModel.CanChangeProductCode();
        }

        public void Execute(object value)
        {
            _viewModel.ChangeProductCode();
        }
    }
    internal class PrintBarcodeCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductEditCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        public PrintBarcodeCommand(ProductViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        private ProductViewModel _viewModel;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public bool CanExecute(object parameter)
        {
            return _viewModel.OnCanPrintBarcode(parameter);
        }


        public void Execute(object parameter)
        {
            var preview = HgConvert.ToBoolean(parameter);
            if (preview)
            { _viewModel.PrintPreviewBarcode(parameter); return; }
            _viewModel.PrintBarcode(parameter);
        }
    }

    internal class ProductCopyCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the ProductCopyCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        private ProductViewModel _viewModel;
        public ProductCopyCommand(ProductViewModel viewModel)
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
            return _viewModel.CanCopyProduct();
        }
        public void Execute(object o)
        { _viewModel.CopyProduct(); }
    }

    internal class ProductPastCommand : ICommand
    {
        /// <summary>
        /// Initialize a new instance of the PastProductCommand class.
        /// </summary>
        /// <param name="viewModel"></param>
        private ProductViewModel _viewModel;

        public ProductPastCommand(ProductViewModel viewModel)
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
            return _viewModel.CanPastProduct();
        }

        public void Execute(object o)
        {
            _viewModel.PastProduct();
        }
    }
    /// <summary>
    /// Get product command
    /// </summary>
    public class GetProductsCommand : ICommand
    {
        private ProductViewModel _viewModel;

        public GetProductsCommand(ProductViewModel viewModel)
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
            return true;
        }

        public void Execute(object value)
        {
            _viewModel.GetProductBy(value as ProductViewType?);
        }
    }
    public class ChangeProductEnabledCommand : ICommand
    {
        private ProductViewModel _viewModel;

        public ChangeProductEnabledCommand(ProductViewModel viewModel)
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
            return _viewModel.CanChangeProductEnabled;
        }

        public void Execute(object value)
        {
            _viewModel.ChangeProductEnabled();
        }
    }
    public class AddProductGroupCommand : ICommand
    {
        private ProductViewModel _viewModel;

        public AddProductGroupCommand(ProductViewModel viewModel)
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
            return _viewModel.Product!=null;
        }

        public void Execute(object value)
        {
            _viewModel.OnAddProductGroup(value.ToString());
        }
    }
}
