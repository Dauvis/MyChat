using MyChat.Model;
using System.IO;
using System.Text.Json;

namespace MyChat
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
#if DEBUG
        private const string userAppFolderName = "MyChat_Debug";
#else
        private const string userAppFolderName = "MyChat";
#endif

        private string _userSettingsPath = string.Empty;

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
                string userAppFolder = Path.Combine(appDataPath, userAppFolderName);
                Directory.CreateDirectory(userAppFolder);
                _userSettingsPath = Path.Combine(userAppFolder, "usersettings.json");
            }

            return _userSettingsPath;
        }
    }
}
