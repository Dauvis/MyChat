using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyChat.Messages;
using MyChat.Model;
using MyChat.Service;
using MyChat.Util;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private ChatDocument? _currentDocument;
        public ObservableCollection<ChatDocument> OpenDocuments { get; set; }

        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;
        private readonly IDialogUtil _dialogUtil;
        private readonly ISettingsService _settingsService;

        private string _prompt = string.Empty;

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

        public MainViewModel(IChatService chatService, IDocumentService documentService, IDialogUtil dialogUtil, ISettingsService settingsService)
        {
            SendPromptCommand = new AsyncRelayCommand(SendPromptAsync);
            NewDocumentCommand = new RelayCommand(NewDocument);
            OpenDocumentCommand = new RelayCommand(OpenDocument);
            CloseDocumentCommand = new RelayCommand<object>(CloseDocument);
            SaveDocumentCommand = new RelayCommand(SaveDocument);
            SaveDocumentAsCommand = new RelayCommand(SaveDocumentAs);
            SaveAllDocumentCommand = new RelayCommand(SaveAllDocument);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            ExportAsHTMLCommand = new AsyncRelayCommand(ExportAsHTMLAsync);

            _chatService = chatService;
            _documentService = documentService;
            _dialogUtil = dialogUtil;
            _settingsService = settingsService;
            _documentService.OpenDocumentsChanged += DocumentService_OpenDocumentsChanged;

            OpenDocuments = [];
            WeakReferenceMessenger.Default.Register<MainWindowStateMessage>(this, (r, m) => OnMainWindowState(m));
        }

        private void OnMainWindowState(MainWindowStateMessage message)
        {
            if (message.StateAction == MainWindowStateAction.Startup)
            {
                UserSettings userSettings = _settingsService.GetUserSettings();
                _documentService.OpenDocumentList(userSettings.LastOpenFiles);

                if (OpenDocuments.Count == 0)
                {
                    _currentDocument = _documentService.CreateDocument(userSettings.DefaultTone, userSettings.DefaultCustomInstructions);
                }
                else
                {
                    _currentDocument = OpenDocuments.First();
                }

                SelectionChanged(_currentDocument, null);
            }

            if (message.StateAction == MainWindowStateAction.Shutdown)
            {
                WeakReferenceMessenger.Default.Unregister<MainWindowStateMessage>(this);
                UserSettings userSettings = _settingsService.GetUserSettings();
                _documentService.UpdateUserSettings(userSettings);
                _settingsService.SetUserSettings(userSettings);
            }

            if (message.StateAction == MainWindowStateAction.Refresh)
            {
                OnPropertyChanged("");
            }
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
                    _currentDocument = _documentService.CreateDocument(userSettings.DefaultTone, userSettings.DefaultCustomInstructions);
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

                return string.Empty;
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
            get => _currentDocument?.Tone ?? SystemPrompts.DefaultTone;
        }

        private async Task SendPromptAsync()
        {
            WeakReferenceMessenger.Default.Send(new CursorStateChangeMessage(true));

            if (_currentDocument is not null)
            {
                var errorMessage = await _chatService.SendPromptAsync(_currentDocument, _prompt);

                if (errorMessage is not null)
                {
                    _dialogUtil.ShowErrorMessage(errorMessage);
                }
                else
                {
                    WeakReferenceMessenger.Default.Send(new ChatViewUpdatedMessage(_currentDocument.ChatContent));
                    OnPropertyChanged(nameof(WindowTitle));
                    Prompt = string.Empty;
                }
            }

            WeakReferenceMessenger.Default.Send(new CursorStateChangeMessage(false));
        }

        private void NewDocument()
        {
            var newChatDto = _dialogUtil.PromptForNewChat();
            _currentDocument = _documentService.CreateDocument(newChatDto.Tone, newChatDto.CustomInstructions);
            OnPropertyChanged(string.Empty);
        }

        private void CloseDocument(object? parameter)
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

        public void OpenDocument()
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
                OnPropertyChanged(string.Empty);
            }
            else
            {
                _dialogUtil.FailedToSaveDocument(documentPath);
            }
        }

        private void SaveDocumentAs()
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

        private void SaveDocument()
        {
            if (_currentDocument is not null)
            {
                PerformSaveDocument(_currentDocument);

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void SaveAllDocument()
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

        public async Task ExportAsHTMLAsync()
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
            WeakReferenceMessenger.Default.Send(new ChatViewUpdatedMessage(document.ChatContent, true));            
            OnPropertyChanged(string.Empty);
        }

        private void Undo()
        {
            if (_currentDocument is not null)
            {
                string lastPrompt = _documentService.Undo(_currentDocument);
                Prompt = lastPrompt;
                WeakReferenceMessenger.Default.Send(new ChatViewUpdatedMessage(_currentDocument.ChatContent));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void Redo()
        {
            if (_currentDocument is not null)
            {
                _documentService.Redo(_currentDocument);
                Prompt = string.Empty;
                WeakReferenceMessenger.Default.Send(new ChatViewUpdatedMessage(_currentDocument.ChatContent));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void SetFocusOnDocument(ChatDocument document)
        {
            var previous = _currentDocument;
            _currentDocument = document;
            SelectionChanged(document, previous);
        }
    }
}
