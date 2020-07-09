using Newtonsoft.Json;

namespace AmeisenBotX.RconClient.Messages
{
    public class KeepAliveMessage
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }
    }
}