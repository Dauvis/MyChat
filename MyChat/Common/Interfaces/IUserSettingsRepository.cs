using MyChat.Common.Model;

namespace MyChat.Common.Interfaces
{
    public interface IUserSettingsRepository
    {
        UserSettings Fetch();
        void Update(UserSettings settings);
    }
}