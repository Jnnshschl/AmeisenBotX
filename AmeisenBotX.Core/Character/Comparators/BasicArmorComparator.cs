using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicArmorComparator : BasicComparator
    {
        public BasicArmorComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_STAMINA_SHORT", 3 },
                { "ITEM_MOD_STRENGHT_SHORT", 2 },
                { "RESISTANCE0_NAME", 1 },
            });
        }
    }
}