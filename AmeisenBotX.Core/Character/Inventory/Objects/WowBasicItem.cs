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
                BagId = item.BagId;
                BagSlot = item.BagSlot;
                Count = Count;
                Durability = item.Durability;
                EquipLocation = item.EquipLocation;
                EquipSlot = item.EquipSlot;
                Id = item.Id;
                ItemLevel = item.ItemLevel;
                ItemLink = item.ItemLink;
                ItemQuality = item.ItemQuality;
                MaxDurability = item.MaxDurability;
                MaxStack = item.MaxStack;
                Name = item.Name;
                Price = item.Price;
                RequiredLevel = item.RequiredLevel;
                Stats = item.Stats;
                Subtype = item.Subtype;
                Type = item.Type;
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