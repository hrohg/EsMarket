using System;
using System.Drawing;
using System.Windows;
using ES.Common.Enumerations;

namespace ES.Common.Models
{
    public class MessageModel
    {
        #region Properties

        #endregion

        #region Internal properties

        private DateTime _date = DateTime.Now;

        #endregion

        #region External properties
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
            }
        }
        public string Title { get; set; }
        public string Message { get; set; }
        public MessageBoxImage MessageBoxImage
        {
            get
            {
                switch (MessageType)
                {
                    case MessageTypeEnum.Information:
                        return MessageBoxImage.Information;
                    case MessageTypeEnum.Success:
                        return MessageBoxImage.Information;
                    case MessageTypeEnum.Error:                        
                        return MessageBoxImage.Error;
                    case MessageTypeEnum.Warning:
                        return MessageBoxImage.Warning;
                }
                return MessageBoxImage.None;
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
        public MessageModel(string message, string title, MessageTypeEnum type)
        {
            Date = DateTime.Now;
            Message = message;
            Title = title;
            MessageType = type;
        }
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
