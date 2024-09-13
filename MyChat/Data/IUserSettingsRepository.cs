using MyChat.Model;

namespace MyChat
{
    public interface IUserSettingsRepository
    {
        UserSettings Fetch();
        void Update(UserSettings settings);
    }
}