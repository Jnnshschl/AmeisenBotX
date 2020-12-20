using AmeisenBotX.Core.Character.Comparators.Objects;
using AmeisenBotX.Core.Character.Inventory.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class SimpleItemComparator : IWowItemComparator
    {
        public SimpleItemComparator(CharacterManager characterManager, Dictionary<string, double> statPriorities)
        {
            // This introduces a cyclic dependency. Are we fine with that?
            CharacterManager = characterManager;
            GearscoreFactory = new GearscoreFactory(statPriorities);
        }

        protected GearscoreFactory GearscoreFactory { get; set; }
        
        private CharacterManager CharacterManager { get; }
        
        public bool IsBetter(IWowItem current, IWowItem item)
        {
            if (!CharacterManager.IsAbleToUseItem(item))
            {
                return false;
            }

            double scoreCurrent = GearscoreFactory.Calculate(current);
            double scoreNew = GearscoreFactory.Calculate(item);
            return scoreCurrent < scoreNew;
        }

        public bool IsBlacklistedItem(IWowItem item)
        {
            return !CharacterManager.IsAbleToUseItem(item);
        }
    }
}