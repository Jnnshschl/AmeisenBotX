using AmeisenBotX.Core.Managers.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Managers.Character.Comparators
{
    public interface IItemComparator
    {
        bool IsBetter(IWowInventoryItem current, IWowInventoryItem item);

        bool IsBlacklistedItem(IWowInventoryItem item);
    }
}