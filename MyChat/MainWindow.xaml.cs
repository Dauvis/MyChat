using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MyChat.Messages;
using MyChat.Service;
using MyChat.Util;
using MyChat.ViewModel;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly IDialogUtil _dialogUtil;
        private readonly ISettingsService _settingsService;

        public MainWindow(MainViewModel viewModel, IDialogUtil dialogUtil, ISettingsService settingsService)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();

            _dialogUtil = dialogUtil;
            _settingsService = settingsService;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            UserDialogResult result = _dialogUtil.AllowApplicationClosure(_viewModel.OpenDocuments);

            if (result == UserDialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == UserDialogResult.Yes)
            {
                _viewModel.SaveAllDocumentCommand.Execute(null);
            }

            var settings = _settingsService.GetUserSettings();
            Grid content = (Grid)Content;
            var columns = content.ColumnDefinitions;

            settings.MainWindow.Rectangle = new Rect(Left, Top, Width, Height);
            settings.MainWindow.ChatColumnWidth = columns[0].Width.Value;
            settings.MainWindow.MessageColumnWidth = columns[4].Width.Value;

            _settingsService.SetUserSettings(settings);

            WeakReferenceMessenger.Default.Unregister<ChatViewUpdatedMessage>(this);
            WeakReferenceMessenger.Default.Send(new MainWindowStateMessage(MainWindowStateAction.Shutdown));

            Application.Current.Shutdown();
        }

        private async Task OnChatViewUpdatedAsync(ChatViewUpdatedMessage message)
        {
            await ChatViewer.EnsureCoreWebView2Async();

            if (message.IsReplacement)
            {
                ChatViewer.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, message.ChatText));
            }
            else
            {
                await ChatViewer.CoreWebView2.ExecuteScriptAsync($"document.body.innerHTML = `{message.ChatText}`;");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = _settingsService.GetUserSettings();
            Grid content = (Grid)Content;
            var columns = content.ColumnDefinitions;

            if (settings.MainWindow.Rectangle.Width > 0)
            {
                Left = settings.MainWindow.Rectangle.Left;
                Top = settings.MainWindow.Rectangle.Top;
                Width = settings.MainWindow.Rectangle.Width;
                Height = settings.MainWindow.Rectangle.Height;
                columns[0].Width = new(settings.MainWindow.ChatColumnWidth);
                columns[4].Width = new(settings.MainWindow.MessageColumnWidth);
            }

            WeakReferenceMessenger.Default.Register<ChatViewUpdatedMessage>(this, async (r, m) => await OnChatViewUpdatedAsync(m));
            WeakReferenceMessenger.Default.Send(new MainWindowStateMessage(MainWindowStateAction.Startup));
        }
    }
}