using AmeisenBotX.Core.Engines.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Engines.Character.Comparators
{
    public interface IItemComparator
    {
        bool IsBetter(IWowInventoryItem current, IWowInventoryItem item);

        bool IsBlacklistedItem(IWowInventoryItem item);
    }
}