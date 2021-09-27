using System.Collections.Generic;
using AmeisenBotX.Core.Managers.Character.Comparators.Objects;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class ShamanElementalComparator : BasicComparator
    {
        public ShamanElementalComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>
            {
                { WowStatType.HIT, 3.0 },         // Hard-cap 17%
                { WowStatType.HASTE, 3.0 },       // Soft-cap 1000 - 1200
                { WowStatType.CRIT, 2.5 },        // Soft-cap 48.3%
                { WowStatType.SPELL_POWER, 2.5 }, // uncapped
                { WowStatType.INTELLECT, 2.0 },   // uncapped
                { WowStatType.MP5, 2.0 },         // uncapped
                { WowStatType.STAMINA, 1.5 },     // uncapped
                { WowStatType.SPIRIT, 1.5 }       // uncapped
            });
        }
    }
}