using System;
using System.ComponentModel;
using System.Windows.Input;
using ES.Business.Managers;
using ES.Business.Models;
using ES.Common;
using ES.Common.Helpers;
using ES.Common;

namespace UserControls.ViewModels
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        #region Internal properties

        #endregion
        #region External properties
        public DataServer DataServer { get; set; }
        #endregion

        public ServerViewModel(DataServer server)
        {
            DataServer = server;
        }
        #region Internal methods
        #endregion
        #region External methods

        public bool CanSave(object o)
        {
            {
                return !string.IsNullOrEmpty(DataServer.Description) &&
                       !string.IsNullOrEmpty(DataServer.Name) &&
                       !string.IsNullOrEmpty(DataServer.Database);
            }
        }
        public void OnSave(object o)
        {
            if (!ConfigSettings.SetDataServer(DataServer))
            {
                ApplicationManager.MessageManager.OnNewMessage(
                new MessageModel("Գրանցումը ձախողվել է։", MessageModel.MessageTypeEnum.Warning));
                return;
            }
            ApplicationManager.MessageManager.OnNewMessage(
                new MessageModel("Գրանցումն իրականացել է հաջողությամբ։",  MessageModel.MessageTypeEnum.Success));
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
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
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
