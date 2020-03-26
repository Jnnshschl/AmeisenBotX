using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicIntellectComparator : BasicComparator
    {
        public BasicIntellectComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_INTELLECT_SHORT", 8 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 5 },
                { "RESISTANCE0_NAME", 3 },
                { "ITEM_MOD_POWER_REGEN0_SHORT", 2 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 1 },
            });
        }
    }
}