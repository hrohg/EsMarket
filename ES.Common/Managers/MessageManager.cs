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
            Manager.SendMessage(new MessageModel(message, type));
        }
        public static void OnMessage(MessageModel message)
        {
            Manager.SendMessage(message);
        }
        #endregion External methods

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
