using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.Model
{
    public class ImageToolMetadata
    {
        public string Quality { get; set; }
        public string Size { get; set; }
        public string Style { get; set; }
        public string Prompt { get; set; }

        public ImageToolMetadata(string prompt, string quality, string size, string style)
        {
            Prompt = prompt;
            Quality = quality;
            Size = size;
            Style = style;
        }
    }
}
