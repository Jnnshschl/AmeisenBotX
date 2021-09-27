using System;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Managers.Character.Inventory.Objects
{
    public class WowWeapon : WowBasicItem
    {
        public WowWeaponType WeaponType { get; set; }

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
            WeaponType = Enum.TryParse(GetWeaponTypeName(Subtype), out WowWeaponType weaponType) 
                ? weaponType : WowWeaponType.Misc;
        }

        private static string GetWeaponTypeName(string subType)
        {
            if (subType.StartsWith("Main Hand"))
            {
                subType = subType.Replace("Main Hand", "");

                if (subType.EndsWith("s"))
                    subType = subType.Remove(subType.Length - 1);

                return subType;
            } 
            else if (subType.StartsWith("Off Hand"))
            {
                subType = subType.Replace("Off Hand", "");

                if (subType.EndsWith("s"))
                    subType = subType.Remove(subType.Length - 1);

                return subType;
            }
            else if (subType.StartsWith("One-Handed"))
            {
                subType = subType.Replace("One-Handed", "");

                if (subType.EndsWith("s"))
                    subType = subType.Remove(subType.Length - 1);

                return subType;
            }
            else if (subType.StartsWith("One-Hand"))
            {
                subType = subType.Replace("One-Hand", "");

                if (subType.EndsWith("s"))
                    subType = subType.Remove(subType.Length - 1);

                return subType;
            }

            if (subType.Contains("-"))
            {
                string handedness = subType.Replace("-", string.Empty).Split(" ", 2)[0];
                string weaponType = subType.Replace("-", string.Empty).Split(" ", 2)[1];

                if (weaponType.EndsWith("s"))
                    weaponType = weaponType.Remove(weaponType.Length - 1);

                return weaponType + handedness;
            }

            return string.Empty;
        }
    }
}