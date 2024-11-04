using MyChat.Common.DTO;
using MyChat.Common.Enums;
using MyChat.Common.Interfaces;
using System.Security.Claims;

namespace MyChat.Logic.Services
{
    public class UserService: IUserService
    {
        private readonly IIdentityTokenValidator _tokenValidator;
        private readonly IJwtGenerator _jwtGenerator;

        public UserService(IIdentityTokenValidator tokenValidator, IJwtGenerator jwtGenerator) 
        {
            _tokenValidator = tokenValidator;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthenticationResponseDTO> AuthenticateAsync(AuthenticationRequestDTO request)
        {
            var authProviderUserId = await _tokenValidator.ValidateTokenEntraAsync(request.IdentityToken);

            if (authProviderUserId is null)
            {
                return new AuthenticationResponseDTO(request.AuthProvider, "")
                {
                    ErrorCode = ErrorCodesType.AuthenticationFailed,
                    ErrorMessage = "Identity was not able to be verified."
                };
            }
            else
            {
                var jwt = _jwtGenerator.GenerateJwtToken(CreateClaimsPrincipal(authProviderUserId));

                return new AuthenticationResponseDTO(request.AuthProvider, jwt);
            }            
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(string authProviderUserId)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "8675309"),
                new(ClaimTypes.Name, "John Doe"),
                new(ClaimTypes.Email, "jdoe@example.com")
            };

            // TODO: Add user roles as claims, if any

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }
    }
}
