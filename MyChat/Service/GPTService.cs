using OpenAI.Chat;
using OpenAI;
using MyChat.Model;

namespace MyChat.Service
{
    public class GPTService : IGPTService
    {
        private const string _keyVarName = "OPENAI_API_KEY";
        private const string _chatClientModel = "gpt-4o-mini";

        private readonly string _openAIKey;
        private readonly OpenAIClient _client;
        private readonly ChatClient _chatClient;

        public GPTService()
        {
            var key = Environment.GetEnvironmentVariable(_keyVarName) ?? throw new InvalidOperationException($"Environment variable {_keyVarName} has not been set");

            _openAIKey = key;
            _client = new OpenAIClient(_openAIKey);
            _chatClient = _client.GetChatClient(_chatClientModel);
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
