using MyChat.Model;

namespace MyChat.Data
{
    public interface IChatDocumentRepository
    {
        void CloseDocument(ChatDocument document);
        ChatDocument CreateDocument();
        Task<ChatDocument?> OpenDocumentAsync(string documentPath);
        Task<bool> SaveDocumentAsync(ChatDocument document, string documentPath);
    }
}