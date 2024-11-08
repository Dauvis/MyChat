using MyChat.Common.Enums;

namespace MyChat.Common.DTO
{
    public class UserProfileDocumentDTO
    {
        public string Id { get; set; } = "";
        public AuthenticationProvidersType AuthProvider { get; set; }
        public string AuthUserId { get; set; } = "";

        public UserInfoDTO UserInfo { get; set; } = new();

    }
}
