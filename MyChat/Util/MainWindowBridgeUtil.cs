using Microsoft.Web.WebView2.Wpf;
using MyChat.Util;
using System.Windows;
using System.Windows.Input;

namespace MyChat
{
    public class MainWindowBridgeUtil : IMainWindowBridgeUtil
    {        
        private readonly WebView2 _chatWebView;
        private readonly Window _window;

        public Window MainWindow => _window;

        public MainWindowBridgeUtil(Window window, WebView2 chatWebView) 
        {
            _window = window;
            _chatWebView = chatWebView;
        }

        public async Task AppendChatMessageAsync(string htmlContent)
        {
            // Ensure WebView2 is initialized
            await _chatWebView.EnsureCoreWebView2Async();

            // Update the WebView2 with the new content
            if (string.IsNullOrEmpty(htmlContent))
            {
                await _chatWebView.CoreWebView2.ExecuteScriptAsync($"document.body.innerHTML = `{HTMLConstants.StartChatMessage}`;");
            }
            else
            {
                await _chatWebView.CoreWebView2.ExecuteScriptAsync($"document.body.innerHTML = `{htmlContent}`;");
            }
        }

        public void SetCursorState(bool isWaiting)
        {
            _window.Cursor = isWaiting ? Cursors.Wait : null;
            Mouse.OverrideCursor = isWaiting ? Cursors.Wait : null;
        }

        public async Task InitializeAsync()
        {
            await _chatWebView.EnsureCoreWebView2Async();
            _chatWebView.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, HTMLConstants.StartChatMessage));
        }

        public async Task SetChatMessagesAsync(string htmlContent)
        {
            await _chatWebView.EnsureCoreWebView2Async();

            if (string.IsNullOrEmpty(htmlContent))
            {
                _chatWebView.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, HTMLConstants.StartChatMessage));
            }
            else
            {
                _chatWebView.NavigateToString(string.Format(HTMLConstants.DocumentTemplate, htmlContent));
            }
        }
    }
}
