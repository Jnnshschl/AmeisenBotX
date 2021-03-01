using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicIntellectComparator : BasicComparator
    {
        public BasicIntellectComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new(new()
            {
                { WowStatType.INTELLECT, 2.5 },
                { WowStatType.SPELL_POWER, 2.5 },
                { WowStatType.ARMOR, 2.0 },
                { WowStatType.MP5, 2.0 },
                { WowStatType.HASTE, 2.0 },
            });
        }
    }
}