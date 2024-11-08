
using Newtonsoft.Json;

namespace MyChat.Data.Models
{
    public class UserProfileDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = "";
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; } = "";
        public string AuthUserId { get; set; } = "";

        public UserInfo UserInfo { get; set; } = new();

    }
}
