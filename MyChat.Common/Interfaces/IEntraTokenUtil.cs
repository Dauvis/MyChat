using MyChat.Common.DTO;

namespace MyChat.Common.Interfaces
{
    public interface IEntraTokenUtil
    {
        Task<UserProfileDocumentDTO?> ProfileDocumentForIdTokenAsync(string? token);
    }
}
