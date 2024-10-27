using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.Common.Model;
using MyChat.Common.Util;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public class QuestionAnswerViewModel : ObservableObject
    {
        private string _prompt = "";
        private Cursor? _currentCursor = null;
        private Visibility _processOverlayVisibility = Visibility.Collapsed;
        private Visibility _chatViewerVisibility = Visibility.Visible;
        private ChatExchange? _lastExchange = null;

        private readonly IChatService _chatService;
        private readonly SystemMessageUtil _systemMessageUtil;
        private readonly IDocumentService _documentService;
        private readonly ISettingsService _settingService;

        public ICommand NewChatButtonCommand { get; }
        public ICommand SendButtonCommand { get; }

        public QuestionAnswerViewModel(IChatService chatService, SystemMessageUtil systemMessageUtil, 
            IDocumentService documentService, ISettingsService settingService)
        {
            NewChatButtonCommand = new RelayCommand(OnNewChatButtonClicked);
            SendButtonCommand = new AsyncRelayCommand(OnSendButtonClicked);
            _chatService = chatService;
            _systemMessageUtil = systemMessageUtil;
            _documentService = documentService;
            _settingService = settingService;
        }

        public string Prompt
        {
            get => _prompt;
            set => SetProperty(ref _prompt, value);
        }

        public Cursor? CurrentCursorState
        {
            get => _currentCursor;
            set => SetProperty(ref _currentCursor, value);
        }

        public Visibility ProcessOverlayVisibility
        {
            get => _processOverlayVisibility;
            set => SetProperty(ref _processOverlayVisibility, value);
        }

        public Visibility ChatViewerVisibility
        {
            get => _chatViewerVisibility;
            set => SetProperty(ref _chatViewerVisibility, value);
        }

        private void OnNewChatButtonClicked()
        {
            if (_lastExchange is not null)
            {
                var settings = _settingService.GetUserSettings();
                var document = _documentService.CreateDocument(settings.DefaultTone, settings.DefaultInstructions);
                document.Metadata.ChatMessages.Add(new UserChatMessage(_lastExchange.Prompt));
                document.Metadata.ChatMessages.Add(new AssistantChatMessage(_lastExchange.Response));
                _documentService.AddExchange(document, _lastExchange, 0);
                WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.ChatViewer, document.ChatContent));
                WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Refresh, WindowType.Main));
                WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Focus, WindowType.Main));
            }
        }

        private void SetWindowWaiting(bool isWaiting)
        {
            if (isWaiting)
            {
                CurrentCursorState = Cursors.Wait;
                ProcessOverlayVisibility = Visibility.Visible;
                ChatViewerVisibility = Visibility.Collapsed;
            }
            else
            {
                CurrentCursorState = null;
                ProcessOverlayVisibility = Visibility.Collapsed;
                ChatViewerVisibility = Visibility.Visible;
            }
        }

        private async Task OnSendButtonClicked()
        {
            SetWindowWaiting(true);

            _lastExchange = await _chatService.SendMessagesAsync([new SystemChatMessage(_systemMessageUtil.QnASystemMessage)], _prompt);

            if (_lastExchange is not null)
            {
                string htmlText = _lastExchange.Content();
                WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.QnAViewer, htmlText));
                Prompt = "";
            }

            SetWindowWaiting(false);
        }
    }
}
