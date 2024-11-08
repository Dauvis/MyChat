using MyChat.Common.DTO;

namespace MyChat.Common.Interfaces
{
    public interface IUserProfileService
    {
        Task<AuthenticationResponseDTO> AuthenticateAsync(AuthenticationRequestDTO request);
        Task<UserInfoDTO?> GetAsync(string userId);
        Task<UserInfoDTO?> GetByAuthUserIdAsync(string authUserId);
        Task<UserProfileDocumentDTO?> GetProfileAsync(string userId);
        Task<UserProfileDocumentDTO?> GetProfileByAuthUserIdAsync(string authUserId);
    }
}
