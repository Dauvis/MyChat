using MyChat.Model;

namespace MyChat.Service
{
    public class SettingsService : ISettingsService
    {
        private readonly IUserSettingsRepository _userSettingsRepository;
        private UserSettings? _userSettings = null;

        public SettingsService(IUserSettingsRepository userSettingsRepository)
        {
            _userSettingsRepository = userSettingsRepository;
        }

        public UserSettings GetUserSettings()
        {
            _userSettings ??= _userSettingsRepository.Fetch();

            return _userSettings;
        }

        public void SetUserSettings(UserSettings userSettings)
        {
            _userSettingsRepository.Update(userSettings);
            _userSettings = null;
        }
    }
}
