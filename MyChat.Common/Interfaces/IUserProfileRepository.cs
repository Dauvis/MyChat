using MyChat.Common.DTO;

namespace MyChat.Common.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<UserProfileDocumentDTO?> CreateAsync(UserInfoDTO userInfoDto, string authSource, string authUserId);
        Task<UserProfileDocumentDTO?> GetAsync(string userId);
        Task<UserProfileDocumentDTO?> GetByAuthUserIdAsync(string authUserId);
        Task<UserInfoDTO?> GetUserAsync(string userId);
        Task<UserInfoDTO?> GetUserByAuthUserIdAsync(string authUserId);
    }
}
