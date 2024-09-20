using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Model
{
    public class ExchangeToolCall
    {
        public ExchangeToolCall(string toolCallId, string callArgs, string callFunction, string callResult)
        {
            ToolCallId = toolCallId;
            CallArgs = callArgs;
            CallFunction = callFunction;
            CallResult = callResult;
        }

        public string ToolCallId { get; set;  }
        public string CallArgs { get; set; }
        public string CallFunction { get; set; }
        public string CallResult { get; set; }
    }
}
