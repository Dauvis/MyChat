using System.Windows;

namespace MyChat
{
    public interface IMainWindowBridgeUtil
    {
        Window MainWindow { get; }

        Task AppendChatMessageAsync(string htmlContent);
        Task SetChatMessagesAsync(string htmlContent);
        void SetCursorState(bool isWaiting);
        Task InitializeAsync();
    }
}
