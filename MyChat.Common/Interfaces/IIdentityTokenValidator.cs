using System.Security.Claims;

namespace MyChat.Common.Interfaces
{
    public interface IIdentityTokenValidator
    {
        ClaimsPrincipal? ValidateToken(string? token);
        Task<string?> ValidateTokenEntraAsync(string? token);
    }
}