using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Messages
{
    public enum MainWindowStateAction
    {
        Startup,
        Shutdown,
        Refresh
    }
    public class MainWindowStateMessage
    {
        public MainWindowStateMessage(MainWindowStateAction stateAction)
        {
            StateAction = stateAction;
        }

        public MainWindowStateAction StateAction { get; }
    }
}
