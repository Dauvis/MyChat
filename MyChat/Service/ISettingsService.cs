using MyChat.Model;

namespace MyChat.Service
{
    public interface ISettingsService
    {
        UserSettings GetUserSettings();
        void SetUserSettings(UserSettings userSettings);
    }
}