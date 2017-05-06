using System;

namespace Gateway
{
    public class NewMessageEventArgs : EventArgs
    {
        public Message Message { get; set; }

        public NewMessageEventArgs(Message m)
        {
            this.Message = m;
        }
    }
}