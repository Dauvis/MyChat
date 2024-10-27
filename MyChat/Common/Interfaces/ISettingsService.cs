using MyChat.Common.Model;

namespace MyChat.Common.Interfaces
{
    public interface ISettingsService
    {
        UserSettings GetUserSettings();
        void SetUserSettings(UserSettings userSettings);
    }
}