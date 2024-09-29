﻿using MyChat.Model;
using MyChat.Util;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyChat.Service
{
    public class ToolService : IToolService
    {
        private static readonly ChatTool _getChatTitleTool = ChatTool.CreateFunctionTool(
            functionName: nameof(GetChatTitle),
            functionDescription: "Returns the short description of a chat");

        private static readonly ChatTool _setChatTitleTool = ChatTool.CreateFunctionTool(
            functionName: nameof(SetChatTitle),
            functionDescription: "Set the short description of a chat",
            functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "title": {
                            "type": "string",
                            "description": "The short description of the chat"
                        }
                    },
                    "required": ["title"]
                }
                """));

        private static readonly ChatTool _startNewChatTool = ChatTool.CreateFunctionTool(
            functionName: nameof(StartNewChat),
            functionDescription: "Creates a new chat and changes focus to it. If tone and custom instructions are not supplied, the new chat will inherit from the original.",
            functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "summary": { "type": "string", "description": "Optional summarization of original chat" },
                        "title": { "type": "string", "description": "Optional title for the new chat" },
                        "tone": { "type": "string", "description": "Optional tone for the new chat. Acceptable values are Helpful, Friendly, Professional, Enthusiastic, Empathetic, Informative, Casual, Concise, Encouraging, Playful, Inquisitive, Mentor, Technical"  },
                        "instructions": { "type": "string", "description": "Optional custom instructions for the new chat" }
                    },
                    "required": []
                }
                """));

        private static readonly ChatTool _setImageGenerationPromptTool = ChatTool.CreateFunctionTool(
            functionName: nameof(SetImageGenerationPrompt),
            functionDescription: "Opens image generation tool and sets the prompt. This will not generate an image.",
            functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "prompt": { "type": "string", "description": "The prompt for generating an image" }
                    },
                    "required": ["prompt"]
                }
                """));

        public event EventHandler<ChatTitleEventArgs>? ChatTitleEvent;
        public event EventHandler<NewChatEventArgs>? StartNewChatEvent;
        private static EventHandler<ImageGenerationPromptEventArgs>? _imageGenerationPromptEvent;
        private static EventHandler<OpenImageToolEventArgs>? _openImageToolEvent;

        public ChatTool GetChatTitleTool
        {
            get => _getChatTitleTool;
        }

        public ChatTool SetChatTitleTool
        {
            get => _setChatTitleTool;
        }

        public ChatTool StartNewChatTool
        {
            get => _startNewChatTool;
        }

        public ChatTool SetImageGenerationPromptTool
        {
            get => _setImageGenerationPromptTool;
        }

        public string GetChatTitle()
        {
            ChatTitleEventArgs args = new(false, "");
            OnChatTitleEvent(args);

            return args.Title;
        }

        public void SetChatTitle(string title)
        {
            ChatTitleEventArgs args = new(true, title);
            OnChatTitleEvent(args);
        }

        public void StartNewChat(string summary = "", string title = "", string tone = "", string customInstructions = "")
        {
            NewChatEventArgs args = new(summary, title, tone, customInstructions);
            OnStartNewChatEvent(args);
        }

        public void SetImageGenerationPrompt(string prompt)
        {
            ImageGenerationPromptEventArgs args = new(prompt);
            OnSetImageGenerationPromptEvent(args);
        }

        public ExchangeToolCallCollection ProcessToolCalls(ChatCompletion chatCompletion, List<ChatMessage> chatMessages)
        {
            chatMessages.Add(new AssistantChatMessage(chatCompletion));
            ExchangeToolCallCollection toolCalls = new(chatCompletion.ToString());

            foreach (ChatToolCall toolCall in chatCompletion.ToolCalls)
            {
                switch (toolCall.FunctionName)
                {
                    case nameof(GetChatTitle):
                        {
                            string toolResult = GetChatTitle();
                            toolCalls.ToolCalls.Add(new(toolCall.Id, toolCall.FunctionArguments, toolCall.FunctionName, toolResult));
                            chatMessages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                            break;
                        }

                    case nameof(SetChatTitle):
                        {
                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                            bool hasTitle = argumentsJson.RootElement.TryGetProperty("title", out JsonElement jsonPrompt);
                            string? chatTitle = hasTitle ? jsonPrompt.GetString() : null;

                            if (!hasTitle || chatTitle is null)
                            {
                                throw new NullReferenceException($"Title argument for {nameof(SetChatTitle)} was not provided");
                            }

                            SetChatTitle(chatTitle);
                            toolCalls.ToolCalls.Add(new(toolCall.Id, toolCall.FunctionArguments, toolCall.FunctionName, chatTitle));
                            chatMessages.Add(new ToolChatMessage(toolCall.Id, chatTitle));
                            break;
                        }

                    case nameof(StartNewChat):
                        {
                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                            bool hasSummary = argumentsJson.RootElement.TryGetProperty("summary", out JsonElement jsonSummary);
                            string? summary = hasSummary ? jsonSummary.GetString() : "";
                            bool hasTitle = argumentsJson.RootElement.TryGetProperty("title", out JsonElement jsonTitle);
                            string? title = hasTitle ? jsonTitle.GetString() : "";
                            bool hasTone = argumentsJson.RootElement.TryGetProperty("tone", out JsonElement jsonTone);
                            string? tone = hasTone ? jsonTone.GetString() : "";
                            bool hasInstructions = argumentsJson.RootElement.TryGetProperty("instructions", out JsonElement jsonInstructions);
                            string? instructions = hasInstructions ? jsonInstructions.GetString() : "";

                            StartNewChat(string.IsNullOrEmpty(summary) ?  "" : summary,
                                string.IsNullOrEmpty(title) ? "" : title,
                                string.IsNullOrEmpty(tone) ? "" : tone,
                                string.IsNullOrEmpty(instructions) ? "" : instructions);

                            toolCalls.ToolCalls.Add(new(toolCall.Id, toolCall.FunctionArguments, toolCall.FunctionName, string.IsNullOrEmpty(title) ? "" : title));
                            chatMessages.Add(new ToolChatMessage(toolCall.Id, string.IsNullOrEmpty(title) ? "" : title));
                            break;
                        }

                    case nameof(SetImageGenerationPrompt):
                        {
                            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
                            bool hasPrompt = argumentsJson.RootElement.TryGetProperty("prompt", out JsonElement jsonPrompt);
                            string? imagePrompt = hasPrompt ? jsonPrompt.GetString() : null;

                            if (!hasPrompt || imagePrompt is null)
                            {
                                throw new NullReferenceException($"Prompt argument for {nameof(SetImageGenerationPrompt)} was not provided");
                            }

                            SetImageGenerationPrompt(imagePrompt);
                            toolCalls.ToolCalls.Add(new(toolCall.Id, toolCall.FunctionArguments, toolCall.FunctionName, imagePrompt));
                            chatMessages.Add(new ToolChatMessage(toolCall.Id, imagePrompt));
                            break;
                        }

                    default:
                        {
                            throw new NotImplementedException($"Tool call {toolCall.FunctionName} is not currently supported");
                        }
                }
            }

            return toolCalls;
        }

        public void SubscribeToSetImageGenerationPrompt(EventHandler<ImageGenerationPromptEventArgs> handler)
        {
            _imageGenerationPromptEvent += handler;
        }

        public void UnsubscribeFromSetImageGenerationPrompt(EventHandler<ImageGenerationPromptEventArgs> handler)
        {
            _imageGenerationPromptEvent -= handler;
        }

        public void SubscribeToOpenImageTool(EventHandler<OpenImageToolEventArgs> handler)
        {
            _openImageToolEvent += handler;
        }

        public void UnsubscribeFromOpenImageTool(EventHandler<OpenImageToolEventArgs> handler)
        {
            _openImageToolEvent -= handler;
        }

        private void OnChatTitleEvent(ChatTitleEventArgs e)
        {
            ChatTitleEvent?.Invoke(this, e);
        }

        private void OnStartNewChatEvent(NewChatEventArgs e)
        {
            StartNewChatEvent?.Invoke(this, e);
        }

        private void OnSetImageGenerationPromptEvent(ImageGenerationPromptEventArgs e)
        {
            _imageGenerationPromptEvent?.Invoke(this, e);

            if (!e.Handled)
            {
                _openImageToolEvent?.Invoke(this, new());
                _imageGenerationPromptEvent?.Invoke(this, e);
            }
        }
    }
}
