using Microsoft.Extensions.DependencyInjection;
using MyChat.Util;
using MyChat.ViewModel;
using System;
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
        private readonly IServiceProvider _services;

        public MainWindow(MainViewModel viewModel, IDialogUtil dialogUtil, IServiceProvider services)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _dialogUtil = dialogUtil;
            _services = services;
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = _services.GetRequiredService<SettingsWindow>();
            settingsWindow.ShowDialog();
        }
    }
}