using System;
using System.Collections.Generic;
using System.Security;
using System.Windows.Input;
using ES.Business.Helpers;
using ES.Common;
using ES.Common.Helpers;
using ES.Common.Enumerations;
using ES.Common.Managers;
using ES.Common.Models;
using ES.Common.ViewModels.Base;

namespace UserControls.ViewModels
{
    public class ServerViewModel : ViewModelBase
    {
        #region Internal properties
        private SecureString _snPassword;
        private SecureString _srPassword;
        #endregion Internal properties

        #region Internal properties

        #endregion
        #region External properties
        public DataServer DataServer { get; set; }
        public bool IsShowPassword { get
        {
            return false;// ApplicationManager.IsInRole(UserRoleEnum.Admin); 
        } }

        public string Password { get; private set; }

        public SecureString SnPassword
        {
            get { return _snPassword; }
            set { _snPassword = value; RaisePropertyChanged("SnPassword"); }
        }

        public SecureString SrPassword
        {
            get { return _srPassword; }
            set { _srPassword = value; RaisePropertyChanged("RepeatedPassword"); }
        }

        #endregion

        public ServerViewModel(DataServer server)
        {
            DataServer = server;
            Initialize();
        }
        #region Internal methods

        private void Initialize()
        {
            Password = DataServer.Password;
            SnPassword = SrPassword = DataServer.Password.ToSecureString();

        }
        #endregion

        #region External methods

        public bool CanSave(object o)
        {
            {
                return !string.IsNullOrEmpty(DataServer.Description) &&
                       !string.IsNullOrEmpty(DataServer.Name) &&
                       !string.IsNullOrEmpty(DataServer.Database) &&
                       SnPassword.ToUnsecureString() == SrPassword.ToUnsecureString();
            }
        }
        public void OnSave(object o)
        {
            DataServer.Password = SnPassword.ToUnsecureString();
            if (!DataServerSettings.SetDataServer(new List<DataServer>(){ DataServer}))
            {
                MessageManager.OnMessage("Գրանցումը ձախողվել է։", MessageTypeEnum.Warning);
                return;
            }
            MessageManager.OnMessage("Գրանցումն իրականացել է հաջողությամբ։", MessageTypeEnum.Success);
            if (ClosingEvent != null)
            {
                ClosingEvent(this, EventArgs.Empty);
            }
        }
        public event EventHandler ClosingEvent;
        public void OnCancel(object o)
        {
            if (ClosingEvent != null)
            {
                ClosingEvent(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Commands
        public ICommand OkButtonCommand { get { return new RelayCommand(OnSave, CanSave); } }
        public ICommand CancelButtonCommand { get { return new RelayCommand(OnCancel); } }
        #endregion
    }
}
