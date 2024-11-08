using System.Text.Json.Serialization;

namespace MyChat.Data.Models
{
    public class UserInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
