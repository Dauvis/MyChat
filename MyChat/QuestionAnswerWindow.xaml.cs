using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Web.WebView2.Core;
using MyChat.Common;
using MyChat.Common.Messages;
using MyChat.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for QuestionAnswerWindow.xaml
    /// </summary>
    public partial class QuestionAnswerWindow : Window
    {
        private readonly QuestionAnswerViewModel _viewModel;

        public QuestionAnswerWindow(QuestionAnswerViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();

            QnAViewer.CoreWebView2InitializationCompleted += (sender, args) =>
            {
                QnAViewer.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Register<WebViewUpdatedMessage>(this, async (r, m) => await OnChatViewUpdatedMessageAsync(m));
        }

        private async Task OnChatViewUpdatedMessageAsync(WebViewUpdatedMessage message)
        {
            if (message.ViewerId == ViewerIdentification.QnAViewer)
            {
                await QnAViewer.EnsureCoreWebView2Async();
                string untickedText = message.ChatText.Replace("`", "'"); // I don't like backticks

                QnAViewer.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, untickedText));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<WebViewUpdatedMessage>(this);

            if (QnAViewer.CoreWebView2 != null)
            {
                QnAViewer.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
            }
        }
    }
}
