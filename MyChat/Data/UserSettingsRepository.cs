using MyChat.Common;
using MyChat.Common.Interfaces;
using MyChat.Common.Model;
using System.IO;
using System.Text.Json;

namespace MyChat
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private string _userSettingsPath = "";

        public UserSettings Fetch()
        {
            string settingsFilePath = GetUserSettingsPath();

            if (File.Exists(settingsFilePath))
            {
                string settingsJson = File.ReadAllText(settingsFilePath);
                return JsonSerializer.Deserialize<UserSettings>(settingsJson) ?? new UserSettings();
            }

            return new UserSettings();
        }

        public void Update(UserSettings settings)
        {
            string settingsFilePath = GetUserSettingsPath();
            string settingsJson = JsonSerializer.Serialize(settings);
            File.WriteAllText(settingsFilePath, settingsJson);
        }

        private string GetUserSettingsPath()
        {
            if (string.IsNullOrEmpty(_userSettingsPath))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string userAppFolder = Path.Combine(appDataPath, Constants.UserAppFolderName);
                Directory.CreateDirectory(userAppFolder);
                _userSettingsPath = Path.Combine(userAppFolder, Constants.UserSettingsFileName);
            }

            return _userSettingsPath;
        }
    }
}
