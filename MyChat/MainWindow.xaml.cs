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
        private readonly IServiceProvider _services;
        private readonly ISettingsService _settingsService;
        private readonly IToolService _toolService;

        public MainWindow(MainViewModel viewModel, IDialogUtil dialogUtil, IServiceProvider services, ISettingsService settingsService, IToolService toolService)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();

            _dialogUtil = dialogUtil;
            _services = services;
            _settingsService = settingsService;
            _toolService = toolService;
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
            WeakReferenceMessenger.Default.Unregister<CursorStateChangeMessage>(this);
            WeakReferenceMessenger.Default.Send(new MainWindowStateMessage(MainWindowStateAction.Shutdown));

            Application.Current.Shutdown();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = _services.GetRequiredService<SettingsWindow>();
            settingsWindow.ShowDialog();
            WeakReferenceMessenger.Default.Send(new MainWindowStateMessage(MainWindowStateAction.Refresh));
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

        private void OnCursorStateChanged(CursorStateChangeMessage message)
        {
            Cursor = message.IsWaiting ? Cursors.Wait : null;
            Mouse.OverrideCursor = message.IsWaiting ? Cursors.Wait : null;
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
            WeakReferenceMessenger.Default.Register<CursorStateChangeMessage>(this, (r, m) => OnCursorStateChanged(m));
            WeakReferenceMessenger.Default.Send(new MainWindowStateMessage(MainWindowStateAction.Startup));
            _toolService.SubscribeToOpenImageTool(ToolService_OpenImageTool);
        }

        private void ToolService_OpenImageTool(object? sender, OpenImageToolEventArgs e)
        {
            OpenImageTool();
        }

        private void ImageTool_Click(object sender, RoutedEventArgs e)
        {
            OpenImageTool();
        }

        private void OpenImageTool()
        {
            var imageToolWindow = _services.GetRequiredService<ImageToolWindow>();
            imageToolWindow.Show();
        }
    }
}