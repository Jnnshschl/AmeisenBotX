using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Logic.Utils.Auras.Objects
{
    public interface IAuraJob
    {
        bool Run(IEnumerable<RawWowAura> auras);
    }
}