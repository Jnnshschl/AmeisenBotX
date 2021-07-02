using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AmeisenBotX.Core.Engines.Character.Spells.Objects
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