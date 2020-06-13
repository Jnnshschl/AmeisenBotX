using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicSpiritComparator : BasicComparator
    {
        public BasicSpiritComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_INTELLECT_SHORT", 8 },
                { "ITEM_MOD_SPIRIT_SHORT ", 5 },
                { "ITEM_MOD_SPELL_POWER_SHORT ", 3 },
                { "ITEM_MOD_POWER_REGEN0_SHORT ", 2 },
                { "RESISTANCE0_NAME", 1 },
            });
        }
    }
}