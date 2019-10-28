using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Spells.Objects
{
    public class Spell
    {
        [JsonProperty("castTime")]
        public int CastTime { get; set; }

        [JsonProperty("costs")]
        public int Costs { get; set; }

        [JsonProperty("maxRange")]
        public int MaxRange { get; set; }

        [JsonProperty("minRange")]
        public int MinRange { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("spellbookId")]
        public int SpellbookId { get; set; }

        [JsonProperty("spellBookName")]
        public string SpellbookName { get; set; }
    }
}
