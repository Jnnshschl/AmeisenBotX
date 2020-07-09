using Newtonsoft.Json;

namespace AmeisenBotX.RconClient.Messages
{
    public class RegisterMessage
    {
        [JsonProperty("class")]
        public string Class { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("race")]
        public string Race { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}