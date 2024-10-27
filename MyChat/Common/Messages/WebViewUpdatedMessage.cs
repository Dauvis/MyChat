using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.Messages
{
    public enum ViewerIdentification
    {
        ChatViewer,
        QnAViewer
    }

    public class WebViewUpdatedMessage
    {
        public WebViewUpdatedMessage(ViewerIdentification viewerId, string chatText, bool isReplacement = false)
        {
            ViewerId = viewerId;
            ChatText = chatText;
            IsReplacement = isReplacement;
        }

        public ViewerIdentification ViewerId { get; set; }
        public bool IsReplacement { get; set; }
        public string ChatText { get; set; }
    }
}
