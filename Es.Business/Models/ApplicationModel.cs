using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ES.Business.Models
{
    public class MessageModel : INotifyPropertyChanged
    {
        public enum MessageTypeEnum
        {
            Information = 1,
            Success = 2,
            Error = 3,
            Warning = 4
        }

        #region Properties

        private const string DateProperty = "Date";
        private const string MessageProperty = "Message";
        #endregion

        #region Internal properties

        private DateTime _date = DateTime.Now;
        private string _message;
        #endregion

        #region External properties
        public DateTime Date
        {
            get { return _date; }
            set
            {
                if (_date == value) { return; }
                _date = value;
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message == value) { return; }
                _message = value;
            }
        }
        public MessageTypeEnum MessageType { get; set; }

        public Brush MessageForeground
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypeEnum.Success:
                        return Brushes.Green;

                    case MessageTypeEnum.Error:
                        return Brushes.Red;

                    case MessageTypeEnum.Information:
                        return Brushes.Blue;

                    case MessageTypeEnum.Warning:
                        return Brushes.BlueViolet;

                    default:
                        return Brushes.Black;
                }
            }
        }

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

        public MessageModel(DateTime date, string message, MessageTypeEnum type)
        {
            Date = date;
            Message = message;
            MessageType = type;
        }
        public MessageModel(string message, MessageTypeEnum type)
            : this(DateTime.Now, message, type)
        {
        }
    }
}
