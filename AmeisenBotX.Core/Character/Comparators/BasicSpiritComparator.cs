using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class BasicSpiritComparator : IWowItemComparator
    {
        public BasicSpiritComparator()
        {
            GearscoreFactory = new GearscoreFactory(new Dictionary<string, double>() {
                { "ITEM_MOD_INTELLECT_SHORT", 8 },
                { "ITEM_MOD_SPIRIT_SHORT ", 5 },
                { "ITEM_MOD_SPELL_POWER_SHORT ", 3 },
                { "ITEM_MOD_POWER_REGEN0_SHORT ", 2 },
                { "RESISTANCE0_NAME", 1 },
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
