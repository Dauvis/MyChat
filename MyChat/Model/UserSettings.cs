using MyChat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Model
{
    public class UserSettings
    {
        public string DefaultCustomInstructions { get; set; } = string.Empty;
        public string SelectedChatModel { get; set; } = string.Empty;
        public string DefaultTone { get; set; } = SystemPrompts.DefaultTone;
    }
}
