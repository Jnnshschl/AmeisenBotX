using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Fsm.Utils.Auras.Objects
{
    public interface IAuraJob
    {
        bool Run(IEnumerable<WowAura> auras);
    }
}