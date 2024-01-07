using AmeisenBotX.Core.Managers.Character.Comparators.Objects;
using AmeisenBotX.Core.Managers.Character.Inventory.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public class SimpleItemComparator(DefaultCharacterManager characterManager, Dictionary<string, double> statPriorities) : IItemComparator
    {
        protected GearscoreFactory GearscoreFactory { get; set; } = new(statPriorities);

        private DefaultCharacterManager CharacterManager { get; } = characterManager;

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