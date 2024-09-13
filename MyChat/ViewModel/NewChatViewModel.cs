using CommunityToolkit.Mvvm.ComponentModel;
using MyChat.Service;

namespace MyChat.ViewModel
{
    public class NewChatViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;

        private string _customInstructions = string.Empty;
        private string _tone = string.Empty;

        public NewChatViewModel(ISettingsService settings)
        {
            _settings = settings;
        }

        public string CustomInstructions
        {
            get => _customInstructions;

            set => SetProperty(ref _customInstructions, value);
        }

        public string Tone
        {
            get => _tone;

            set => SetProperty(ref _tone, value);
        }

        public void LoadUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            CustomInstructions = userSettings.DefaultCustomInstructions;
            Tone = userSettings.DefaultTone;
        }
    }
}
