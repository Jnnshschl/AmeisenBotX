using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Objects
{
    public class WowMount
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("spellId")]
        public int SpellId { get; set; }

        [JsonProperty("mountId")]
        public int MountId { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }
    }
}