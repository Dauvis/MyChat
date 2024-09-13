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

        private string _defaultCustomInstructions = string.Empty;
        private string _selectedModel = string.Empty;
        private string _defaultTone = string.Empty;

        public ICommand OkButtonClickedCommand { get; }
        public ICommand CancelButtonClickedCommand { get; }

        public SettingsViewModel(ISettingsService settings, IGPTService gptService)
        {
            _settings = settings;
            _gptService = gptService;
            OkButtonClickedCommand = new RelayCommand(OnOkButtonClicked);
            CancelButtonClickedCommand = new RelayCommand(OnCancelButtonClicked);
        }

        public string DefaultCustomInstructions
        {
            get => _defaultCustomInstructions;

            set => SetProperty(ref _defaultCustomInstructions, value);
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
            WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
        }

        private void OnCancelButtonClicked()
        {
            WeakReferenceMessenger.Default.Send(new CloseWindowMessage());
        }

        public void LoadUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            DefaultCustomInstructions = userSettings.DefaultCustomInstructions;
            SelectedModel = userSettings.SelectedChatModel;
            DefaultTone = userSettings.DefaultTone;
        }

        public void SaveUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            userSettings.DefaultCustomInstructions = _defaultCustomInstructions;
            userSettings.SelectedChatModel = _selectedModel;
            userSettings.DefaultTone = _defaultTone;

            _settings.SetUserSettings(userSettings);
            _gptService.ChangeChatModel(userSettings.SelectedChatModel);
        }
    }
}
