using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicAgilityComparator : BasicComparator
    {
        public BasicAgilityComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_AGILITY_SHORT", 3.0 },
                { "ITEM_MOD_ATTACK_POWER_SHORT", 2.0 },
                { "ITEM_MOD_CRIT_RATING_SHORT", 2.2 },
                { "RESISTANCE0_NAME", 2.0 },
            });
        }
    }
}