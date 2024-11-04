using System.Security.Claims;

namespace MyChat.Common.Interfaces
{
    public interface IJwtGenerator
    {
        string GenerateJwtToken(ClaimsPrincipal claimsPrincipal);
    }
}
