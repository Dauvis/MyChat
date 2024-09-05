﻿using MyChat.Data;
using MyChat.Model;
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

                (var exchange, int tokens) = await _gptService.SendMessageAsync(document.ChatMessages, prompt);
                _documentService.AddExchange(document, exchange, tokens);
            }
            catch (Exception ex)
            {
                return $"GPT communication error: {ex.Message}";
            }

            return null;
        }

        public async Task<bool> ExportAsHTMLAsync(ChatDocument document, string filename)
        {
            bool ok = true;

            try
            {
                string htmlDocument = $"<!DOCTYPE html><html><head></head><body>{document.CreateChatContentBuilder()}</body></html>";
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