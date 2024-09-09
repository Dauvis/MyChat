using MyChat.Util;
using MyChat.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly IDialogUtil _dialogUtil;

        public MainWindow(MainViewModel viewModel, IDialogUtil dialogUtil)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _dialogUtil = dialogUtil;
            var bridge = new MainWindowBridgeUtil(this, ChatViewer);
            _viewModel.SetMainWindowBridge(bridge);
            _ = bridge.InitializeAsync();
            DataContext = viewModel;
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
        }
    }
}