using AmeisenBotX.Core.Character.Inventory.Enums;
using Newtonsoft.Json;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowBasicItem : IWowItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("link")]
        public string ItemLink { get; set; }

        [JsonProperty("equipslot")]
        public EquipmentSlot EquipSlot { get; set; } = EquipmentSlot.NOT_EQUIPABLE;

        [JsonProperty("quality")]
        public ItemQuality ItemQuality { get; set; }

        [JsonProperty("level")]
        public int ItemLevel { get; set; }

        [JsonProperty("minLevel")]
        public int RequiredLevel { get; set; }

        [JsonProperty("sellprice")]
        public int Price { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("maxStack")]
        public int MaxStack { get; set; }

        [JsonProperty("curDurability")]
        public int Durability { get; set; }

        [JsonProperty("maxDurability")]
        public int MaxDurability { get; set; }

        [JsonProperty("equiplocation")]
        public string EquipLocation { get; set; }
    }
}
