using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Util
{
    public class NewChatEventArgs : EventArgs
    {
        public NewChatEventArgs(string summary, string title, string tone, string instructions)
        {
            Summary = summary;
            Title = title;
            Tone = tone;
            Instructions = instructions;
        }

        public string Summary { get; set; }
        public string Title { get; set; }
        public string Tone { get; set; }
        public string Instructions { get; set; }
    }
}
