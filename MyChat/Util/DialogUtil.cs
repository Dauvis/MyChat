using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MyChat.DTO;
using MyChat.Model;
using MyChat.Service;
using MyChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyChat.Util
{
    public enum UserDialogResult
    {
        No,
        Yes,
        Cancel
    }

    public class DialogUtil : IDialogUtil
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService;

        public DialogUtil(IServiceProvider serviceProvider, ISettingsService settingsService)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService;
        }

        public UserDialogResult AllowApplicationClosure(ObservableCollection<ChatDocument> documents)
        {
            int dirtyDocs = documents.Where(d => d.Metadata.IsDirty).Count();

            if (dirtyDocs > 0)
            {
                MessageBoxResult result = MessageBox.Show("One or more chats have not been saved. Do you wish to save them?", "MyChat", MessageBoxButton.YesNoCancel);

                return MsgBoxResultToDlgResult(result);
            }

            return UserDialogResult.Yes;
        }

        public UserDialogResult AllowFileClosure(ChatDocument document)
        {
            if (document is not null)
            {
                int chatExchanges = document.Exchanges.Where(e => e.Type == ExchangeType.Chat).Count();

                if (!document.Metadata.IsDirty || chatExchanges == 0)
                {
                    return UserDialogResult.No; // returning No as the file does not need to be saved
                }

                MessageBoxResult result = MessageBox.Show($"File {document.Metadata.DocumentFilename} has been changed. Do you wish to save?", "MyChat", MessageBoxButton.YesNoCancel);
                return MsgBoxResultToDlgResult(result);
            }

            return UserDialogResult.No;
        }

        public string PromptForExportHTMLPath(string curDocumentPath)
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
                return "";
            }

            return dialog.FileName;
        }

        public string PromptForSaveDocumentPath(string filename)
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
                return "";
            }

            return dialog.FileName;
        }

        public string PromptForOpenDocumentPath()
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
                return "";
            }

            return dialog.FileName;
        }

        public string PromptForSaveImagePath()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".png",
                Filter = "Portable Network Graphics (.png)|*.png"
            };

            bool? result = dialog.ShowDialog();

            if (result == false)
            {
                return "";
            }

            return dialog.FileName;
        }

        public NewChatDTO PromptForNewChat()
        {
            var userSettings = _settingsService.GetUserSettings();
            return PerformNewChatDialog(userSettings.DefaultTone, userSettings.DefaultInstructions, "");
        }

        public NewChatDTO PromptForNewChat(string tone, string instuctions, string topic)
        {
            return PerformNewChatDialog(tone, instuctions, topic);
        }

        private NewChatDTO PerformNewChatDialog(string tone, string instructions, string topic)
        {
            var newChatWindow = _serviceProvider.GetRequiredService<NewChatWindow>();

            if (newChatWindow == null)
            {
                return new();
            }

            NewChatDTO dto = new()
            {
                Tone = tone,
                Instructions = instructions,
                Topic = topic
            };

            var dlgViewModel = (NewChatViewModel)newChatWindow.DataContext;
            dlgViewModel.DialogValues = dto;

            var dlgResult = newChatWindow.ShowDialog();
            var ret = dlgViewModel.DialogValues;
            ret.IsOk = dlgResult == true;

            return ret;

        }

        public void FailedToSaveDocument(string documentPath)
        {
            MessageBox.Show($"Failed to save chat file ({documentPath})");
        }

        public void FailedToExportHTML(string filepath)
        {
            MessageBox.Show($"Failed to export HTML file ({filepath})");
        }

        public void ShowErrorMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage);
        }

        public bool PromptForConfirmation(string message)
        {
            var result = MessageBox.Show(message, "MyChat", MessageBoxButton.YesNo);

            return result == MessageBoxResult.Yes;
        }

        private static UserDialogResult MsgBoxResultToDlgResult(MessageBoxResult msgBoxResult)
        {
            if (msgBoxResult == MessageBoxResult.Yes)
            {
                return UserDialogResult.Yes;
            }
            else if (msgBoxResult == MessageBoxResult.No)
            {
                return UserDialogResult.No;
            }
            else if (msgBoxResult == MessageBoxResult.Cancel)
            {
                return UserDialogResult.Cancel;
            }

            return UserDialogResult.Cancel;
        }
    }
}
