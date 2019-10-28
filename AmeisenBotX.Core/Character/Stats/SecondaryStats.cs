using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Stats
{
    public class SecondaryStats
    {
        [JsonProperty("blockRating")]
        public double BlockRating { get; set; }

        [JsonProperty("critRating")]
        public double CritRating { get; set; }

        [JsonProperty("evadeRating")]
        public double EvadeRating { get; set; }

        [JsonProperty("hitRating")]
        public double HitRating { get; set; }

        [JsonProperty("resilience")]
        public double Resilience { get; set; }
    }
}
