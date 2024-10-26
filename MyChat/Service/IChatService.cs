using MyChat.Model;
using OpenAI.Chat;

namespace MyChat.Service
{
    public interface IChatService
    {
        Task<string?> SendPromptAsync(ChatDocument document, string prompt);
        Task<bool> ExportAsHTMLAsync(ChatDocument document, string filename);
        Task<ChatExchange?> SendMessagesAsync(List<ChatMessage> messages, string prompt);
    }
}