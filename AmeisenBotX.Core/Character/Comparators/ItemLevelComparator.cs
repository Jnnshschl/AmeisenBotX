using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
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