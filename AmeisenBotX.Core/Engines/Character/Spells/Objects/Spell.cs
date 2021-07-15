using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AmeisenBotX.Core.Engines.Character.Spells.Objects
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
            Match result = Regex.Match(Rank, @"\d+");

            if (result.Success && int.TryParse(result.Value, out rank))
            {
                return true;
            }

            rank = 0;
            return false;
        }
    }
}