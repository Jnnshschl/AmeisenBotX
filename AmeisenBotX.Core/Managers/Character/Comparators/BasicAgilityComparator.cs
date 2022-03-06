using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class BasicAgilityComparator : BasicComparator
    {
        public BasicAgilityComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new(new()
            {
                { WowStatType.AGILITY, 3.0 },
                { WowStatType.ATTACK_POWER, 2.0 },
                { WowStatType.CRIT, 2.2 },
                { WowStatType.ARMOR, 2.0 },
            });
        }
    }
}