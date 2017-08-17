using System.ComponentModel;
using System.Windows.Input;
using ES.Common.Helpers;
using UserControls.Helpers;
using UserControls.Views.PrintPreview.Views;

namespace UserControls.Views.PrintPreview
{
    public class PrintPreviewViewModel : INotifyPropertyChanged
    {
        #region Internal properties
        private UiPrintPreview _printPreview;
        private bool _isShowPrintDialog;
        private string _title;
        #endregion

        #region External properties
        #endregion

        #region Constructors

        public PrintPreviewViewModel(UiPrintPreview printPreview, string title, bool isShowPrintDialog = false)
        {
            _printPreview = printPreview;
            _title = title;
            _isShowPrintDialog = isShowPrintDialog;
            Initialize();
        }
        
        #endregion
        
        #region Internal methods
        private void Initialize()
        {
            CloseCommand = new RelayCommand(OnClose);
            PrintCommand = new RelayCommand(OnPrint);
        }

        private void OnClose(object o)
        {
            _printPreview.Close();
        }

        private void OnPrint(object o)
        {
            PrintManager.Print(_printPreview.PageContent, _title, _isShowPrintDialog);
            _printPreview.Close();
        }
        #endregion

        #region External properties
        #endregion

        #region Commands
        public ICommand CloseCommand { get; private set; }
        public ICommand PrintCommand { get; private set; }
        #endregion


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
