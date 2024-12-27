using MyChat.Common.Interfaces;
using MyChat.Common.Model;
using OpenAI.Chat;
using System.IO;
using System.Text.Json;

namespace MyChat.Data
{
    public class ChatDocumentRepository : IChatDocumentRepository
    {
        private static int _newDocumentCounter = 1;

        public ChatDocument CreateDocument()
        {
            ChatDocument document =  new();
            document.Metadata.DocumentFilename = $"New Chat {_newDocumentCounter++}";

            return document;
        }

        public ChatDocument? OpenDocument(string documentPath)
        {
            ChatDocument? document;

            try
            {
                string serialized = File.ReadAllText(documentPath);
                document = JsonSerializer.Deserialize<ChatDocument>(serialized);
            }
            catch
            {
                return null;
            }

            return document;
        }

        public bool SaveDocument(ChatDocument document, string documentPath)
        {
            bool ok = true;

            try
            {
                string serialized = JsonSerializer.Serialize(document);
                File.WriteAllText(documentPath, serialized);
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
