using MyChat.Model;
using OpenAI.Chat;

namespace MyChat.Service
{
    public interface IGPTService
    {
        string CurrentChatModel { get; }
        List<string> AvailableModels { get; }

        void ChangeChatModel(string newModel);
        Task<BinaryData?> GenerateImageAsync(string prompt, string quality, string size, string style);
        Task<(ChatExchange, int)> SendMessageAsync(List<ChatMessage> chatMessages, string prompt, bool useTools = true);
    }
}