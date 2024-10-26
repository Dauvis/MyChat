using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Messages
{
    public enum WindowEventType
    {
        Loaded,         // window was loaded
        Refresh,        // window should fully refresh
        Closing,        // window is closing
        Selection,      // selection of control changed
        DoClose,        // request window/popup to be closed        
        Focus           // request window to take focus
    }

    public enum WindowType
    {
        Main,
        ImageTool,
        ChatTemplate,
        Setting,
        NewChat,
        NewDocumentPopup,
        QnA
    }

    public class WindowEventMessage
    {
        public WindowEventType State {  get; set; }
        public WindowType Type { get; set; }
        public string Parameter { get; set; } = "";
        public object? ObjectParameter { get; set; }
        public bool Cancel { get; set; }

        public WindowEventMessage(WindowEventType state, WindowType type)
        {
            State = state;
            Type = type;
        }
    }
}
