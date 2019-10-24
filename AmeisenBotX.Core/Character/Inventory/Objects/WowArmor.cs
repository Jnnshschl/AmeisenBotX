using AmeisenBotX.Core.Character.Inventory.Enums;
using System;

namespace AmeisenBotX.Core.Character.Inventory.Objects
{
    public class WowArmor : WowBasicItem
    {
        public ArmorType ArmorType { get; private set; }

        public WowArmor(WowBasicItem wowBasicItem) : base()
        {
            Id = wowBasicItem.Id;
            Type = wowBasicItem.Type;
            Subtype = wowBasicItem.Subtype;
            Name = wowBasicItem.Name;
            ItemLink = wowBasicItem.ItemLink;
            EquipLocation = wowBasicItem.EquipLocation;
            ItemQuality = wowBasicItem.ItemQuality;
            ItemLevel = wowBasicItem.ItemLevel;
            RequiredLevel = wowBasicItem.RequiredLevel;
            Price = wowBasicItem.Price;
            Count = wowBasicItem.Count;
            MaxStack = wowBasicItem.MaxStack;
            Durability = wowBasicItem.Durability;
            MaxDurability = wowBasicItem.MaxDurability;
            ArmorType = Enum.TryParse(Subtype.ToUpper(), out ArmorType armorType) ? armorType : ArmorType.MISCELLANEOUS;
        }
    }
}
