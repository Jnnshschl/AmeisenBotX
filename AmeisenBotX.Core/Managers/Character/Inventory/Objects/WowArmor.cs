using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Managers.Character.Inventory.Objects
{
    public class WowArmor : WowBasicItem
    {
        public WowArmorType ArmorType { get; }

        public WowArmor(IWowInventoryItem wowBasicItem) : base(wowBasicItem)
        {
            Id = wowBasicItem.Id;
            BagId = wowBasicItem.BagId;
            BagSlot = wowBasicItem.BagSlot;
            Type = wowBasicItem.Type;
            Subtype = wowBasicItem.Subtype;
            Name = wowBasicItem.Name;
            ItemLink = wowBasicItem.ItemLink;
            EquipSlot = wowBasicItem.EquipSlot;
            ItemQuality = wowBasicItem.ItemQuality;
            ItemLevel = wowBasicItem.ItemLevel;
            RequiredLevel = wowBasicItem.RequiredLevel;
            Price = wowBasicItem.Price;
            Count = wowBasicItem.Count;
            MaxStack = wowBasicItem.MaxStack;
            Durability = wowBasicItem.Durability;
            MaxDurability = wowBasicItem.MaxDurability;
            EquipLocation = wowBasicItem.EquipLocation;

            if (Subtype.EndsWith("s"))
            {
                Subtype = Subtype.Remove(Subtype.Length - 1);
            }

            ArmorType = Enum.TryParse(Subtype, out WowArmorType armorType)
                ? armorType : WowArmorType.Misc;
        }
    }
}