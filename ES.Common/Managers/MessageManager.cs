using ES.Common.Enumerations;
using ES.Common.Models;

namespace ES.Common.Managers
{
    public class MessageManager
    {
        #region Internal fields


        #endregion Internal fields
        #region Internal properties

        #endregion Internal properties

        private static readonly MessageManager Manager;

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
            Manager.SendMessage(message, type);
        }
        public static void OnMessage(MessageModel message)
        {
            Manager.SendMessage(message);
        }
        #endregion External methods

        #region Events

        public delegate void MessageReceivedDelegate(string message, MessageTypeEnum type);
        public event MessageReceivedDelegate MessageReceived;

        private void SendMessage(string message, MessageTypeEnum type)
        {
            var handler = MessageReceived;
            if (handler != null) handler(message, type);
        }

        public delegate void NewMessageEvent(MessageModel message);
        public event NewMessageEvent NewMessageReceived;
        private void SendMessage(MessageModel message)
        {
            var handler = NewMessageReceived;
            if (handler != null) handler(message);
        }
        public void OnNewMessage(string message, MessageTypeEnum type)
        {
            var handler = NewMessageReceived;
            if (handler != null) handler(new MessageModel(message, type));
        }
        #endregion Events
    }
}
