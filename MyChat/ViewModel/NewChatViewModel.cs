﻿using CommunityToolkit.Mvvm.ComponentModel;
using MyChat.Service;

namespace MyChat.ViewModel
{
    public class NewChatViewModel : ObservableObject
    {
        private readonly ISettingsService _settings;

        private string _instructions = "";
        private string _tone = "";
        private string _topic = "";

        public NewChatViewModel(ISettingsService settings)
        {
            _settings = settings;
        }

        public string Instructions
        {
            get => _instructions;

            set => SetProperty(ref _instructions, value);
        }

        public string Tone
        {
            get => _tone;

            set => SetProperty(ref _tone, value);
        }

        public string Topic
        {
            get => _topic;

            set => SetProperty(ref _topic, value);
        }

        public void LoadUserSettings()
        {
            var userSettings = _settings.GetUserSettings();

            Instructions = userSettings.DefaultInstructions;
            Tone = userSettings.DefaultTone;
        }
    }
}
