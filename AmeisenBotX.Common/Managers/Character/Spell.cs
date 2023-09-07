using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Managers.Character.Spells.Objects
{
    public class Spell
    {
        [JsonPropertyName("castTime")]
        public int CastTime { get; set; }

        [JsonPropertyName("costs")]
        public int Costs { get; set; }

        [JsonPropertyName("maxRange")]
        public int MaxRange { get; set; }

        [JsonPropertyName("minRange")]
        public int MinRange { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [JsonPropertyName("spellbookId")]
        public int SpellbookId { get; set; }

        [JsonPropertyName("spellBookName")]
        public string SpellbookName { get; set; }

        public bool TryGetRank(out int rank)
        {
            return int.TryParse(Rank, out rank);
        }
    }
}