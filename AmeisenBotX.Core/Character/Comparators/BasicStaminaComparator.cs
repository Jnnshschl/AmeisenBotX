using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicStaminaComparator : BasicComparator
    {
        public BasicStaminaComparator(List<ArmorType> armorTypeBlacklist = null, List<WeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "RESISTANCE0_NAME", 3 },
                { "ITEM_MOD_STRENGHT_SHORT", 2 },
                { "ITEM_MOD_STAMINA_SHORT", 1 },
            });
        }
    }
}