using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Objects
{
    public class WowMount
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("spellId")]
        public int SpellId { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }
    }
}