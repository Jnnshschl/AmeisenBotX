using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Engines.Character.Inventory.Objects
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
            // TODO: this will need fix :]
            WeaponType = Enum.TryParse(Subtype.Replace("-", string.Empty), out WowWeaponType armorType) ? armorType : WowWeaponType.Misc;
        }

        public WowWeaponType WeaponType { get; set; }
    }
}