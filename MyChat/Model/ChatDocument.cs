using OpenAI.Chat;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace MyChat.Model
{
    public class ChatDocument : INotifyPropertyChanged
    {
        private string _documentName = string.Empty;

        public string DocumentName {
            get => _documentName;
            set
            {
                _documentName = value;
                OnPropertyChanged(nameof(DocumentName));
            }
        }

        public LinkedList<ChatExchange> Exchanges { get; set; } = [];
        public int TotalWeight { get; set; }
        public int TotalTokens { get; set; }

        #region Metadata that will not be saved
        [JsonIgnore]
        private string _defaultDocName;
        [JsonIgnore] 
        public Guid Identifier { get; } = Guid.NewGuid();
        private bool _isDirty = false;
        [JsonIgnore] 
        public bool IsDirty
        {
            get => _isDirty;

            set
            {
                _isDirty = value;
                OnPropertyChanged(nameof(FileTitle));
            }
        }
        [JsonIgnore]
        private string _documentPath = string.Empty;
        [JsonIgnore]
        public string DocumentPath
        {
            get => _documentPath;
            set
            {
                _documentPath = value;
                OnPropertyChanged(nameof(Filename));
                OnPropertyChanged(nameof(FileTitle));
            }
        }
        [JsonIgnore] 
        public StringBuilder ChatContentBuilder { get; set; } = new();
        [JsonIgnore] 
        public Stack<ChatExchange> UndoStack { get; } = [];
        [JsonIgnore] 
        public List<ChatMessage> ChatMessages { get; } = [];
        [JsonIgnore]
        public string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(DocumentPath))
                {
                    return _defaultDocName;
                }

                return Path.GetFileNameWithoutExtension(DocumentPath);
            }
        }
        [JsonIgnore]
        public string FileTitle
        {
            get
            {
                return $"{Filename}{(IsDirty ? "*" : "")}";
            }
        }
        [JsonIgnore]
        public string ChatContent
        {
            get => ChatContentBuilder.ToString();
        }
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonConstructor]
        public ChatDocument()
        {
            _defaultDocName = string.Empty;
        }

        public ChatDocument(string defaultFilename)
        {
            _defaultDocName = defaultFilename;
        }

        public void AddExchange(ChatExchange exchange)
        {
            Exchanges.AddLast(exchange);
            TotalWeight += exchange.Weight;
            IsDirty = true;
            UndoStack.Clear();
        }

        public StringBuilder CreateChatContentBuilder()
        {
            StringBuilder content = new();

            foreach (var exchange in Exchanges)
            {
                if (exchange.Type == ExchangeType.Chat)
                {
                    content.Append(exchange.Content());
                }
            }

            return content;
        }
    }
}
