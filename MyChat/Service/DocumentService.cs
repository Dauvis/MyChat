using MyChat.Common.CommunicationEventArgs;
using MyChat.Common.Interfaces;
using MyChat.Common.Model;
using MyChat.Common.Util;
using System.IO;

namespace MyChat.Service
{
    public class DocumentService : IDocumentService
    {
        private static readonly List<ChatDocument> _openDocuments = [];
        private readonly IChatDocumentRepository _repository;
        private readonly SystemMessageUtil _systemMessageUtil;

        private static event EventHandler<OpenDocumentsChangedEventArgs>? _openDocumentsChanged;

        public IEnumerable<ChatDocument> OpenDocuments => _openDocuments.AsEnumerable();

        public DocumentService(IChatDocumentRepository repository, SystemMessageUtil systemMessageUtil)
        {
            _repository = repository;
            _systemMessageUtil = systemMessageUtil;
        }

        public ChatDocument CreateDocument(string tone, string instructions, string topic = "")
        {
            string systemPrompt = _systemMessageUtil.ChatSystemMessage(tone, instructions, topic);
            ChatExchange systemExchange = new(systemPrompt, "", ExchangeType.System);

            ChatDocument document = _repository.CreateDocument();
            document.Tone = string.IsNullOrEmpty(tone) ? _systemMessageUtil.DefaultTone : tone;
            document.Instructions = instructions;
            document.Topic = topic;
            document.AddExchange(systemExchange);
            PopulateMetadata("", document);
            AddOpenDocument(document);

            return document;
        }

        public ChatDocument? FindDocument(Guid identifier)
        {
            var document = _openDocuments.Where(d => d.Metadata.Identifier == identifier).FirstOrDefault();

            return document;
        }

        public ChatDocument? FindDocument(string documentPath)
        {
            var document = _openDocuments.Where(d => d.Metadata.DocumentPath == documentPath).FirstOrDefault();

            return document;
        }

        public ChatDocument? OpenDocument(string documentPath)
        {
            var document = _repository.OpenDocument(documentPath);

            if (document is not null)
            {
                document.MarkAsDirty(false);
                PopulateMetadata(documentPath, document);
                AddOpenDocument(document);
            }

            return document;
        }

        public bool SaveDocument(ChatDocument document, string documentPath)
        {
            bool result = _repository.SaveDocument(document, documentPath);

            document.MarkAsDirty(false);
            document.Metadata.DocumentPath = documentPath;
            document.DocumentFilename = Path.GetFileNameWithoutExtension(documentPath);

            return result;
        }

        public void CloseDocument(ChatDocument document)
        {
            _repository.CloseDocument(document);
            RemoveOpenDocument(document);
        }

        public void AddExchange(ChatDocument document, ChatExchange exchange, int tokens)
        {
            document.AddExchange(exchange);
            document.Metadata.ChatContentBuilder.Append(exchange.Content());
            document.MarkAsDirty(true);
            document.TotalTokens = tokens;
        }

        public string Undo(ChatDocument document)
        {
            string undonePrompt = "";

            if (document.Exchanges.Count < 1)
            {
                return undonePrompt;
            }

            ChatExchange exchange = document.Exchanges.Last();

            if (exchange.Type == ExchangeType.Chat)
            {
                document.Exchanges.RemoveLast();
                document.Metadata.UndoStack.Push(exchange);
                undonePrompt = exchange.Prompt;
                document.Metadata.ChatContentBuilder = document.CreateChatContentBuilder();                
                document.TotalWeight -= exchange.Weight;
                document.MarkAsDirty(true);
                ReloadChatMessages(document);
            }

            return undonePrompt;
        }

        public void Redo(ChatDocument document)
        {
            if (document.Metadata.UndoStack.Count < 1)
            {
                return;
            }

            ChatExchange exchange = document.Metadata.UndoStack.Pop();
            document.Exchanges.AddLast(exchange);
            document.TotalWeight += exchange.Weight;
            document.Metadata.ChatContentBuilder = document.CreateChatContentBuilder();
            document.MarkAsDirty(true);
            ReloadChatMessages(document);
        }

        private static void PopulateMetadata(string documentPath, ChatDocument? document)
        {
            if (document is not null)
            {
                document.Metadata.DocumentPath = documentPath;
                document.Metadata.DocumentFilename = string.IsNullOrEmpty(documentPath) ? document.Metadata.DocumentFilename : Path.GetFileNameWithoutExtension(documentPath);
                document.Metadata.ChatContentBuilder = document.CreateChatContentBuilder();
                ReloadChatMessages(document);
            }
        }

        private static void ReloadChatMessages(ChatDocument document)
        {
            document.Metadata.ChatMessages.Clear();

            foreach (var exchange in document.Exchanges)
            {
                foreach (var message in exchange.ChatMessages())
                {
                    document.Metadata.ChatMessages.Add(message);
                }
            }
        }

        public void UpdateUserSettings(UserSettings userSettings)
        {
            userSettings.LastOpenFiles.Clear();

            foreach (var document in _openDocuments.Where(d => !d.Metadata.IsDirty || d.Metadata.DocumentPath != ""))
            {
                userSettings.LastOpenFiles.Add(document.Metadata.DocumentPath);
            }
        }

        public void OpenDocumentList(List<string> documentPaths)
        {
            foreach (var documentPath in documentPaths)
            {
                OpenDocument(documentPath);
            }
        }

        private void AddOpenDocument(ChatDocument document)
        {
            _openDocuments.Add(document);
            OnOpenDocumentsChanged(new(OpenDocumentsChangedAction.Added, document.Metadata.Identifier));
        }

        private void RemoveOpenDocument(ChatDocument document)
        {
            _openDocuments.Remove(document);
            OnOpenDocumentsChanged(new(OpenDocumentsChangedAction.Removed, document.Metadata.Identifier));
        }

        public void SubscribeToOpenDocumentsChanged(EventHandler<OpenDocumentsChangedEventArgs> handler)
        {
            _openDocumentsChanged += handler;
        }

        public void UnsubscribeFromOpenDocumentsChanged(EventHandler<OpenDocumentsChangedEventArgs> handler)
        {
            _openDocumentsChanged -= handler;
        }

        private void OnOpenDocumentsChanged(OpenDocumentsChangedEventArgs e)
        {
            _openDocumentsChanged?.Invoke(this, e);
        }
    }
}
