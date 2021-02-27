using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public interface IItemComparator
    {
        bool IsBetter(IWowItem current, IWowItem item);

        bool IsBlacklistedItem(IWowItem item);
    }
}