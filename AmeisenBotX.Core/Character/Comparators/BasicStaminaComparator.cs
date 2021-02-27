using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicStaminaComparator : BasicComparator
    {
        public BasicStaminaComparator(List<WowArmorType> armorTypeBlacklist = null, List<WowWeaponType> weaponTypeBlacklist = null) : base(armorTypeBlacklist, weaponTypeBlacklist)
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { WowStatType.STAMINA, 4.0 },
                { WowStatType.STRENGTH, 2.5 },
                { WowStatType.ARMOR, 2.0 },
            });
        }
    }
}