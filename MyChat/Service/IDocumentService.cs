using MyChat.Model;
using MyChat.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Service
{
    public interface IDocumentService
    {
        event EventHandler<OpenDocumentsChangedEventArgs>? OpenDocumentsChanged;

        IEnumerable<ChatDocument> OpenDocuments { get; }
        ChatDocument CreateDocument(string additionalInstructions);
        ChatDocument? FindDocument(Guid identifier);
        ChatDocument? FindDocument(string documentPath);
        Task<ChatDocument?> OpenDocumentAsync(string documentPath);
        Task<bool> SaveDocumentAsync(ChatDocument document, string documentPath);
        void CloseDocument(ChatDocument document);
        void AddExchange(ChatDocument document, ChatExchange exchange, int tokens);
        string Undo(ChatDocument document);
        void Redo(ChatDocument document);

    }
}
