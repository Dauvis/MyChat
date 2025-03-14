﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MyChat.Common.CommunicationEventArgs;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.Common.Model;
using MyChat.Common.Util;
using MyChat.Util;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyChat.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly Guid _newChatTemplateGuid = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
        private readonly Guid _openTemplateWindowGuid = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);

        private ChatDocument? _currentDocument;
        public ObservableCollection<ChatDocument> OpenDocuments { get; set; } = [];
        public ObservableCollection<TemplateMRUEntry> TemplateMRU { get; set; } = [];

        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;
        private readonly IDialogUtil _dialogUtil;
        private readonly ISettingsService _settingsService;
        private readonly IToolService _toolService;
        private readonly IServiceProvider _services;
        private readonly SystemMessageUtil _systemMessageUtil;
        private readonly IChatTemplateRepository _templateRepository;
        private string _prompt = "";
        private Cursor? _currentCursor = null;
        private Visibility _processOverlayVisibility = Visibility.Collapsed;
        private Visibility _chatViewerVisibility = Visibility.Visible;
        private ImageToolWindow? _imageToolWindow = null;
        private ChatTemplatesWindow? _chatTemplatesWindow = null;
        private QuestionAnswerWindow? _questionAnswerWindow = null;

        public ICommand SendPromptCommand { get; }
        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveDocumentAsCommand { get; }
        public ICommand SaveAllDocumentCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand ExportAsHTMLCommand { get; }
        public ICommand OpenSettingsDialogCommand { get; }
        public ICommand OpenImageToolCommand { get; }
        public ICommand OpenChatTemplatesCommand { get; }
        public ICommand QnAButtonCommand { get; }

        public MainViewModel(IChatService chatService, IDocumentService documentService, IDialogUtil dialogUtil, 
            ISettingsService settingsService, IToolService toolService, IServiceProvider services,
            SystemMessageUtil systemMessageUtil, IChatTemplateRepository templateRepository)
        {
            SendPromptCommand = new AsyncRelayCommand(OnSendPromptAsync);
            NewDocumentCommand = new RelayCommand<TemplateMRUEntry>(OnNewDocument);
            OpenDocumentCommand = new RelayCommand(OnOpenDocument);
            CloseDocumentCommand = new RelayCommand<object>(OnCloseDocument);
            SaveDocumentCommand = new RelayCommand(OnSaveDocument);
            SaveDocumentAsCommand = new RelayCommand(OnSaveDocumentAs);
            SaveAllDocumentCommand = new RelayCommand(OnSaveAllDocument);
            UndoCommand = new RelayCommand(OnUndo);
            RedoCommand = new RelayCommand(OnRedo);
            ExportAsHTMLCommand = new AsyncRelayCommand(OnExportAsHTMLAsync);
            OpenSettingsDialogCommand = new RelayCommand(OnOpenSettingsDialog);
            OpenImageToolCommand = new RelayCommand(OnOpenImageTool);
            OpenChatTemplatesCommand = new RelayCommand(OnOpenChatTemplatesWindow);
            QnAButtonCommand = new RelayCommand(OnQnAButtonClicked);

            _chatService = chatService;
            _documentService = documentService;
            _dialogUtil = dialogUtil;
            _settingsService = settingsService;
            _toolService = toolService;
            _services = services;
            _systemMessageUtil = systemMessageUtil;
            _templateRepository = templateRepository;
            WeakReferenceMessenger.Default.Register<WindowEventMessage>(this, (r, m) => OnWindowStateMessage(m));
        }

        private void ToolService_StartNewChatEvent(object? sender, NewChatEventArgs e)
        {
            if (_currentDocument is not null)
            {
                string tone = string.IsNullOrEmpty(e.Tone) ? _currentDocument.Tone : e.Tone;
                string instructions = string.IsNullOrEmpty(e.Instructions) ? _currentDocument.Instructions : e.Instructions;
                Prompt = "";

                _currentDocument = _documentService.CreateDocument(tone, instructions, e.Topic);
                _currentDocument.DocumentName = e.Title;
                _currentDocument.Topic = e.Topic;

                OnPropertyChanged("");
            }
        }

        private void ToolService_ChatTitleEvent(object? sender, ChatTitleEventArgs e)
        {
            if (_currentDocument is not null)
            {
                if (e.Set)
                {
                    _currentDocument.DocumentName = e.Title;
                    OnPropertyChanged(nameof(DocumentName));
                }
                else
                {
                    e.Title = _currentDocument.DocumentName;
                }
            }
        }

        private void OnWindowStateMessage(WindowEventMessage message)
        {
            if (message.Type == WindowType.Main)
            {
                if (message.State == WindowEventType.Loaded)
                {
                    _documentService.SubscribeToOpenDocumentsChanged(DocumentService_OpenDocumentsChanged);
                    _toolService.SubscribeToChatTitle(ToolService_ChatTitleEvent);
                    _toolService.SubscribeToNewChat(ToolService_StartNewChatEvent);
                    _toolService.SubscribeToOpenImageTool(ToolService_OpenImageTool);

                    UserSettings userSettings = _settingsService.GetUserSettings();
                    _documentService.OpenDocumentList(userSettings.LastOpenFiles);

                    if (OpenDocuments.Count == 0)
                    {
                        _currentDocument = _documentService.CreateDocument(userSettings.DefaultTone, userSettings.DefaultInstructions);
                    }
                    else
                    {
                        _currentDocument = OpenDocuments.First();
                    }

                    SelectionChanged(_currentDocument, null);
                    InitializeTemplateMRU();
                }

                if (message.State == WindowEventType.Closing)
                {
                    WeakReferenceMessenger.Default.Unregister<WindowEventMessage>(this);
                    UserSettings userSettings = _settingsService.GetUserSettings();
                    _documentService.UpdateUserSettings(userSettings);
                    _settingsService.SetUserSettings(userSettings);

                    _documentService.UnsubscribeFromOpenDocumentsChanged(DocumentService_OpenDocumentsChanged);
                    _toolService.UnsubscribeFromChatTitle(ToolService_ChatTitleEvent);
                    _toolService.UnsubscribeFromNewChat(ToolService_StartNewChatEvent);
                    _toolService.UnsubscribeFromOpenImageTool(ToolService_OpenImageTool);
                }

                if (message.State == WindowEventType.Refresh)
                {
                    InitializeTemplateMRU();
                    OnPropertyChanged("");
                }
            }
            else if (message.Type == WindowType.ImageTool && message.State == WindowEventType.Closing)
            {
                _imageToolWindow = null;
            }
            else if (message.Type == WindowType.ChatTemplate && message.State == WindowEventType.Closing)
            {
                _chatTemplatesWindow = null;
            }
            else if (message.Type == WindowType.QnA && message.State == WindowEventType.Closing)
            {
                _questionAnswerWindow = null;
            }
        }

        private void InitializeTemplateMRU()
        {
            TemplateMRU.Clear();
            TemplateMRU.Add(new(_newChatTemplateGuid, "New chat..."));

            var settings = _settingsService.GetUserSettings();

            foreach (var mruEntry in settings.RecentTemplates)
            {
                TemplateMRU.Add(mruEntry);
            }

            TemplateMRU.Add(new(_openTemplateWindowGuid, "See more..."));
            OnPropertyChanged(nameof(TemplateMRU));
        }

        private void ToolService_OpenImageTool(object? sender, OpenImageToolEventArgs e)
        {
            OpenImageToolWindow();
        }

        private void DocumentService_OpenDocumentsChanged(object? sender, OpenDocumentsChangedEventArgs e)
        {
            if (e.Action == OpenDocumentsChangedAction.Added)
            {
                var document = _documentService.FindDocument(e.Identifier);

                if (document is not null)
                {
                    OpenDocuments.Add(document);
                    SetFocusOnDocument(document);
                }
            }
            else if (e.Action == OpenDocumentsChangedAction.Removed)
            {
                ChatDocument? newSelection = GetNewSelection(e.Identifier);
                var document = OpenDocuments.Where(d => d.Metadata.Identifier == e.Identifier).FirstOrDefault();

                if (document is not null)
                {
                    OpenDocuments.Remove(document);
                }

                if (OpenDocuments.Count == 0 || newSelection is null)
                {
                    var userSettings = _settingsService.GetUserSettings();
                    _currentDocument = _documentService.CreateDocument(userSettings.DefaultTone, userSettings.DefaultInstructions);
                    SetFocusOnDocument(_currentDocument);
                }
                else
                {
                    SetFocusOnDocument(newSelection);
                }
            }
        }

        private ChatDocument? GetNewSelection(Guid identifier)
        {
            ChatDocument? document = null;

            foreach(var doc in OpenDocuments)
            {
                if (doc.Metadata.Identifier == identifier)
                {
                    break;
                }
                else
                {
                    document = doc;
                }
            }

            if (document is null && OpenDocuments.Count > 1)
            {
                document = OpenDocuments[1];
            }

            return document;
        }

        public string Prompt
        {
            get => _prompt;

            set
            {
                SetProperty(ref _prompt, value);

                if (_currentDocument is not null)
                {
                    _currentDocument.Metadata.CurrentPrompt = value;
                }
            }
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

        public string WindowTitle
        {
            get
            {
                string filename = "Chat";
                bool isDirty = true;

                if (_currentDocument is not null)
                {
                    isDirty = _currentDocument.Metadata.IsDirty;

                    if (!string.IsNullOrEmpty(_currentDocument.Metadata.DocumentPath))
                    {
                        filename = _currentDocument.Metadata.DocumentPath;
                    }
                    else
                    {
                        filename = _currentDocument.Metadata.DocumentFilename;
                    }
                }

                return $"MyChat - {filename}{(isDirty ? " *" : "")}";
            }
        }

        public string DocumentName
        {
            get
            {
                if (_currentDocument is not null)
                {
                    return _currentDocument.DocumentName;
                }

                return "";
            }

            set
            {
                if (_currentDocument is not null)
                {
                    _currentDocument.DocumentName = value;
                }
            }
        }

        public ChatDocument CurrentDocument
        {
            get
            {
                if (_currentDocument is null)
                {
                    if (OpenDocuments.Count > 0)
                    {
                        _currentDocument = OpenDocuments.First();
                    }
                    else
                    {
                        _currentDocument = new();
                    }
                }

                return _currentDocument;
            }

            set
            {
                if (_currentDocument is not null && value is not null && _currentDocument.Metadata.Identifier != value.Metadata.Identifier)
                {
                    var previous = _currentDocument;
                    _currentDocument = value;
                    SelectionChanged(_currentDocument, previous);
                }
            }
        }

        public string CurrentChatModel
        {
            get
            {
                UserSettings settings = _settingsService.GetUserSettings();
                return settings.SelectedChatModel;
            }
        }

        public string CurrentTone
        {
            get => _currentDocument?.Tone ?? _systemMessageUtil.DefaultTone;
        }

        public double TokenUsageBarValue
        {
            get
            {
                double totalTokens = _currentDocument?.TotalTokens ?? 0;

                return 100 * (totalTokens / (25000)); // limiting chat size to 25k tokens
            }
        }

        public Brush TokenUsageBarColor
        {
            get
            {
                double barValue = TokenUsageBarValue;

                if (barValue < 25)
                {
                    return new SolidColorBrush(Colors.Green);
                }
                else if (barValue < 50)
                {
                    return new SolidColorBrush(Colors.GreenYellow);
                }
                else if (barValue < 70)
                {
                    return new SolidColorBrush(Colors.Yellow);
                }
                else if (barValue < 90)
                {
                    return new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    return new SolidColorBrush(Colors.Red);
                }
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

        private async Task OnSendPromptAsync()
        {
            SetWindowWaiting(true);

            if (_currentDocument is not null)
            {
                if (TokenUsageBarValue > 100)
                {
                    _dialogUtil.ShowErrorMessage("Token limit for this conversation has be exceeded. You will need to start a new conversation");
                    SetWindowWaiting(false);
                    return;
                }

                var errorMessage = await _chatService.SendPromptAsync(_currentDocument, _prompt);

                if (errorMessage is not null)
                {
                    _dialogUtil.ShowErrorMessage(errorMessage);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.ChatViewer, _currentDocument.ChatContent));
                    OnPropertyChanged(nameof(WindowTitle));
                    OnPropertyChanged(nameof(TokenUsageBarValue));
                    OnPropertyChanged(nameof(TokenUsageBarColor));
                    Prompt = "";
                }
            }

            SetWindowWaiting(false);
        }

        private void OnNewDocument(TemplateMRUEntry? entry)
        {
            if (entry is null)
            {
                return;
            }

            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.DoClose, WindowType.NewDocumentPopup));

            if (entry.Identifier == _newChatTemplateGuid)
            {
                NewDocumentFromDialog();
            }
            else if (entry.Identifier == _openTemplateWindowGuid)
            {
                OnOpenChatTemplatesWindow();
            }
            else
            {
                NewDocumentFromTemplateId(entry.Identifier);
            }
        }

        private void NewDocumentFromTemplateId(Guid identifier)
        {
            var templates = _templateRepository.Fetch();
            var template = templates.Where(t => t.Identifier == identifier).FirstOrDefault();

            if (template is not null)
            {
                var newChatDto = _dialogUtil.PromptForNewChat(template.Tone, template.Instructions, template.Topic);

                if (newChatDto.IsOk)
                {
                    _currentDocument = _documentService.CreateDocument(newChatDto.Tone, newChatDto.Instructions, newChatDto.Topic);
                    var settings = _settingsService.GetUserSettings();
                    settings.UpdateTemplateMRU(template.Identifier, templates);
                    _settingsService.SetUserSettings(settings);
                    InitializeTemplateMRU();
                    OnPropertyChanged("");

                }
            }
        }

        private void NewDocumentFromDialog()
        {
            var newChatDto = _dialogUtil.PromptForNewChat();

            if (newChatDto.IsOk)
            {
                _currentDocument = _documentService.CreateDocument(newChatDto.Tone, newChatDto.Instructions, newChatDto.Topic);
                OnPropertyChanged("");
            }
        }

        private void OnCloseDocument(object? parameter)
        {
            if (parameter is not null && _currentDocument is not null)
            {
                ChatDocument document = (ChatDocument)parameter;

                var result = _dialogUtil.AllowFileClosure(document);

                if (result == UserDialogResult.Cancel)
                {
                    return;
                }
                else if (result == UserDialogResult.Yes)
                {
                    PerformSaveDocument(document);

                    if (document.Metadata.IsDirty)
                    {
                        return; // assume that user clicked cancel on the save dialog
                    }
                }

                _documentService.CloseDocument(document);
            }
        }

        public void OnOpenDocument()
        {
            string documentPath = _dialogUtil.PromptForOpenDocumentPath();

            if (string.IsNullOrEmpty(documentPath))
            {
                return;
            }

            var foundDocument = _documentService.FindDocument(documentPath);

            if (foundDocument is not null)
            {
                SetFocusOnDocument(foundDocument);
                return;
            }

            var document = _documentService.OpenDocument(documentPath);

            if (document is not null)
            {
                SetFocusOnDocument(document);
                OnPropertyChanged("");
            }
            else
            {
                _dialogUtil.FailedToSaveDocument(documentPath);
            }
        }

        private void OnSaveDocumentAs()
        {
            if (_currentDocument is not null)
            {
                var documentPath = _dialogUtil.PromptForSaveDocumentPath(_currentDocument.DocumentFilename);

                if (string.IsNullOrEmpty(documentPath))
                {
                    return;
                }

                bool success = _documentService.SaveDocument(_currentDocument, documentPath);

                if (!success)
                {
                    _dialogUtil.FailedToSaveDocument(documentPath);
                }

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void OnSaveDocument()
        {
            if (_currentDocument is not null)
            {
                PerformSaveDocument(_currentDocument);

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void OnSaveAllDocument()
        {
            foreach (var document in OpenDocuments.Where(d => d.Metadata.IsDirty == true))
            {
                PerformSaveDocument(document);
            }

            OnPropertyChanged(nameof(WindowTitle));
        }

        private void PerformSaveDocument(ChatDocument document)
        {
            string? documentPath = document.Metadata.DocumentPath;

            if (string.IsNullOrEmpty(documentPath))
            {
                documentPath = _dialogUtil.PromptForSaveDocumentPath(document.Metadata.DocumentFilename);
            }

            if (string.IsNullOrEmpty(documentPath))
            {
                return;
            }

            bool success = _documentService.SaveDocument(document, documentPath);

            if (!success)
            {
                _dialogUtil.FailedToSaveDocument(documentPath);
            }
        }

        public async Task OnExportAsHTMLAsync()
        {
            if (_currentDocument is not null)
            {
                string filename = "Chat";

                if (!string.IsNullOrEmpty(_currentDocument.Metadata.DocumentPath))
                {
                    filename = _currentDocument.Metadata.DocumentFilename;
                }

                var filepath = _dialogUtil.PromptForExportHTMLPath(filename);

                if (string.IsNullOrEmpty(filepath))
                {
                    return;
                }

                bool exportResult = await _chatService.ExportAsHTMLAsync(_currentDocument, filepath);

                if (exportResult == false)
                {
                    _dialogUtil.FailedToExportHTML(filepath);
                }
            }
        }

        public void SelectionChanged(ChatDocument document, ChatDocument? previousDocument)
        {
            if (previousDocument is not null)
            {
                previousDocument.Metadata.CurrentPrompt = _prompt;
            }

            _prompt = document.Metadata.CurrentPrompt;
            WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.ChatViewer, document.ChatContent, true));            
            OnPropertyChanged("");
        }

        private void OnUndo()
        {
            if (_currentDocument is not null)
            {
                string lastPrompt = _documentService.Undo(_currentDocument);
                Prompt = lastPrompt;
                WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.ChatViewer, _currentDocument.ChatContent));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void OnRedo()
        {
            if (_currentDocument is not null)
            {
                _documentService.Redo(_currentDocument);
                Prompt = "";
                WeakReferenceMessenger.Default.Send(new WebViewUpdatedMessage(ViewerIdentification.ChatViewer, _currentDocument.ChatContent));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void SetFocusOnDocument(ChatDocument document)
        {
            var previous = _currentDocument;
            _currentDocument = document;
            SelectionChanged(document, previous);
        }

        private void OnOpenSettingsDialog()
        {
            var settingsWindow = _services.GetRequiredService<SettingsWindow>();
            settingsWindow.ShowDialog();
            OnPropertyChanged("");
        }

        private void OnOpenImageTool()
        {
            OpenImageToolWindow();
        }

        private void OpenImageToolWindow()
        {
            OpenSingleInstanceWindow(ref _imageToolWindow);
        }

        private void OnOpenChatTemplatesWindow()
        {
            OpenSingleInstanceWindow(ref _chatTemplatesWindow);
        }

        private void OnQnAButtonClicked()
        {
            OpenSingleInstanceWindow(ref _questionAnswerWindow);
        }

        private void OpenSingleInstanceWindow<T>(ref T? window) where T : Window
        {
            if (window is null)
            {
                window = _services.GetRequiredService<T>();
                window.Show();
            }
            else
            {
                window.Show();
                window.Activate();
            }
        }
    }
}
