namespace MyChat.Common.CommunicationEventArgs
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
