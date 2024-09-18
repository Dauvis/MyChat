using MyChat.Model;

namespace MyChat.Data
{
    public interface IChatDocumentRepository
    {
        void CloseDocument(ChatDocument document);
        ChatDocument CreateDocument();
        ChatDocument? OpenDocument(string documentPath);
        bool SaveDocument(ChatDocument document, string documentPath);
    }
}