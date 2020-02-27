using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicStrengthComparator : IWowItemComparator
    {
        public BasicStrengthComparator()
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_STRENGTH_SHORT", 8 },
                { "ITEM_MOD_ATTACK_POWER_SHORT", 5 },
                { "RESISTANCE0_NAME", 3 },
                { "ITEM_MOD_CRIT_RATING_SHORT", 2 },
                { "ITEM_MOD_DAMAGE_PER_SECOND_SHORT", 1 },
            });
        }

        private GearscoreFactory GearscoreFactory { get; }

        public bool IsBetter(IWowItem current, IWowItem item)
        {
            double scoreCurrent = GearscoreFactory.Calculate(current);
            double scoreNew = GearscoreFactory.Calculate(item);
            return scoreCurrent < scoreNew;
        }
    }
}