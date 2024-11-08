using MyChat.Common.Enums;

namespace MyChat.Common.DTO
{
    public class AuthenticationResponseDTO
    {
        public AuthenticationResponseDTO(AuthenticationProvidersType authProvider, string jwt)
        {
            AuthProvider = authProvider;
            Jwt = jwt;
        }

        public AuthenticationProvidersType AuthProvider { get; set; }
        public string Jwt { get; set; }
        public ErrorCodesType ErrorCode { get; set; } = ErrorCodesType.None;
        public string ErrorMessage { get; set; } = "";
    }
}
