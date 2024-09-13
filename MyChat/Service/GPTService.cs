using OpenAI.Chat;
using OpenAI;
using MyChat.Model;
using System.Windows;

namespace MyChat.Service
{
    public class GPTService : IGPTService
    {
        private const string _keyVarName = "OPENAI_API_KEY";

        private readonly string _openAIKey;
        private readonly OpenAIClient _client;
        private ChatClient _chatClient;

        public const string DefaultChatModel = "gpt-4o-mini";
        public string CurrentChatModel { get; private set; }
        public List<string> AvailableModels { get; } = ["gpt-4o", "gpt-4o-mini"];

        public GPTService(ISettingsService settingsService)
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
            chatMessages.Add(new UserChatMessage(prompt));

            bool requiresAction;
            int totalTokens;
            ChatExchange? exchange;
            do
            {
                requiresAction = false;
                ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync(chatMessages);

                switch (chatCompletion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            // Add the assistant message to the conversation history.
                            string assistantMessage = chatCompletion.ToString();
                            totalTokens = chatCompletion.Usage.TotalTokens;
                            exchange = new(prompt, assistantMessage);
                            chatMessages.Add(new AssistantChatMessage(assistantMessage));
                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        throw new NotImplementedException("Tool calls not yet supported");

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

            return (exchange, totalTokens);
        }
    }
}
