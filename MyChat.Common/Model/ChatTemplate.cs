using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyChat.Common.Model
{
    public class ChatTemplate : ObservableObject
    {
        private Guid _identifier = new();
        private string _name = "";
        private string _description = "";
        private string _category = "";
        private string _tone = "";
        private string _instructions = "";
        private string _topic = "";
        private bool _enableSaveDiscard;

        public Guid Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Tone
        {
            get => _tone;
            set => SetProperty(ref _tone, value);
        }

        public string Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }

        public string Topic
        {
            get => _topic;
            set => SetProperty(ref _topic, value);
        }

        [JsonIgnore]
        public bool Selected { get; set; }

        [JsonIgnore]
        public bool EnableSaveDiscard
        {
            get => _enableSaveDiscard;
            set => SetProperty(ref _enableSaveDiscard, value);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (Selected)
            {
                EnableSaveDiscard = true;
            }
        }
    }
}
