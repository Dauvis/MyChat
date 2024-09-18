using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Messages
{
    public class ChatViewUpdatedMessage
    {
        public ChatViewUpdatedMessage(string chatText, bool isReplacement = false)
        {
            ChatText = chatText;
            IsReplacement = isReplacement;
        }

        public bool IsReplacement { get; set; }
        public string ChatText { get; set; }
    }
}
