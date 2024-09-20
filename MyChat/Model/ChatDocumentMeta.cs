﻿using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Model
{
    public class ChatDocumentMeta
    {
        public Guid Identifier { get; } = Guid.NewGuid();
        public bool IsDirty { get; set; } = true;
        public string DocumentFilename { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public string CurrentPrompt { get; set; } = string.Empty;
        public StringBuilder ChatContentBuilder { get; set; } = new();
        public Stack<ChatExchange> UndoStack { get; } = [];
        public List<ChatMessage> ChatMessages { get; } = [];
    }
}