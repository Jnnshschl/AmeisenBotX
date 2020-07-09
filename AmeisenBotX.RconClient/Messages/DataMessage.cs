using Newtonsoft.Json;

namespace AmeisenBotX.RconClient.Messages
{
    public class DataMessage
    {
        [JsonProperty("bagslotsfree")]
        public int BagSlotsFree { get; set; }

        [JsonProperty("combatclass")]
        public string CombatClass { get; set; }

        [JsonProperty("currentprofile")]
        public string CurrentProfile { get; set; }

        [JsonProperty("energy")]
        public int Energy { get; set; }

        [JsonProperty("exp")]
        public int Exp { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("health")]
        public int Health { get; set; }

        [JsonProperty("itemlevel")]
        public int ItemLevel { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("mapname")]
        public string MapName { get; set; }

        [JsonProperty("maxenergy")]
        public int MaxEnergy { get; set; }

        [JsonProperty("maxexp")]
        public int MaxExp { get; set; }

        [JsonProperty("maxhealth")]
        public int MaxHealth { get; set; }

        [JsonProperty("money")]
        public int Money { get; set; }

        [JsonProperty("posx")]
        public float PosX { get; set; }

        [JsonProperty("posy")]
        public float PosY { get; set; }

        [JsonProperty("posz")]
        public float PosZ { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("subzonename")]
        public string SubZoneName { get; set; }

        [JsonProperty("zonename")]
        public string ZoneName { get; set; }
    }
}