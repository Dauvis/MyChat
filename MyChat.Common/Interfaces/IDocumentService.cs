﻿using MyChat.Common.CommunicationEventArgs;
using MyChat.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.Interfaces
{
    public interface IDocumentService
    {
        IEnumerable<ChatDocument> OpenDocuments { get; }
        ChatDocument CreateDocument(string tone, string instructions, string topic = "");
        ChatDocument? FindDocument(Guid identifier);
        ChatDocument? FindDocument(string documentPath);
        ChatDocument? OpenDocument(string documentPath);
        bool SaveDocument(ChatDocument document, string documentPath);
        void CloseDocument(ChatDocument document);
        void AddExchange(ChatDocument document, ChatExchange exchange, int tokens);
        string Undo(ChatDocument document);
        void Redo(ChatDocument document);
        void UpdateUserSettings(UserSettings userSettings);
        void OpenDocumentList(List<string> documentPaths);
        void SubscribeToOpenDocumentsChanged(EventHandler<OpenDocumentsChangedEventArgs> handler);
        void UnsubscribeFromOpenDocumentsChanged(EventHandler<OpenDocumentsChangedEventArgs> handler);
    }
}
