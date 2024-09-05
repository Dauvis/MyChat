using MyChat.Model;
using OpenAI.Chat;

namespace MyChat.Service
{
    public interface IGPTService
    {
        Task<(ChatExchange, int)> SendMessageAsync(List<ChatMessage> chatMessages, string prompt);
    }
}