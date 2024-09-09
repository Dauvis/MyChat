using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MyChat.Model;
using MyChat.Service;
using MyChat.Util;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IMainViewModel
    {
        private ChatDocument? _currentDocument;
        public ObservableCollection<ChatDocument> OpenDocuments { get; set; }

        private Dictionary<Guid, string> _prompts = [];
        private IMainWindowBridgeUtil? _mainWindowBridgeUtil;
        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;
        private readonly IDialogUtil _dialogUtil;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public MainViewModel(IChatService chatService, IDocumentService documentService, IDialogUtil dialogUtil)
        {
            SendPromptCommand = new AsyncRelayCommand(SendPromptAsync);
            NewDocumentCommand = new AsyncRelayCommand(NewDocumentAsync);
            OpenDocumentCommand = new AsyncRelayCommand(OpenDocumentAsync);
            CloseDocumentCommand = new AsyncRelayCommand<object>(CloseDocumentAsync);
            SaveDocumentCommand = new AsyncRelayCommand(SaveDocumentAsync);
            SaveDocumentAsCommand = new AsyncRelayCommand(SaveDocumentAsAsync);
            SaveAllDocumentCommand = new AsyncRelayCommand(SaveAllDocumentAsync);
            UndoCommand = new AsyncRelayCommand(UndoAsync);
            RedoCommand = new AsyncRelayCommand(RedoAsync);
            ExportAsHTMLCommand = new AsyncRelayCommand(ExportAsHTMLAsync);

            _chatService = chatService;
            _documentService = documentService;
            _dialogUtil = dialogUtil;
            _documentService.OpenDocumentsChanged += DocumentService_OpenDocumentsChanged;

            OpenDocuments = [];
            _currentDocument = _documentService.CreateDocument("");
        }

        private void DocumentService_OpenDocumentsChanged(object? sender, OpenDocumentsChangedEventArgs e)
        {
            if (e.Action == OpenDocumentsChangedAction.Added)
            {
                var document = _documentService.FindDocument(e.Identifier);

                if (document is not null)
                {
                    OpenDocuments.Add(document);
                    _prompts[document.Identifier] = string.Empty;
                    SetFocusOnDocument(document);
                }
            }
            else if (e.Action == OpenDocumentsChangedAction.Removed)
            {
                ChatDocument? newSelection = GetNewSelection(e.Identifier);
                var document = OpenDocuments.Where(d => d.Identifier == e.Identifier).FirstOrDefault();

                if (document is not null)
                {
                    OpenDocuments.Remove(document);
                    _prompts.Remove(document.Identifier);
                }

                if (OpenDocuments.Count == 0 || newSelection is null)
                {
                    _currentDocument = _documentService.CreateDocument("");
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
                if (doc.Identifier == identifier)
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

        public void SetMainWindowBridge(IMainWindowBridgeUtil mainWindowBridgeUtil)
        {
            _mainWindowBridgeUtil = mainWindowBridgeUtil;
        }

        public string Prompt
        {
            get
            {
                if (_currentDocument is not null)
                {
                    return _prompts[_currentDocument.Identifier];
                }

                return string.Empty;
            }

            set
            {
                if (_currentDocument is not null)
                {
                    _prompts[_currentDocument.Identifier] = value;
                }

                OnPropertyChanged(nameof(Prompt));
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
                    isDirty = _currentDocument.IsDirty;

                    if (!string.IsNullOrEmpty(_currentDocument.DocumentPath))
                    {
                        filename = _currentDocument.DocumentPath;
                    }
                    else
                    {
                        filename = _currentDocument.Filename;
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
                _currentDocument ??= OpenDocuments.First();

                return _currentDocument;
            }

            set
            {
                if (_currentDocument is not null && value is not null && _currentDocument.Identifier != value.Identifier)
                {
                    _currentDocument = value;
                    _ = SelectionChangedAsync(_currentDocument);
                }
            }
        }

        private async Task SendPromptAsync()
        {
            _mainWindowBridgeUtil!.SetCursorState(true);

            if (_currentDocument is not null)
            {
                var errorMessage = await _chatService.SendPromptAsync(_currentDocument, _prompts[_currentDocument.Identifier]);

                if (errorMessage is not null)
                {
                    _dialogUtil.ShowErrorMessage(errorMessage);
                }
                else
                {
                    await _mainWindowBridgeUtil.AppendChatMessageAsync(_currentDocument.ChatContent);
                    OnPropertyChanged(nameof(WindowTitle));
                    Prompt = string.Empty;
                }
            }

            _mainWindowBridgeUtil.SetCursorState(false);
        }

        private async Task NewDocumentAsync()
        {
            _currentDocument = _documentService.CreateDocument("");
            await _mainWindowBridgeUtil!.InitializeAsync();
            OnPropertyChanged(string.Empty);
        }

        private async Task CloseDocumentAsync(object? parameter)
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
                    await PerformSaveDocumentAsync(document);

                    if (document.IsDirty)
                    {
                        return; // assume that user clicked cancel on the save dialog
                    }
                }

                _documentService.CloseDocument(document);
            }
        }

        public async Task OpenDocumentAsync()
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

            var document = await _documentService.OpenDocumentAsync(documentPath);

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

        private async Task SaveDocumentAsAsync()
        {
            if (_currentDocument is not null)
            {
                var documentPath = _dialogUtil.PromptForSaveDocumentPath(_currentDocument.DocumentPath);

                if (string.IsNullOrEmpty(documentPath))
                {
                    return;
                }

                bool success = await _documentService.SaveDocumentAsync(_currentDocument, documentPath);

                if (!success)
                {
                    _dialogUtil.FailedToSaveDocument(documentPath);
                }

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task SaveDocumentAsync()
        {
            if (_currentDocument is not null)
            {
                await PerformSaveDocumentAsync(_currentDocument);

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task SaveAllDocumentAsync()
        {
            foreach (var document in OpenDocuments.Where(d => d.IsDirty == true))
            {
                await PerformSaveDocumentAsync(document);
            }

            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task PerformSaveDocumentAsync(ChatDocument document)
        {
            string? documentPath = document.DocumentPath;

            if (string.IsNullOrEmpty(documentPath))
            {
                documentPath = _dialogUtil.PromptForSaveDocumentPath(document.Filename);
            }

            if (string.IsNullOrEmpty(documentPath))
            {
                return;
            }

            bool success = await _documentService.SaveDocumentAsync(document, documentPath);

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

                if (!string.IsNullOrEmpty(_currentDocument.DocumentPath))
                {
                    filename = Path.GetFileNameWithoutExtension(_currentDocument.DocumentPath);
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

        public async Task SelectionChangedAsync(ChatDocument document)
        {
            await _mainWindowBridgeUtil!.SetChatMessagesAsync(document.ChatContent);
            OnPropertyChanged(string.Empty);
        }

        private async Task UndoAsync()
        {
            if (_currentDocument is not null)
            {
                string lastPrompt = _documentService.Undo(_currentDocument);
                Prompt = lastPrompt;
                await _mainWindowBridgeUtil!.AppendChatMessageAsync(_currentDocument.ChatContent);
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task RedoAsync()
        {
            if (_currentDocument is not null)
            {
                _documentService.Redo(_currentDocument);
                Prompt = string.Empty;
                await _mainWindowBridgeUtil!.AppendChatMessageAsync(_currentDocument.ChatContent);
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private void SetFocusOnDocument(ChatDocument document)
        {
            CurrentDocument = document;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
