using MyChat.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Util
{
    public enum OpenDocumentsChangedAction
    {
        Added,
        Removed
    }

    public class OpenDocumentsChangedEventArgs : EventArgs
    {
        public OpenDocumentsChangedEventArgs(OpenDocumentsChangedAction action, Guid identifier)
        {
            Action = action;
            Identifier = identifier;
        }

        public OpenDocumentsChangedAction Action { get; }
        public Guid Identifier { get; }
    }
}
