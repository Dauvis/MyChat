using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using MyChat.Messages;
using MyChat.Service;
using MyChat.Util;
using MyChat.ViewModel;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Encodings.Web;
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
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Closing, WindowType.Main));

            Application.Current.Shutdown();
        }

        private async Task OnChatViewUpdatedMessageAsync(ChatViewUpdatedMessage message)
        {
            await ChatViewer.EnsureCoreWebView2Async();
            string untickedText = message.ChatText.Replace("`", "'"); // I don't like backticks

            string scrollYPosition = "0";

            if (!message.IsReplacement)
            {
                var jsResult = await ChatViewer.CoreWebView2.ExecuteScriptWithResultAsync("window.scrollY.toString()");

                if (jsResult.Succeeded)
                {
                    jsResult.TryGetResultAsString(out scrollYPosition, out _);
                }
            }

            ChatViewer.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, untickedText));
            await Task.Delay(500);
            await ChatViewer.CoreWebView2.ExecuteScriptAsync($"window.scroll(0, {scrollYPosition});");
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

            WeakReferenceMessenger.Default.Register<ChatViewUpdatedMessage>(this, async (r, m) => await OnChatViewUpdatedMessageAsync(m));
            WeakReferenceMessenger.Default.Register<WindowEventMessage>(this, (r, m) => OnWindowEventMessage(m));
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Loaded, WindowType.Main));
        }

        private void OnWindowEventMessage(WindowEventMessage m)
        {
            if (m.Type == WindowType.NewDocumentPopup)
            {
                if (m.State == WindowEventType.DoClose)
                {
                    NewDocumentPopup.IsOpen = false;
                }
            }
        }
    }
}