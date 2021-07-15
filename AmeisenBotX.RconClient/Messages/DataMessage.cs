using System.Text.Json.Serialization;

namespace AmeisenBotX.RconClient.Messages
{
    public class DataMessage
    {
        [JsonPropertyName("bagslotsfree")]
        public int BagSlotsFree { get; set; }

        [JsonPropertyName("combatclass")]
        public string CombatClass { get; set; }

        [JsonPropertyName("currentprofile")]
        public string CurrentProfile { get; set; }

        [JsonPropertyName("energy")]
        public int Energy { get; set; }

        [JsonPropertyName("exp")]
        public int Exp { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("health")]
        public int Health { get; set; }

        [JsonPropertyName("itemlevel")]
        public int ItemLevel { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("mapname")]
        public string MapName { get; set; }

        [JsonPropertyName("maxenergy")]
        public int MaxEnergy { get; set; }

        [JsonPropertyName("maxexp")]
        public int MaxExp { get; set; }

        [JsonPropertyName("maxhealth")]
        public int MaxHealth { get; set; }

        [JsonPropertyName("money")]
        public int Money { get; set; }

        [JsonPropertyName("posx")]
        public float PosX { get; set; }

        [JsonPropertyName("posy")]
        public float PosY { get; set; }

        [JsonPropertyName("posz")]
        public float PosZ { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("subzonename")]
        public string SubZoneName { get; set; }

        [JsonPropertyName("zonename")]
        public string ZoneName { get; set; }
    }
}