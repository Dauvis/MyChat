﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common
{
    public static class HTMLConstants
    {
        public const string DocumentTemplate =
            "<!DOCTYPE html><html><head><link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/default.min.css\">" +
            "<style>.user-msg-box {{background-color: lightgray;border: 1px solid black;border-radius: 8px;" +
            "padding: 0 10px;}}</style></head><body>{0}<script src=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.6.0/highlight.min.js\"></script>" +
            "<script>hljs.highlightAll();</script></body></html>";

        public const string ExportDocumentTemplate = "<!DOCTYPE html><html><head>" +
            "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/default.min.css\">" +
            "<style>body {{font-family: Arial, sans-serif;background-color: #f8f8f8;margin: 0;" +
            "padding: 20px;}}.container {{max-width: 800px;margin: 0 auto;background-color: white;padding: 20px;border-radius: 8px;" +
            "box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);}}.user-msg-box {{background-color: lightgray;border: 1px solid black;border-radius: 8px;" +
            "padding: 0 10px;}}</style></head><body><div class='container'>{0}</div><script src=\"https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.6.0/highlight.min.js\">" +
            "</script><script>hljs.highlightAll();</script></body></html>";

        public const string StartChatMessage = "<p><em>Enter a message to start a chat</em></p>";
    }
}
