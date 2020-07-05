using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects.Structs
{
    public interface IRawWowAuraTable
    {
        int MaxBuffs { get; }

        List<RawWowAura> AsList();
    }
}