using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Utils.Aura
{
    public class AuraManager
    {
        public AuraManager(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Jobs = new();
        }

        public List<IAuraJob> Jobs { get; set; }

        public AmeisenBotInterfaces Bot { get; }

        public bool Tick(IEnumerable<RawWowAura> auras)
        {
            foreach (IAuraJob job in Jobs)
            {
                if (job.Run(auras))
                {
                    return true;
                }
            }

            return false;
        }
    }
}