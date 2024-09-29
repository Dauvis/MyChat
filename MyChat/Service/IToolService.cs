using MyChat.Model;
using MyChat.Util;
using OpenAI.Chat;

namespace MyChat.Service
{
    public interface IToolService
    {
        ChatTool GetChatTitleTool { get; }
        ChatTool SetChatTitleTool { get; }
        ChatTool StartNewChatTool { get; }
        ChatTool SetImageGenerationPromptTool { get; }

        event EventHandler<ChatTitleEventArgs>? ChatTitleEvent;
        event EventHandler<NewChatEventArgs>? StartNewChatEvent;

        string GetChatTitle();
        ExchangeToolCallCollection ProcessToolCalls(ChatCompletion chatCompletion, List<ChatMessage> chatMessages);
        void SetChatTitle(string title);
        void StartNewChat(string summary = "", string title = "", string tone = "", string customInstructions = "");
        void SubscribeToOpenImageTool(EventHandler<OpenImageToolEventArgs> handler);
        void SubscribeToSetImageGenerationPrompt(EventHandler<ImageGenerationPromptEventArgs> handler);
        void UnsubscribeFromOpenImageTool(EventHandler<OpenImageToolEventArgs> handler);
        void UnsubscribeFromSetImageGenerationPrompt(EventHandler<ImageGenerationPromptEventArgs> handler);
    }
}