using System;
using System.IO.Packaging;
using System.Windows.Input;
using ES.Common.Helpers;
using ES.Common.ViewModels.Base;
using Shared.Helpers;

namespace UserControls.ViewModels
{
    public class ReportExceptionViewModel : ViewModelBase
    {
        #region Internal properties

        private Exception _ex;
        #endregion Internal properties

        #region External properties

        #region Is show detiles
        private bool _isShowDetiles;

        public bool IsShowDetiles
        {
            get
            {
                return _isShowDetiles;
            }
            set
            {
                if (value == _isShowDetiles) return;
                _isShowDetiles = value;
                RaisePropertyChanged("IsShowDetiles");
                RaisePropertyChanged("ExceptionDetail");
            }
        }

        #endregion Is show detiles

        public string Title { get; private set; }
        public string ExceptionText { get { return _ex != null ? _ex.Message : string.Empty; } }
        public string ExceptionDetail { get { return IsShowDetiles && _ex != null ? _ex.ToString() : string.Empty; } }
        public string Note { get; set; }
        #endregion External properties

        #region Constructors
        public ReportExceptionViewModel(Exception ex)
        {
            Title = "Տեղեկացում սխալի վերաբերյալ";
            _ex = ex;
            Initialize();
        }
        #endregion Contructors

        #region Internal Methods

        private void Initialize()
        {
            CloseCommand = new RelayCommand(OnClose);
            SendExceptionCommand = new RelayCommand(OnSendErrorReport);
        }
        private void OnClose(object o)
        {

        }
        private void OnSendErrorReport(object o)
        {
            MailSender.SendErrorReport(ExceptionText, string.Format("{0}\n\nUser note: {1}", _ex.ToString(), Note));
        }
        #endregion Internal methods

        #region External methods
        #endregion External methods

        #region Commands
        public ICommand CloseCommand { get; private set; }
        public ICommand SendExceptionCommand { get; private set; }
        #endregion Commands
    }
}
