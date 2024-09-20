using OpenAI.Chat;
using OpenAI;
using MyChat.Model;
using System.Windows;
using System.Text.Json;

namespace MyChat.Service
{
    public class GPTService : IGPTService
    {
        private const string _keyVarName = "OPENAI_API_KEY";

        private readonly string _openAIKey;
        private readonly OpenAIClient _client;
        private readonly IToolService _toolService;
        private ChatClient _chatClient;

        public const string DefaultChatModel = "gpt-4o-mini";
        public string CurrentChatModel { get; private set; }
        public List<string> AvailableModels { get; } = ["gpt-4o", "gpt-4o-mini"];

        public GPTService(ISettingsService settingsService, IToolService toolService)
        {
            var key = Environment.GetEnvironmentVariable(_keyVarName) ?? throw new InvalidOperationException($"Environment variable {_keyVarName} has not been set");

            _openAIKey = key;
            _client = new OpenAIClient(_openAIKey);

            var userSettings = settingsService.GetUserSettings();

            if (userSettings is not null && !string.IsNullOrEmpty(userSettings.SelectedChatModel))
            {
                CurrentChatModel = userSettings.SelectedChatModel;
            }
            else
            {
                CurrentChatModel = DefaultChatModel;
            }

            _chatClient = _client.GetChatClient(CurrentChatModel);
            _toolService = toolService;
        }

        public void ChangeChatModel(string newModel)
        {
            if (newModel != CurrentChatModel)
            {
                CurrentChatModel = newModel;
                _chatClient = _client.GetChatClient(CurrentChatModel);
            }
        }

        public async Task<(ChatExchange, int)> SendMessageAsync(List<ChatMessage> chatMessages, string prompt)
        {
            chatMessages.Add(new UserChatMessage(prompt + (chatMessages.Count == 1 ? " Set the title of this conversation." : "")));

            ChatCompletionOptions options = new()
            {
                Tools = { _toolService.GetChatTitleTool, _toolService.SetChatTitleTool, _toolService.StartNewChatTool }
            };

            bool requiresAction;
            int totalTokens = 0;
            List<ExchangeToolCallCollection> toolCalls = [];

            ChatExchange? exchange = null;

            do
            {
                requiresAction = false;
                ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync(chatMessages, options);

                switch (chatCompletion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            string assistantMessage = chatCompletion.ToString();
                            totalTokens = chatCompletion.Usage.TotalTokens;
                            chatMessages.Add(new AssistantChatMessage(assistantMessage));

                            exchange = new(prompt, assistantMessage)
                            {
                                ToolCalls = toolCalls
                            };

                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        var callCollection = _toolService.ProcessToolCalls(chatCompletion, chatMessages);
                        toolCalls.Add(callCollection);
                        requiresAction = true;
                        break;

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(chatCompletion.FinishReason.ToString());
                }
            }
            while (requiresAction);

            if (exchange is null)
            {
                throw new NullReferenceException("General error recording GPT response");
            }

            return (exchange, totalTokens);
        }
    }
}
