using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Stats
{
    public class Resistances
    {
        [JsonProperty("armor")]
        public double Armor { get; set; }

        [JsonProperty("arcane")]
        public double Arcane { get; set; }

        [JsonProperty("fire")]
        public double Fire { get; set; }

        [JsonProperty("frost")]
        public double Frost { get; set; }

        [JsonProperty("nature")]
        public double Nature { get; set; }

        [JsonProperty("shadow")]
        public double Shadow { get; set; }
    }
}
