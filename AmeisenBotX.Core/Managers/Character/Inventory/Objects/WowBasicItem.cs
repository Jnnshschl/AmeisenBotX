using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmeisenBotX.Core.Managers.Character.Inventory.Objects
{
    public class WowBasicItem : IWowInventoryItem
    {
        public WowBasicItem()
        {
        }

        protected WowBasicItem(IWowInventoryItem item)
        {
            if (item == null)
            {
                return;
            }

            BagId = item.BagId;
            BagSlot = item.BagSlot;
            Count = item.Count;
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

        [JsonPropertyName("bagid")]
        public int BagId { get; set; }

        [JsonPropertyName("bagslot")]
        public int BagSlot { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("curDurability")]
        public int Durability { get; set; }

        [JsonPropertyName("equiplocation")]
        public string EquipLocation { get; set; }

        [JsonPropertyName("equipslot")]
        public int EquipSlot { get; set; } = -1;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("level")]
        public int ItemLevel { get; set; }

        [JsonPropertyName("link")]
        public string ItemLink { get; set; }

        [JsonPropertyName("quality")]
        public int ItemQuality { get; set; }

        [JsonPropertyName("maxDurability")]
        public int MaxDurability { get; set; }

        [JsonPropertyName("maxStack")]
        public int MaxStack { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sellprice")]
        public int Price { get; set; }

        [JsonPropertyName("minLevel")]
        public int RequiredLevel { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, string> Stats { get; set; }

        [JsonPropertyName("subtype")]
        public string Subtype { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        public override string ToString()
        {
            return $"[{BagId}][{BagSlot}] - [{ItemQuality}][{Type}] {Name} (ilvl. {ItemLevel} | lvl.{RequiredLevel} | {Subtype} | {Price})";
        }
    }
}