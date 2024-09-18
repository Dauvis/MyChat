using MyChat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyChat.Model
{
    public class UserSettings
    {
        public string DefaultCustomInstructions { get; set; } = string.Empty;
        public string SelectedChatModel { get; set; } = string.Empty;
        public string DefaultTone { get; set; } = SystemPrompts.DefaultTone;
        public List<string> LastOpenFiles { get; set; } = [];
        public MainWindowInfo MainWindow { get; set; } = new();

        public class MainWindowInfo
        {
            public Rect Rectangle { get; set; }
            public double ChatColumnWidth { get; set; }
            public double MessageColumnWidth { get; set; }
        }
    }
}
