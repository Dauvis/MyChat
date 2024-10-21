using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyChat.Messages;
using MyChat.Model;
using MyChat.Service;
using System.Windows.Input;

namespace MyChat.ViewModel
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;
        private readonly IGPTService _gptService;

        private string _defaultInstructions = "";
        private string _selectedModel = "";
        private string _defaultTone = "";

        public ICommand OkButtonClickedCommand { get; }
        public ICommand CancelButtonClickedCommand { get; }

        public SettingsViewModel(ISettingsService settings, IGPTService gptService)
        {
            _settings = settings;
            _gptService = gptService;
            OkButtonClickedCommand = new RelayCommand(OnOkButtonClicked);
            CancelButtonClickedCommand = new RelayCommand(OnCancelButtonClicked);
        }

        public string DefaultInstructions
        {
            get => _defaultInstructions;

            set => SetProperty(ref _defaultInstructions, value);
        }

        public string SelectedModel
        {
            get => _selectedModel;

            set => SetProperty(ref _selectedModel, value);
        }

        public string DefaultTone
        {
            get => _defaultTone;

            set => SetProperty(ref _defaultTone, value);
        }

        private void OnOkButtonClicked()
        {
            SaveUserSettings();
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.DoClose, WindowType.Setting));
        }

        private void OnCancelButtonClicked()
        {
            WeakReferenceMessenger.Default.Send(new WindowEventMessage(WindowEventType.DoClose, WindowType.Setting));
        }

        public void LoadUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            DefaultInstructions = userSettings.DefaultInstructions;
            SelectedModel = userSettings.SelectedChatModel;
            DefaultTone = userSettings.DefaultTone;
        }

        public void SaveUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            userSettings.DefaultInstructions = _defaultInstructions;
            userSettings.SelectedChatModel = _selectedModel;
            userSettings.DefaultTone = _defaultTone;

            _settings.SetUserSettings(userSettings);
            _gptService.ChangeChatModel(userSettings.SelectedChatModel);
        }
    }
}
