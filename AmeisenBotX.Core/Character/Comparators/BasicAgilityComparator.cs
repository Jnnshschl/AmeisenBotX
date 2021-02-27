using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicAgilityComparator : BasicComparator
    {
        public BasicAgilityComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { WowStatType.AGILITY, 3.0 },
                { WowStatType.ATTACK_POWER, 2.0 },
                { WowStatType.CRIT, 2.2 },
                { WowStatType.ARMOR, 2.0 },
            });
        }
    }
}