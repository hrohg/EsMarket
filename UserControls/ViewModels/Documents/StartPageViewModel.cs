using System.Windows.Input;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using UserControls.Enumerations;
using UserControls.Interfaces;

namespace UserControls.ViewModels.Documents
{
    public class StartPageViewModel : DocumentViewModel
    {
        #region Internal properties

        private readonly IShellViewModel _parent;
        #endregion Internal properties

        #region External properties

        #endregion External properties

        #region Constructors

        public StartPageViewModel(IShellViewModel parent)
        {
            _parent = parent;
            Initialize();
        }
        #endregion Constructors

        #region Internal methods

        private void Initialize()
        {
            Title = "Գլխավոր";
            CanFloat = false;
            IsClosable = false;
            OpenInvocieCommand = new RelayCommand(OnOpenInvoice);
            GetReportCommand = new RelayCommand<ReportTypes>(OnGetReport);
            OpenCarculatorCommand = new RelayCommand(_parent.OnOpenCalc);
            ToolsCommand = new RelayCommand<ToolsEnum>(_parent.OnTools);
        }

        private void OnOpenInvoice(object o)
        {
            _parent.OnGetInvoices(o);
        }

        private void OnGetReport(ReportTypes type)
        {
            _parent.OnGetReport(type);
        }
        #endregion Internal methods

        #region External methods

        #endregion External methods

        #region Commands

        #region Invoices
        public ICommand OpenInvocieCommand { get; private set; }
        #endregion Invoices

        #region Reports
        public ICommand GetReportCommand { get; private set; }
        public ICommand OpenCarculatorCommand { get; private set; }
        #endregion Reports
        public ICommand ToolsCommand { get; private set; }
        #endregion Commands
    }
}
