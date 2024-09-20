using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Util
{
    public class ChatTitleEventArgs : EventArgs
    {
        public ChatTitleEventArgs(bool set, string title)
        {
            Set = set;
            Title = title;
        }

        public bool Set { get; }
        public string Title { get; set; }
    }
}
