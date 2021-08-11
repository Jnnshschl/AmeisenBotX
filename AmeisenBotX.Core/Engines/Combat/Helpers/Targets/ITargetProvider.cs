﻿using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Combat.Helpers.Targets
{
    public interface ITargetProvider
    {
        IEnumerable<int> BlacklistedTargets { get; set; }

        IEnumerable<int> PriorityTargets { get; set; }

        bool Get(out IEnumerable<IWowUnit> possibleTargets);
    }
}