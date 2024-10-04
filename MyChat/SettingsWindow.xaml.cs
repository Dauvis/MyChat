using CommunityToolkit.Mvvm.Messaging;
using MyChat.Messages;
using MyChat.Service;
using MyChat.Util;
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
            WeakReferenceMessenger.Default.Register<CloseWindowMessage>(this, (r, m) => CloseWindow());

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

        private void CloseWindow()
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<CloseWindowMessage>(this);

            base.OnClosed(e);
        }
    }
}
