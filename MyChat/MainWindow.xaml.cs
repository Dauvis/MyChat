using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Web.WebView2.Core;
using MyChat.Common;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.Util;
using MyChat.ViewModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

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

            ChatViewer.CoreWebView2InitializationCompleted += (sender, args) =>
            {
                ChatViewer.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            };
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var url = e.Uri;

            // Here you can specify conditions for URLs that should open in the default browser
            if (ShouldOpenInBrowser(url))
            {
                // Cancel WebView2 navigation
                e.Cancel = true;

                // Open the URL in the default browser
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private static bool ShouldOpenInBrowser(string url)
        {
            if (url.StartsWith("data:text/html", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
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

            settings.MainWindow.Rectangle = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
            settings.MainWindow.ChatColumnWidth = columns[0].Width.Value;
            settings.MainWindow.MessageColumnWidth = columns[4].Width.Value;

            _settingsService.SetUserSettings(settings);

            WeakReferenceMessenger.Default.Unregister<WebViewUpdatedMessage>(this);
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Closing, WindowType.Main));

            if (ChatViewer.CoreWebView2 != null)
            {
                ChatViewer.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
            }

            Application.Current.Shutdown();
        }

        private async Task OnChatViewUpdatedMessageAsync(WebViewUpdatedMessage message)
        {
            if (message.ViewerId == ViewerIdentification.ChatViewer)
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

            WeakReferenceMessenger.Default.Register<WebViewUpdatedMessage>(this, async (r, m) => await OnChatViewUpdatedMessageAsync(m));
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

            if (m.Type == WindowType.Main)
            {
                if (m.State == WindowEventType.Focus)
                {
                    Activate();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutWindow();
            dialog.ShowDialog();
        }
    }
}