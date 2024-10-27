using MyChat.Common;
using MyChat.Common.Interfaces;
using MyChat.Common.Model;
using MyChat.Data;
using OpenAI.Chat;
using System.IO;

namespace MyChat.Service
{
    public class ChatService : IChatService
    {
        private readonly IGPTService _gptService;
        private readonly IDocumentService _documentService;

        public ChatService(IGPTService gptService, IDocumentService documentService)
        {
            _gptService = gptService;
            _documentService = documentService;
        }

        public async Task<string?> SendPromptAsync(ChatDocument document, string prompt)
        {
            try
            {
                if (string.IsNullOrEmpty(prompt))
                {
                    return null;
                }

                (var exchange, int tokens) = await _gptService.SendMessageAsync(document.Metadata.ChatMessages, prompt);
                _documentService.AddExchange(document, exchange, tokens);
            }
            catch (Exception ex)
            {
                return $"GPT communication error: {ex.Message}";
            }

            return null;
        }

        public async Task<ChatExchange?> SendMessagesAsync(List<ChatMessage> messages, string prompt)
        {
            try
            {
                if (string.IsNullOrEmpty(prompt))
                {
                    return null;
                }

                (var exchange, int tokens) = await _gptService.SendMessageAsync(messages, prompt, false);
                return exchange;
            }
            catch (Exception e)
            {
                return new ChatExchange(prompt, $"# Error\n{e.Message}");
            }
        }

        public async Task<bool> ExportAsHTMLAsync(ChatDocument document, string filename)
        {
            bool ok = true;

            try
            {
                string htmlDocument = string.Format(HTMLConstants.ExportDocumentTemplate, document.CreateChatContentBuilder(true));
                await File.WriteAllTextAsync(filename, htmlDocument);
            }
            catch
            {
                ok = false;
            }

            return ok;

        }


    }
}
