using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public class ItemLevelComparator : IWowItemComparator
    {
        public bool IsBetter(IWowItem current, IWowItem item)
        {
            return current == null || current.ItemLevel < item.ItemLevel;
        }
    }
}