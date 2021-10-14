using AmeisenBotX.Wow.Objects.Raw.SubStructs;
using System.Collections.Generic;

namespace AmeisenBotX.Wow.Objects
{
    public interface IWowItem : IWowObject
    {
        int Count { get; }

        List<ItemEnchantment> ItemEnchantments { get; }

        ulong Owner { get; }

        IEnumerable<string> GetEnchantmentStrings();
    }
}