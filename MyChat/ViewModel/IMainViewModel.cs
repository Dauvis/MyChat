using System.ComponentModel;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public interface IMainViewModel
    {
        ICommand ExportAsHTMLCommand { get; }
        void SetMainWindowBridge(IMainWindowBridgeUtil mainWindowBridgeUtil);
        ICommand NewDocumentCommand { get; }
        ICommand OpenDocumentCommand { get; }
        string Prompt { get; set; }
        ICommand RedoCommand { get; }
        ICommand SaveDocumentAsCommand { get; }
        ICommand SaveDocumentCommand { get; }
        ICommand SendPromptCommand { get; }
        string DocumentName { get; set; }
        ICommand UndoCommand { get; }
        string WindowTitle { get; }

        event PropertyChangedEventHandler? PropertyChanged;

        Task ExportAsHTMLAsync();
        Task OpenDocumentAsync();
    }
}