using AmeisenBotX.Core.Character.Inventory.Enums;
using System;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowWeapon : WowBasicItem
    {
        public WowWeapon(WowBasicItem wowBasicItem) : base(wowBasicItem)
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
            WeaponType = Enum.TryParse(Subtype.ToUpper().Replace("-", string.Empty).Replace(" ", "_"), out WeaponType armorType) ? armorType : WeaponType.MISCELLANEOUS;
        }

        public WeaponType WeaponType { get; set; }
    }
}