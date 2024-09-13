using MyChat.Model;
using OpenAI.Chat;

namespace MyChat.Service
{
    public interface IGPTService
    {
        string CurrentChatModel { get; }
        List<string> AvailableModels { get; }

        void ChangeChatModel(string newModel);
        Task<(ChatExchange, int)> SendMessageAsync(List<ChatMessage> chatMessages, string prompt);
    }
}