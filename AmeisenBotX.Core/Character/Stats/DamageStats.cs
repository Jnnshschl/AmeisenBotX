using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Stats
{
    public class DamageStats
    {
        [JsonProperty("dps")]
        public double Dps { get; set; }

        [JsonProperty("maxDamage")]
        public double MaxDamage { get; set; }

        [JsonProperty("minDamage")]
        public double MinDamage { get; set; }
    }
}
