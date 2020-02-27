using AmeisenBotX.Core.Character.Inventory.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowBasicItem : IWowItem
    {
        /**
         * Default Constructor
         */

        public WowBasicItem()
        {
        }

        /**
         * Copy Constructor
         */

        public WowBasicItem(WowBasicItem item)
        {
            if (item != null)
            {
                this.BagId = item.BagId;
                this.BagSlot = item.BagSlot;
                this.Count = Count;
                this.Durability = item.Durability;
                this.EquipLocation = item.EquipLocation;
                this.EquipSlot = item.EquipSlot;
                this.Id = item.Id;
                this.ItemLevel = item.ItemLevel;
                this.ItemLink = item.ItemLink;
                this.ItemQuality = item.ItemQuality;
                this.MaxDurability = item.MaxDurability;
                this.MaxStack = item.MaxStack;
                this.Name = item.Name;
                this.Price = item.Price;
                this.RequiredLevel = item.RequiredLevel;
                this.Stats = item.Stats;
                this.Subtype = item.Subtype;
                this.Type = item.Type;
            }
        }

        [JsonProperty("bagid")]
        public int BagId { get; set; }

        [JsonProperty("bagslot")]
        public int BagSlot { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("curDurability")]
        public int Durability { get; set; }

        [JsonProperty("equiplocation")]
        public string EquipLocation { get; set; }

        [JsonProperty("equipslot")]
        public EquipmentSlot EquipSlot { get; set; } = EquipmentSlot.NOT_EQUIPABLE;

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("level")]
        public int ItemLevel { get; set; }

        [JsonProperty("link")]
        public string ItemLink { get; set; }

        [JsonProperty("quality")]
        public ItemQuality ItemQuality { get; set; }

        [JsonProperty("maxDurability")]
        public int MaxDurability { get; set; }

        [JsonProperty("maxStack")]
        public int MaxStack { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sellprice")]
        public int Price { get; set; }

        [JsonProperty("minLevel")]
        public int RequiredLevel { get; set; }

        [JsonProperty("stats")]
        public Dictionary<string, string> Stats { get; set; } = new Dictionary<string, string>();

        [JsonProperty("subtype")]
        public string Subtype { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}