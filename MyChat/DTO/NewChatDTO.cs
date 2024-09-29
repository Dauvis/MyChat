using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.DTO
{
    public class NewChatDTO
    {
        public bool IsOk { get; set; }
        public string CustomInstructions { get; set; } = "";
        public string Tone { get; set; } = "";
    }
}
