using System.Security.Claims;

namespace MyChat.Common.Interfaces
{
    public interface IMyChatTokenUtil
    {
        string GenerateToken(ClaimsPrincipal claimsPrincipal);
        string? ValidateToken(string? token);
    }
}
