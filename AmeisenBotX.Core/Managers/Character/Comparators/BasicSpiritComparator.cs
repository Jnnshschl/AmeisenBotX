using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class BasicSpiritComparator : BasicComparator
    {
        public BasicSpiritComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new(new()
            {
                { WowStatType.INTELLECT, 2.5 },
                { WowStatType.SPIRIT, 2.5 },
                { WowStatType.SPELL_POWER, 2.5 },
                { WowStatType.MP5, 2.0 },
                { WowStatType.ARMOR, 2.0 },
            });
        }
    }
}