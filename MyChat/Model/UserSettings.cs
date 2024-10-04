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
        public string DefaultInstructions { get; set; } = "";
        public string SelectedChatModel { get; set; } = "";
        public string DefaultTone { get; set; } = "";
        public List<string> LastOpenFiles { get; set; } = [];
        public MainWindowInfo MainWindow { get; set; } = new();
        public ImageToolWindowInfo ImageToolWindow { get; set; } = new();

        public class MainWindowInfo
        {
            public Rect Rectangle { get; set; }
            public double ChatColumnWidth { get; set; }
            public double MessageColumnWidth { get; set; }
        }

        public class ImageToolWindowInfo
        {
            public Rect Rectangle { get; set; }
            public double PromptColumnWidth { get; set; }
        }
    }
}
