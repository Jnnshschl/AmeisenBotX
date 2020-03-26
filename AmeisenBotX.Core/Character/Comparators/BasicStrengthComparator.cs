using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicStrengthComparator : BasicComparator
    {
        public BasicStrengthComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_STRENGTH_SHORT", 8 },
                { "ITEM_MOD_ATTACK_POWER_SHORT", 5 },
                { "RESISTANCE0_NAME", 3 },
                { "ITEM_MOD_CRIT_RATING_SHORT", 2 },
                { "ITEM_MOD_DAMAGE_PER_SECOND_SHORT", 1 },
            });
        }
    }
}