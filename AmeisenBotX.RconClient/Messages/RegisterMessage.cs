using System.Text.Json.Serialization;

namespace AmeisenBotX.RconClient.Messages
{
    public class RegisterMessage
    {
        [JsonPropertyName("class")]
        public string Class { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("race")]
        public string Race { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}