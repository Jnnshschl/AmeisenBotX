using AmeisenBotX.Core.Engines.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Engines.Character.Comparators
{
    public class ItemLevelComparator : IItemComparator
    {
        public bool IsBetter(IWowItem current, IWowItem item)
        {
            return current == null || current.ItemLevel < item.ItemLevel;
        }

        public bool IsBlacklistedItem(IWowItem item)
        {
            return false;
        }
    }
}