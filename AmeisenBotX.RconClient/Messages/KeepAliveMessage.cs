using System.Text.Json.Serialization;

namespace AmeisenBotX.RconClient.Messages
{
    public class KeepAliveMessage
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; }
    }
}