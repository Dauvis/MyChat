using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Messages
{
    public class CursorStateChangeMessage
    {
        public CursorStateChangeMessage(bool isWaiting)
        {
            IsWaiting = isWaiting;
        }

        public bool IsWaiting { get; }
    }
}
