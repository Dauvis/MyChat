using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Util
{
    public class NewChatEventArgs : EventArgs
    {
        public NewChatEventArgs(string topic, string title, string tone, string instructions)
        {
            Topic = topic;
            Title = title;
            Tone = tone;
            Instructions = instructions;
        }

        public string Topic { get; set; }
        public string Title { get; set; }
        public string Tone { get; set; }
        public string Instructions { get; set; }
    }
}
