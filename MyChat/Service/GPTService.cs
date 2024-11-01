using OpenAI.Chat;
using OpenAI;
using System.Windows;
using System.Text.Json;
using OpenAI.Images;
using System.IO;
using System.Linq.Expressions;
using MyChat.Data;
using MyChat.Common.Model;
using MyChat.Common.Interfaces;

namespace MyChat.Service
{
    public class GPTService : IGPTService
    {
        private const string _keyVarName = "OPENAI_API_KEY";
        private const string _imageClientModel = "dall-e-3";

        private readonly string _openAIKey;
        private readonly OpenAIClient _client;
        private readonly IToolService _toolService;
        private ChatClient _chatClient;
        private readonly ImageClient _imageClient;

        public const string DefaultChatModel = "gpt-4o-mini";
        public string CurrentChatModel { get; private set; }
        public List<string> AvailableModels { get; } = ["gpt-4o", "gpt-4o-mini"];

        public GPTService(ISettingsService settingsService, IToolService toolService)
        {
            var key = Environment.GetEnvironmentVariable(_keyVarName) ?? throw new InvalidOperationException($"Environment variable {_keyVarName} has not been set");

            _openAIKey = key;
            _client = new OpenAIClient(_openAIKey);

            var userSettings = settingsService.GetUserSettings();

            if (userSettings is not null && !string.IsNullOrEmpty(userSettings.SelectedChatModel))
            {
                CurrentChatModel = userSettings.SelectedChatModel;
            }
            else
            {
                CurrentChatModel = DefaultChatModel;
            }

            _chatClient = _client.GetChatClient(CurrentChatModel);
            _imageClient = _client.GetImageClient(_imageClientModel);
            _toolService = toolService;
        }

        public void ChangeChatModel(string newModel)
        {
            if (newModel != CurrentChatModel)
            {
                CurrentChatModel = newModel;
                _chatClient = _client.GetChatClient(CurrentChatModel);
            }
        }

        public async Task<(ChatExchange, int)> SendMessageAsync(List<ChatMessage> chatMessages, string prompt, bool useTools = true)
        {
            ChatCompletionOptions options;

            chatMessages.Add(new UserChatMessage(prompt + (chatMessages.Count == 1 ? " Set the title of this conversation." : "")));

            if (useTools)
            {
                options = new()
                {
                    Tools = { _toolService.GetChatTitleTool, _toolService.SetChatTitleTool, _toolService.StartNewChatTool, 
                        _toolService.SetImageGenerationPromptTool }
                };
            }
            else
            {
                options = new();
            }

            bool requiresAction;
            int totalTokens = 0;
            List<ExchangeToolCallCollection> toolCalls = [];

            ChatExchange? exchange = null;

            do
            {
                requiresAction = false;
                ChatCompletion chatCompletion = await _chatClient.CompleteChatAsync(chatMessages, options);

                switch (chatCompletion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        {
                            string assistantMessage = chatCompletion.ToString();
                            totalTokens = chatCompletion.Usage.TotalTokens;
                            chatMessages.Add(new AssistantChatMessage(assistantMessage));

                            exchange = new(prompt, assistantMessage)
                            {
                                ToolCalls = toolCalls
                            };

                            break;
                        }

                    case ChatFinishReason.ToolCalls:
                        var callCollection = _toolService.ProcessToolCalls(chatCompletion, chatMessages);
                        toolCalls.Add(callCollection);
                        requiresAction = true;
                        break;

                    case ChatFinishReason.Length:
                        throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

                    case ChatFinishReason.ContentFilter:
                        throw new NotImplementedException("Omitted content due to a content filter flag.");

                    case ChatFinishReason.FunctionCall:
                        throw new NotImplementedException("Deprecated in favor of tool calls.");

                    default:
                        throw new NotImplementedException(chatCompletion.FinishReason.ToString());
                }
            }
            while (requiresAction);

            if (exchange is null)
            {
                throw new NullReferenceException("General error recording GPT response");
            }

            return (exchange, totalTokens);
        }

        public async Task<BinaryData?> GenerateImageAsync(string prompt, string quality, string size, string style)
        {
            try
            {
                ImageGenerationOptions options = new()
                {
                    Quality = QualityValue(quality),
                    Size = SizeValue(size),
                    Style = StyleValue(style),
                    ResponseFormat = GeneratedImageFormat.Bytes
                };

                GeneratedImage image = await _imageClient.GenerateImageAsync(prompt, options);
                BinaryData bytes = image.ImageBytes;

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        private static GeneratedImageStyle StyleValue(string style)
        {
            return style switch
            {
                "Natural" => GeneratedImageStyle.Natural,
                "Vivid" => GeneratedImageStyle.Vivid,
                _ => GeneratedImageStyle.Natural,
            };
        }

        private static GeneratedImageSize SizeValue(string size)
        {
            return size switch
            {
                "Standard" => GeneratedImageSize.W1024xH1024,
                "Portrait" => GeneratedImageSize.W1024xH1792,
                "Landscape" => GeneratedImageSize.W1792xH1024,
                _ => GeneratedImageSize.W1024xH1024
            };
        }

        private static GeneratedImageQuality QualityValue(string quality)
        {
            return quality switch
            {
                "Standard" => GeneratedImageQuality.Standard,
                "High" => GeneratedImageQuality.High,
                _ => GeneratedImageQuality.Standard
            };
        }

    }
}
