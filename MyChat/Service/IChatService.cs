using MyChat.Model;

namespace MyChat.Service
{
    public interface IChatService
    {
        Task<string?> SendPromptAsync(ChatDocument document, string prompt);
        Task<bool> ExportAsHTMLAsync(ChatDocument document, string filename);
    }
}