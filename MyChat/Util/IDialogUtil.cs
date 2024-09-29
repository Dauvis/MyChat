using MyChat.DTO;
using MyChat.Model;
using System.Collections.ObjectModel;

namespace MyChat.Util
{
    public interface IDialogUtil
    {
        UserDialogResult AllowApplicationClosure(ObservableCollection<ChatDocument> documents);
        UserDialogResult AllowFileClosure(ChatDocument document);
        void FailedToExportHTML(string filepath);
        void FailedToSaveDocument(string documentPath);
        bool PromptForConfirmation(string message);
        string PromptForExportHTMLPath(string curDocumentPath);
        NewChatDTO PromptForNewChat();
        string PromptForOpenDocumentPath();
        string PromptForSaveDocumentPath(string filename);
        string PromptForSaveImagePath();
        void ShowErrorMessage(string errorMessage);
    }
}