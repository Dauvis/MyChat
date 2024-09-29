using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Messages
{
    public enum ImageToolWindowStateAction
    {
        Startup,
        Shutdown,
        Refresh
    }

    public class ImageToolWindowStateMessage
    {
        public ImageToolWindowStateMessage(ImageToolWindowStateAction stateAction)
        {
            StateAction = stateAction;
        }

        public ImageToolWindowStateAction StateAction { get; }
    }
}
