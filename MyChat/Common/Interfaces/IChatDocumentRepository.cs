using MyChat.Common.Model;

namespace MyChat.Common.Interfaces
{
    public interface IChatDocumentRepository
    {
        void CloseDocument(ChatDocument document);
        ChatDocument CreateDocument();
        ChatDocument? OpenDocument(string documentPath);
        bool SaveDocument(ChatDocument document, string documentPath);
    }
}