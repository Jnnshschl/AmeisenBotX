using AmeisenBotX.Core.Character.Inventory.Objects;

namespace AmeisenBotX.Core.Character.Comparators
{
    public interface IWowItemComparator
    {
        bool IsBetter(IWowItem current, IWowItem item);
    }
}
