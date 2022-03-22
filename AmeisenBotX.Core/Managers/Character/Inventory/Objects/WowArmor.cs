using AmeisenBotX.Wow.Objects.Enums;
using System;

namespace AmeisenBotX.Core.Managers.Character.Inventory.Objects
{
    public class WowArmor : WowBasicItem
    {
        public WowArmor(IWowInventoryItem wowBasicItem) : base(wowBasicItem)
        {
            if (Subtype.ToLowerInvariant().EndsWith("s"))
            {
                Subtype = Subtype.Remove(Subtype.Length - 1);
            }

            ArmorType = Enum.TryParse(Subtype, true, out WowArmorType armorType)
                ? armorType : WowArmorType.Misc;
        }

        public WowArmorType ArmorType { get; }
    }
}