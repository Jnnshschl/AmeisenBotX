using AmeisenBotX.Wow.Objects.SubStructs;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects
{
    public interface IWowItem : IWowObject
    {
        int Count { get; }

        List<ItemEnchantment> ItemEnchantments { get; }

        ulong Owner { get; }

        IEnumerable<string> GetEnchantmentStrings();
    }
}