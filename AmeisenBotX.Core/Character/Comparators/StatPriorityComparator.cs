using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class StatPriorityComparator : IWowItemComparator
    {
        public StatPriorityComparator()
        {

        }

        public bool IsBetter(IWowItem current, IWowItem item)
        {
            return false;
        }

        private double CalculateGearscore(IWowItem item)
        {
            double baseScore = item.ItemLevel;
            double finalScore = 0;



            return 0.0;
        }
    }
}
