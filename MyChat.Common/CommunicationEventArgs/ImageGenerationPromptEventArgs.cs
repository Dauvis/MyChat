﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyChat.Common.CommunicationEventArgs
{
    public class ImageGenerationPromptEventArgs : EventArgs
    {
        public string Prompt { get; set; }

        public ImageGenerationPromptEventArgs(string prompt)
        {
            Prompt = prompt;
        }
    }
}
