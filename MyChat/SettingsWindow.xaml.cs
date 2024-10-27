using CommunityToolkit.Mvvm.Messaging;
using MyChat.Common.Interfaces;
using MyChat.Common.Messages;
using MyChat.Common.Util;
using MyChat.ViewModel;
using System.Windows;

namespace MyChat
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;
        private readonly SystemMessageUtil _systemMessageUtil;

        public SettingsWindow(SettingsViewModel viewModel, IGPTService gptService, SystemMessageUtil systemMessageUtil)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _systemMessageUtil = systemMessageUtil;
            DataContext = viewModel;
            WeakReferenceMessenger.Default.Register<WindowEventMessage>(this, (r, m) => OnWindowState(m));

            InitViewModel();
            InitModelCombo(gptService);
            InitToneCombo();
        }

        private void InitViewModel()
        {
            _viewModel.LoadUserSettings();
        }

        private void InitModelCombo(IGPTService gptService)
        {
            ModelCombo.Items.Add("");

            foreach (string model in gptService.AvailableModels)
            {
                ModelCombo.Items.Add(model);
            }
        }

        private void InitToneCombo()
        {
            ToneCombo.Items.Add("");

            foreach (string tone in _systemMessageUtil.AvailableTones())
            {
                ToneCombo.Items.Add(tone);
            }
        }

        private void OnWindowState(WindowEventMessage m)
        {
            if (m.Type == WindowType.Setting)
            {
                if (m.State == WindowEventType.DoClose)
                {
                    Close();
                }
                else if (m.State == WindowEventType.Closing)
                {
                    WeakReferenceMessenger.Default.Unregister<WindowEventMessage>(this);
                }                
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Closing, WindowType.Setting));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.Loaded, WindowType.Setting));
        }
    }
}
