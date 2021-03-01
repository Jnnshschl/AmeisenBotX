using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicStrengthComparator : BasicComparator
    {
        public BasicStrengthComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new(new()
            {
                { WowStatType.STRENGTH, 3.0 },
                { WowStatType.ATTACK_POWER, 3.0 },
                { WowStatType.ARMOR, 2.0 },
                { WowStatType.CRIT, 2.0 },
                { WowStatType.DPS, 2.0 },
            });
        }
    }
}