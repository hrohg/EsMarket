using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ES.Common.Enumerations;
using ES.Common.Helpers;
using ES.Common.Models;

namespace ES.Common.Managers
{
    public class MessageManager
    {
        #region Internal fields


        #endregion Internal fields
        #region Internal properties

        #endregion Internal properties

        public static readonly MessageManager Manager;

        #region Contructors

        static MessageManager()
        {
            if (Manager == null)
                Manager = new MessageManager();
        }

        #endregion Constructors

        #region External methods

        public static void OnMessage(string message, MessageTypeEnum type = MessageTypeEnum.Information)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && !Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread && Thread.CurrentThread.IsAlive)
            {
                Manager.SendMessage(new MessageModel(message, type));
            }
            else
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => Manager.SendMessage(new MessageModel(message, type))));
                }
            }
        }
        public static void OnMessage(MessageModel message)
        {
            Manager.SendMessage(message);
        }
        #endregion External methods

        public static void ShowMessage(string message, string title = "Սխալ", MessageBoxImage messageBoxImage = MessageBoxImage.Warning)
        {
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
                {
                    MessageBox.Show(message, title, MessageBoxButton.OK, messageBoxImage);
                });
        }
        public static MessageBoxResult ShowMessage(string message, string title, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage = MessageBoxImage.Warning)
        {
            MessageBoxResult result = MessageBoxResult.None;
            DispatcherWrapper.Instance.Invoke(DispatcherPriority.Send, () =>
            {
                result = MessageBox.Show(message, title, messageBoxButton, messageBoxImage);
            });
            return result;
        }
        #region Events

        public delegate void MessageReceivedDelegate(MessageModel message);
        public event MessageReceivedDelegate MessageReceived;

        private void SendMessage(MessageModel message)
        {
            var handler = MessageReceived;
            if (handler != null) handler(message);
        }
        #endregion Events
    }
}
