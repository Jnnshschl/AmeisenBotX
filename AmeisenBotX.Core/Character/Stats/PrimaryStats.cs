using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Stats
{
    public class PrimaryStats
    {
        [JsonProperty("agility")]
        public double Agility { get; set; }

        [JsonProperty("attackpower")]
        public double Attackpower { get; set; }

        [JsonProperty("intellect")]
        public double Intellect { get; set; }

        [JsonProperty("mana")]
        public double Mana { get; set; }

        [JsonProperty("spellpower")]
        public double Spellpower { get; set; }

        [JsonProperty("spirit")]
        public double Spirit { get; set; }

        [JsonProperty("stamina")]
        public double Stamina { get; set; }

        [JsonProperty("strenght")]
        public double Strenght { get; set; }
    }
}
