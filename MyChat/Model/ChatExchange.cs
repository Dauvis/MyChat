using Markdig;
using OpenAI.Chat;
using System.Web;

namespace MyChat.Model
{
    public enum ExchangeType
    {
        System = 0,
        Chat = 1,
        Silent = 2
    }

    public class ChatExchange
    {
        public ChatExchange(string prompt, string response, ExchangeType type = ExchangeType.Chat)
        {
            _prompt = prompt;
            _response = response;
            Weight = ExchangeWeight();
            Type = type;
        }

        private string _prompt;
        private string _response;

        public string Prompt
        {
            get
            {
                return _prompt;
            }

            set
            {
                _prompt = value;
                Weight = ExchangeWeight();
            }
        }

        public string Response
        {
            get
            {
                return _response;
            }

            set
            {
                _response = value;
                Weight = ExchangeWeight();
            }
        }

        public bool HasToolCalls
        {
            get => ToolCalls.Count > 0;
        }

        public List<ExchangeToolCallCollection> ToolCalls { get; set; } = [];

        public ExchangeType Type { get; set; }
        public int Weight { get; set; }

        public int ExchangeWeight()
        {
            static int StringWeight(string value)
            {
                int whiteAlphaNumeric = 0;
                int other = 0;

                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];

                    if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    {
                        whiteAlphaNumeric++;
                    }
                    else
                    {
                        other++;
                    }
                }

                return whiteAlphaNumeric / 4 + other;
            }

            return StringWeight(Prompt) + StringWeight(Response);
        }

        public string Content()
        {
            if (Type == ExchangeType.Chat)
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

                string promptPart = Markdown.ToHtml("**User:** " + Prompt, pipeline);
                string responsePart = Markdown.ToHtml("**Assistant:** " + Response, pipeline);

                return $"<div class='user-msg-box'>{promptPart}</div>{responsePart}";
            }

            return "";
        }

        public List<ChatMessage> ChatMessages()
        {
            if (Type == ExchangeType.System)
            {
                return [new SystemChatMessage(Prompt)];
            }
            else if (Type == ExchangeType.Chat)
            {
                if (HasToolCalls)
                {
                    List<ChatMessage> callMessages = [new UserChatMessage(Prompt)];
                    callMessages = ToolCallMessages();
                    callMessages.Add(new AssistantChatMessage(Response));

                    return callMessages;
                }
                else
                {
                    return [new UserChatMessage(Prompt), new AssistantChatMessage(Response)];
                }
            }
            else if (Type == ExchangeType.Silent)
            {
                List<ChatMessage> callMessages = [new UserChatMessage(Prompt)];
                callMessages = ToolCallMessages();

                return callMessages;
            }
            else
            {
                return [];
            }
        }

        private List<ChatMessage> ToolCallMessages()
        {
            List<ChatMessage> messages = [];

            foreach (var collection in ToolCalls)
            {
                var assistantMessage = new AssistantChatMessage(collection.AssistantMessage);
                messages.Add(assistantMessage);

                foreach (var toolCall in collection.ToolCalls)
                {
                    messages.Add(new ToolChatMessage(toolCall.ToolCallId, toolCall.CallResult));

                    var call = ChatToolCall.CreateFunctionToolCall(toolCall.ToolCallId, toolCall.CallFunction, toolCall.CallArgs);
                    assistantMessage.ToolCalls.Add(call);
                }
            }

            return messages;
        }
    }
}
