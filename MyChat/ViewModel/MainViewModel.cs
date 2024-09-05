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

        private string _prompt = string.Empty;
        private IMainWindowBridgeUtil? _mainWindowBridgeUtil;
        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;

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

        public MainViewModel(IChatService chatService, IDocumentService documentService)
        {
            SendPromptCommand = new RelayCommand(SendPromptAsync);
            NewDocumentCommand = new RelayCommand(NewDocumentAsync);
            OpenDocumentCommand = new RelayCommand(OpenDocumentAsync);
            CloseDocumentCommand = new RelayCommand(CloseDocument);
            SaveDocumentCommand = new RelayCommand(SaveDocumentAsync);
            SaveDocumentAsCommand = new RelayCommand(SaveDocumentAsAsync);
            SaveAllDocumentCommand = new RelayCommand(SaveAllDocumentAsync);
            UndoCommand = new RelayCommand(UndoAsync);
            RedoCommand = new RelayCommand(RedoAsync);
            ExportAsHTMLCommand = new RelayCommand(ExportAsHTMLAsync);

            _chatService = chatService;
            _documentService = documentService;
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
            get => _prompt;

            set
            {
                _prompt = value;
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
                var errorMessage = await _chatService.SendPromptAsync(_currentDocument, _prompt);

                if (errorMessage is not null)
                {
                    MessageBox.Show(errorMessage);
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

        private void CloseDocument(object parameter)
        {
            if (parameter is not null && _currentDocument is not null)
            {
                ChatDocument document = (ChatDocument)parameter;
                _documentService.CloseDocument(document);
            }
        }

        public async Task OpenDocumentAsync()
        {
            string documentPath = PromptForOpenDocumentPath();

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
                MessageBox.Show($"Failed to open chat file ({documentPath})");
            }
        }

        private async Task SaveDocumentAsAsync()
        {
            if (_currentDocument is not null)
            {
                var documentPath = PromptForSaveDocumentPath(_currentDocument.DocumentPath);

                if (string.IsNullOrEmpty(documentPath))
                {
                    return;
                }

                bool success = await _documentService.SaveDocumentAsync(_currentDocument, documentPath);

                if (!success)
                {
                    MessageBox.Show($"Failed to save chat file ({documentPath})");
                }

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task SaveDocumentAsync()
        {
            if (_currentDocument is not null)
            {
                await PerformSaveDocument(_currentDocument);

                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task SaveAllDocumentAsync()
        {
            foreach (var document in OpenDocuments.Where(d => d.IsDirty == true))
            {
                await PerformSaveDocument(document);
            }

            OnPropertyChanged(nameof(WindowTitle));
        }

        private async Task PerformSaveDocument(ChatDocument document)
        {
            string? documentPath = document.DocumentPath;

            if (string.IsNullOrEmpty(documentPath))
            {
                documentPath = PromptForSaveDocumentPath(document.Filename);
            }

            if (string.IsNullOrEmpty(documentPath))
            {
                return;
            }

            bool success = await _documentService.SaveDocumentAsync(document, documentPath);

            if (!success)
            {
                MessageBox.Show($"Failed to save chat file ({documentPath})");
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

                var filepath = PromptForExportHTMLPath(filename);

                if (string.IsNullOrEmpty(filepath))
                {
                    return;
                }

                bool exportResult = await _chatService.ExportAsHTMLAsync(_currentDocument, filepath);

                if (exportResult == false)
                {
                    MessageBox.Show($"Failed to export HTML file ({filepath})");
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


        private static string PromptForExportHTMLPath(string curDocumentPath)
        {
            var dialog = new SaveFileDialog
            {
                FileName = curDocumentPath,
                DefaultExt = ".html",
                Filter = "HTML (.html)|*.html"
            };

            bool? result = dialog.ShowDialog();

            if (result == false)
            {
                return string.Empty;
            }

            return dialog.FileName;
        }

        private static string PromptForSaveDocumentPath(string filename)
        {
            var dialog = new SaveFileDialog
            {
                FileName = filename,
                DefaultExt = ".chat",
                Filter = "Chat (.chat)|*.chat"
            };

            bool? result = dialog.ShowDialog();

            if (result == false)
            {
                return string.Empty;
            }

            return dialog.FileName;
        }

        private static string PromptForOpenDocumentPath()
        {
            var dialog = new OpenFileDialog
            {
                FileName = "Chat",
                DefaultExt = ".chat",
                Filter = "Chat (.chat)|*.chat"
            };

            bool? result = dialog.ShowDialog();

            if (result == false)
            {
                return string.Empty;
            }

            return dialog.FileName;
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
