using System.Collections.Generic;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class BasicStaminaComparator : BasicComparator
    {
        public BasicStaminaComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new(new()
            {
                { WowStatType.STAMINA, 4.0 },
                { WowStatType.STRENGTH, 2.5 },
                { WowStatType.ARMOR, 2.0 },
            });
        }
    }
}