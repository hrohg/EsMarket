using System;
using System.Reflection;
using System.Windows;
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
        private Reporter _reporter;
        private string _note;

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

        public string Note
        {
            get { return _note; }
            set { _note = value; RaisePropertyChanged("Note"); }
        }

        #endregion External properties

        #region Constructors
        public ReportExceptionViewModel(Exception ex)
            : this()
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
            var assembly = Assembly.GetEntryAssembly();
            string version = null;
            if (assembly != null)
            {
                version = assembly.GetName().Version.ToString();
            }

            var reporter = string.Format("Company: {0}\n User: {1}", Reporter.Company, Reporter.User);
            var note = string.Format("<span style=\"\"font-family:Arial;font-size: 10pt;>{0}</span><br/>" +
                                     "<span style=\"\"font-family:Arial;font-size: 10pt;>Date: {1}</span><br/>" +
                                     "<span style=\"\"font-family:Arial;font-size: 10pt;>Build: {2}</span><br/>" +
                                     "<p style=\"\"font-family:Arial;font-size: 12pt;>{3}</p>",

                                        reporter,
                                        DateTime.Now,
                                        version,
                                        Note);

            MailSender.SendErrorReport(ExceptionText, _ex.ToString(), note, Reporter.Company);
            MessageBox.Show("Ողջույն \n\nՍխալի վերաբերյալ տեղեկությունն ուղարկել եմ սպասարկման թիմին: Սխալը կուղղվի առաջիկա թարմացումների ժամանակ: Հարկ եղած դեպքում լրացուցիչ կկապվենք Ձեր հետ: \n\nՇնորհակլություն համագործակցության համար:", "Հաղորդագրություններ");
            Note = string.Empty;
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

        public Reporter Reporter
        {
            get { return _reporter; }
            set { _reporter = value; }
        }

        #endregion Commands
    }

    public class Reporter
    {
        public string Company { get; set; }
        public string User { get; set; }
    }
}
