using MyChat.Common.DTO;
using MyChat.Common.Model;
using MyChat.Common.Util;
using System.Collections.ObjectModel;

namespace MyChat.Common.Interfaces
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
        NewChatDTO PromptForNewChat(string tone, string instuctions, string topic);
        string PromptForOpenDocumentPath();
        string PromptForSaveDocumentPath(string filename);
        string PromptForSaveImagePath();
        void ShowErrorMessage(string errorMessage);
    }
}