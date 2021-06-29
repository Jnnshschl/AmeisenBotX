﻿using AmeisenBotX.Core.Fsm.Utils.Auras.Objects;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Utils.Aura
{
    public class AuraManager
    {
        public AuraManager(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Jobs = new();
        }

        public List<IAuraJob> Jobs { get; set; }

        public WowInterface WowInterface { get; }

        public bool Tick(IEnumerable<WowAura> auras)
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