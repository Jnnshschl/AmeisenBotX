using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicIntellectComparator : IWowItemComparator
    {
        public BasicIntellectComparator()
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_INTELLECT_SHORT", 8 },
                { "ITEM_MOD_SPELL_POWER_SHORT", 5 },
                { "RESISTANCE0_NAME", 3 },
                { "ITEM_MOD_POWER_REGEN0_SHORT", 2 },
                { "ITEM_MOD_HASTE_RATING_SHORT", 1 },
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
