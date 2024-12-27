using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace MyChat.Common.Model
{
    public class ChatDocument : ObservableObject
    {
        private string _documentName = "";

        public string DocumentName
        {
            get => _documentName;

            set => SetProperty(ref _documentName, value);
        }

        public string Instructions { get; set; } = "";
        public string Tone { get; set; } = "";
        public string Topic { get; set; } = "";
        public LinkedList<ChatExchange> Exchanges { get; set; } = [];
        public int TotalWeight { get; set; }
        public int TotalTokens { get; set; }

        [JsonIgnore]
        public ChatDocumentMeta Metadata { get; } = new();

        [JsonConstructor]
        public ChatDocument()
        {
        }

        [JsonIgnore]
        public string ChatContent
        {
            get => Exchanges.Count > 1 ? Metadata.ChatContentBuilder.ToString() : EmptyChatHTML();
        }

        [JsonIgnore]
        public string DocumentFilename
        {
            get => Metadata.DocumentFilename;

            set
            {
                Metadata.DocumentFilename = value;
                OnPropertyChanged(nameof(DocumentFilename));
                OnPropertyChanged(nameof(DocumentFilenameWithState));
            }
        }

        [JsonIgnore]
        public string DocumentFilenameWithState
        {
            get
            {
                string filename = DocumentFilename;
                return $"{filename}{(Metadata.IsDirty ? " *" : "")}";
            }
        }

        public void MarkAsDirty(bool isDirty)
        {
            Metadata.IsDirty = isDirty;
            OnPropertyChanged(nameof(DocumentFilenameWithState));
        }

        public void AddExchange(ChatExchange exchange)
        {
            Exchanges.AddLast(exchange);
            TotalWeight += exchange.Weight;
            MarkAsDirty(true);
            Metadata.UndoStack.Clear();
        }

        public StringBuilder CreateChatContentBuilder(bool isExport = false)
        {
            StringBuilder content = new();

            content.Append(DocumentHeaderHTML(isExport));

            foreach (var exchange in Exchanges)
            {
                if (exchange.Type == ExchangeType.Chat)
                {
                    content.Append(exchange.Content());
                }
            }

            return content;
        }

        private string DocumentHeaderHTML(bool isExport = false)
        {
            bool addHeader = isExport || !string.IsNullOrEmpty(Instructions) || !string.IsNullOrEmpty(Topic);

            StringBuilder builder = new();

            if (addHeader)
            {
                builder.Append("<div style=\"border: 1px solid black; padding: 10px;\"><ul style=\"list-style-type: none; padding: 0; margin: 0;\">");

                if (isExport)
                {
                    builder.Append($"<li><strong>Tone:</strong> {Tone}</li>");
                }

                if (!string.IsNullOrEmpty(Instructions))
                {
                    builder.Append($"<li><strong>Instructions:</strong> {Instructions}</li>");
                }

                if (!string.IsNullOrEmpty(Topic))
                {
                    builder.Append($"<li><strong>Topic:</strong> {Topic}</li>");
                }

                builder.Append("</ul></div><br/>");
            }

            return builder.ToString();
        }

        private string EmptyChatHTML()
        {
            return $"{DocumentHeaderHTML()}{HTMLConstants.StartChatMessage}";
        }
    }
}
