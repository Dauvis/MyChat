using MyChat.Data;
using MyChat.Model;
using MyChat.Util;

namespace MyChat.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly List<ChatDocument> _openDocuments = [];
        private readonly IChatDocumentRepository _repository;

        public event EventHandler<OpenDocumentsChangedEventArgs>? OpenDocumentsChanged;

        public IEnumerable<ChatDocument> OpenDocuments => _openDocuments.AsEnumerable();

        public DocumentService(IChatDocumentRepository repository)
        {
            _repository = repository;
        }

        public ChatDocument CreateDocument(string additionalInstructions)
        {
            string systemPrompt = SystemPrompts.DefaultSystemPrompt + " " + additionalInstructions;
            ChatExchange systemExchange = new(systemPrompt, string.Empty, ExchangeType.System);

            ChatDocument document = _repository.CreateDocument();
            document.AddExchange(systemExchange);
            PopulateMetadata("", document);
            AddOpenDocument(document);

            return document;
        }

        public ChatDocument? FindDocument(Guid identifier)
        {
            var document = _openDocuments.Where(d => d.Identifier == identifier).FirstOrDefault();

            return document;
        }

        public ChatDocument? FindDocument(string documentPath)
        {
            var document = _openDocuments.Where(d => d.DocumentPath == documentPath).FirstOrDefault();

            return document;
        }

        public async Task<ChatDocument?> OpenDocumentAsync(string documentPath)
        {
            var document = await _repository.OpenDocumentAsync(documentPath);

            if (document is not null)
            {
                PopulateMetadata(documentPath, document);
                AddOpenDocument(document);
            }

            return document;
        }

        public async Task<bool> SaveDocumentAsync(ChatDocument document, string documentPath)
        {
            bool result = await _repository.SaveDocumentAsync(document, documentPath);

            document.IsDirty = false;
            document.DocumentPath = documentPath;

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
            document.ChatContentBuilder.Append(exchange.Content());
            document.TotalTokens = tokens;
            document.IsDirty = true;
        }

        public string Undo(ChatDocument document)
        {
            string undonePrompt = string.Empty;

            if (document.Exchanges.Count < 1)
            {
                return undonePrompt;
            }

            ChatExchange exchange = document.Exchanges.Last();

            if (exchange.Type == ExchangeType.Chat)
            {
                document.Exchanges.RemoveLast();
                document.UndoStack.Push(exchange);
                undonePrompt = exchange.Prompt;
                document.ChatContentBuilder = document.CreateChatContentBuilder();                
                document.TotalWeight -= exchange.Weight;
                document.IsDirty = true;
                ReloadChatMessages(document);
            }

            return undonePrompt;
        }

        public void Redo(ChatDocument document)
        {
            if (document.UndoStack.Count < 1)
            {
                return;
            }

            ChatExchange exchange = document.UndoStack.Pop();
            document.Exchanges.AddLast(exchange);
            document.TotalWeight += exchange.Weight;
            document.ChatContentBuilder = document.CreateChatContentBuilder();
            document.IsDirty = true;
            ReloadChatMessages(document);
        }

        private static void PopulateMetadata(string documentPath, ChatDocument? document)
        {
            if (document is not null)
            {
                document.DocumentPath = documentPath;
                document.ChatContentBuilder = document.CreateChatContentBuilder();
                ReloadChatMessages(document);
            }
        }

        private static void ReloadChatMessages(ChatDocument document)
        {
            document.ChatMessages.Clear();

            foreach (var exchange in document.Exchanges)
            {
                foreach (var message in exchange.ChatMessages())
                {
                    document.ChatMessages.Add(message);
                }
            }
        }

        private void AddOpenDocument(ChatDocument document)
        {
            _openDocuments.Add(document);
            OnOpenDocumentsChanged(new(OpenDocumentsChangedAction.Added, document.Identifier));
        }

        private void RemoveOpenDocument(ChatDocument document)
        {
            _openDocuments.Remove(document);
            OnOpenDocumentsChanged(new(OpenDocumentsChangedAction.Removed, document.Identifier));
        }

        private void OnOpenDocumentsChanged(OpenDocumentsChangedEventArgs e)
        {
            OpenDocumentsChanged?.Invoke(this, e);
        }
    }
}
