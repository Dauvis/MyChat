using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.DTO
{
    public class NewChatDTO
    {
        public bool IsOk { get; set; }
        public string Instructions { get; set; } = "";
        public string Tone { get; set; } = "";
        public string Topic { get; set; } = "";
    }
}
