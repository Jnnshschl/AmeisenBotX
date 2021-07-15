using System.Text.Json.Serialization;

namespace AmeisenBotX.Wow.Objects
{
    public class WowMount
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("mountId")]
        public int MountId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("spellId")]
        public int SpellId { get; set; }
    }
}