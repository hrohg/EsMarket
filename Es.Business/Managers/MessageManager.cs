using ES.Business.Models;

namespace ES.Business.Managers
{
    public class MessageManager
    {
        public delegate void NewMessageEvent(MessageModel message);

        public event NewMessageEvent NewMessageReceived;
        public void OnNewMessage(MessageModel message)
        {
            var handler = NewMessageReceived;
            if (handler != null) handler(message);
        }
    }
}
