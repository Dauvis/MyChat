using MyChat.Model;
using OpenAI.Chat;
using System.IO;
using System.Text.Json;

namespace MyChat.Data
{
    public class ChatDocumentRepository : IChatDocumentRepository
    {
        private int _newDocumentCounter = 1;

        public ChatDocument CreateDocument()
        {
            return new($"New Chat {_newDocumentCounter++}");
        }

        public async Task<ChatDocument?> OpenDocumentAsync(string documentPath)
        {
            ChatDocument? document;

            try
            {
                string serialized = await File.ReadAllTextAsync(documentPath);
                document = JsonSerializer.Deserialize<ChatDocument>(serialized);
            }
            catch
            {
                return null;
            }

            return document;
        }

        public async Task<bool> SaveDocumentAsync(ChatDocument document, string documentPath)
        {
            bool ok = true;

            try
            {
                string serialized = JsonSerializer.Serialize(document);
                await File.WriteAllTextAsync(documentPath, serialized);
            }
            catch
            {
                ok = false;
            }

            return ok;
        }

        public void CloseDocument(ChatDocument document)
        {
            // Do nothing, this might be obsolete
        }
    }
}
