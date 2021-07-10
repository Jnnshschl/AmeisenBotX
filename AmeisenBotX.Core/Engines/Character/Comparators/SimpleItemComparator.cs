using AmeisenBotX.Core.Engines.Character.Comparators.Objects;
using AmeisenBotX.Core.Engines.Character.Inventory.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Character.Comparators
{
    public class SimpleItemComparator : IItemComparator
    {
        public SimpleItemComparator(DefaultCharacterManager characterManager, Dictionary<string, double> statPriorities)
        {
            // This introduces a cyclic dependency. Are we fine with that?
            CharacterManager = characterManager;
            GearscoreFactory = new(statPriorities);
        }

        protected GearscoreFactory GearscoreFactory { get; set; }

        private DefaultCharacterManager CharacterManager { get; }

        public bool IsBetter(IWowInventoryItem current, IWowInventoryItem item)
        {
            if (!CharacterManager.IsAbleToUseItem(item))
            {
                return false;
            }

            double scoreCurrent = GearscoreFactory.Calculate(current);
            double scoreNew = GearscoreFactory.Calculate(item);
            return scoreCurrent < scoreNew;
        }

        public bool IsBlacklistedItem(IWowInventoryItem item)
        {
            return !CharacterManager.IsAbleToUseItem(item);
        }
    }
}