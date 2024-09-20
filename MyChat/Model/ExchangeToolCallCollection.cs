using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Model
{
    public class ExchangeToolCallCollection
    {
        public ExchangeToolCallCollection(string assistantMessage)
        {
            AssistantMessage = assistantMessage;
        }

        public string AssistantMessage { get; set; }
        public List<ExchangeToolCall> ToolCalls { get; set; } = [];
    }
}
