using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Managers.Character.Inventory.Objects
{
    public class WowWeapon : WowBasicItem
    {
        public WowWeapon(IWowInventoryItem wowBasicItem) : base(wowBasicItem)
        {
            WeaponType = Enum.TryParse(GetWeaponTypeName(Subtype.ToLowerInvariant()), true, out WowWeaponType weaponType)
                ? weaponType : WowWeaponType.Misc;
        }

        public WowWeaponType WeaponType { get; set; }

        private static string GetWeaponTypeName(string subType)
        {
            if (subType.StartsWith("Main Hand"))
            {
                subType = subType.Replace("Main Hand", "");

                if (subType.EndsWith("s"))
                {
                    subType = subType.Remove(subType.Length - 1);
                }

                return subType;
            }

            if (subType.StartsWith("Off Hand"))
            {
                subType = subType.Replace("Off Hand", "");

                if (subType.EndsWith("s"))
                {
                    subType = subType.Remove(subType.Length - 1);
                }

                return subType;
            }

            if (subType.StartsWith("One-Handed"))
            {
                subType = subType.Replace("One-Handed", "");

                if (subType.EndsWith("s"))
                {
                    subType = subType.Remove(subType.Length - 1);
                }

                return subType;
            }

            if (subType.StartsWith("One-Hand"))
            {
                subType = subType.Replace("One-Hand", "");

                if (subType.EndsWith("s"))
                {
                    subType = subType.Remove(subType.Length - 1);
                }

                return subType;
            }

            if (subType.StartsWith("Staves"))
            {
                return "Staff";
            }

            if (subType.Contains('-'))
            {
                string handedness = subType.Replace("-", string.Empty).Split(" ", 2)[0];
                string weaponType = subType.Replace("-", string.Empty).Split(" ", 2)[1];

                if (weaponType.EndsWith("s"))
                {
                    weaponType = weaponType.Remove(weaponType.Length - 1);
                }

                if (handedness.EndsWith("ed"))
                {
                    handedness = handedness.Remove(handedness.Length - 2);
                }

                return weaponType + handedness;
            }

            if (subType.EndsWith("s"))
            {
                return subType.Remove(subType.Length - 1);
            }

            return subType;
        }
    }
}