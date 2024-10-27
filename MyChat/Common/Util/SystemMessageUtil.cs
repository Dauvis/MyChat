using System.IO;
using System.Text;
using System.Text.Json;

namespace MyChat.Common.Util
{
    public class SystemMessageUtil
    {
        public string Header { get; set; } = "";
        public string Footer { get; set; } = "";
        public string ToneOpening { get; set; } = "";
        public string ToneClosing { get; set; } = "";
        public string DefaultTone { get; set; } = "";
        public Dictionary<string, string> ToneInstructions { get; set; } = [];
        public string ToolOpening { get; set; } = "";
        public string ToolClosing { get; set; } = "";
        public Dictionary<string, string> ToolInstructions { get; set; } = [];
        public string QnASystemMessage { get; set; } = "";

        public static SystemMessageUtil Create()
        {
            string utility = File.ReadAllText("SystemMessageData.json");
            return JsonSerializer.Deserialize<SystemMessageUtil>(utility) ?? new SystemMessageUtil();
        }

        public List<string> AvailableTones()
        {
            List<string> tones = [];

            foreach (string name in ToneInstructions.Keys)
            {
                tones.Add(name);
            }

            tones.Sort();

            return tones;
        }

        public string ChatSystemMessage(string tone, string instructions, string topic)
        {
            StringBuilder message = new();

            bool found = ToneInstructions.TryGetValue(tone, out string? toneInstructions);

            if (!found || string.IsNullOrEmpty(toneInstructions))
            {
                toneInstructions = ToneInstructions[DefaultTone];
            }

            string topicInstructionsOpening = "";

            if (!string.IsNullOrEmpty(topic))
            {
                topicInstructionsOpening = string.IsNullOrEmpty(instructions) ? "The user supplied the following topic." : "The user supplied the following topic and instructions.";
            }
            else if (!string.IsNullOrEmpty(instructions))
            {
                topicInstructionsOpening = "The user supplied the following instructions.";
            }

            message.AppendLine(Header);
            message.AppendLine($"{ToneOpening} {toneInstructions} {ToneClosing}");
            message.AppendLine(topicInstructionsOpening);

            if (!string.IsNullOrEmpty(topic))
            {
                message.AppendLine($"Topic: {topic}");
            }

            if (!string.IsNullOrEmpty(instructions))
            {
                message.AppendLine($"Instructions: {instructions}");
            }

            message.AppendLine(ToolOpening);

            foreach (var toolEntry in ToolInstructions)
            {
                string toolName = toolEntry.Key;
                string toolInstruction = toolEntry.Value;

                message.AppendLine($"* {toolName}: {toolInstruction}");
            }

            message.AppendLine(ToolClosing);

            message.AppendLine(Footer);

            return message.ToString();
        }
    }
}
