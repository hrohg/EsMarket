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

        private string _exeptionDetile;
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

        public string ExceptionDetail
        {
            get { return _exeptionDetile; }
            private set { _exeptionDetile = value; RaisePropertyChanged("ExceptionDetail"); }
        }
        public string Note { get; set; }
        #endregion External properties

        #region Constructors
        public ReportExceptionViewModel(Exception ex):this()
        {
            _ex = ex;
        }
        public ReportExceptionViewModel()
        {
            Title = "Տեղեկացում սխալի վերաբերյալ";
            Initialize();
        }
        #endregion Contructors

        #region Internal Methods

        private void Initialize()
        {
            _exeptionDetile = string.Empty;
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

        public void OnException(Exception ex)
        {
            _ex = ex;
            ExceptionDetail += ex.ToString() + " \n\n";
            RaisePropertyChanged("ExceptionText"); 
        }
        #endregion External methods

        #region Commands
        public ICommand CloseCommand { get; private set; }
        public ICommand SendExceptionCommand { get; private set; }
        #endregion Commands
    }
}
