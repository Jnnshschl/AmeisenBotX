using AmeisenBotX.Core.Engines.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Engines.Character.Comparators
{
    public interface IItemComparator
    {
        bool IsBetter(IWowItem current, IWowItem item);

        bool IsBlacklistedItem(IWowItem item);
    }
}