using System.Text.Json.Serialization;

namespace AmeisenBotX.RconClient.Messages
{
    public class ImageMessage
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }
}