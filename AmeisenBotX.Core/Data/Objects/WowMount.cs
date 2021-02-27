using Newtonsoft.Json;

namespace AmeisenBotX.Core.Data.Objects
{
    public class WowMount
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("mountId")]
        public int MountId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("spellId")]
        public int SpellId { get; set; }
    }
}