using AmeisenBotX.Core.Logic.Idle.Actions;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Logic.Idle
{
    public interface IIdleActionManager
    {
        TimeSpan Cooldown { get; }
        DateTime ExecuteUntil { get; }
        IEnumerable<IIdleAction> IdleActions { get; set; }
        DateTime LastActionExecuted { get; }
        List<KeyValuePair<DateTime, IIdleAction>> LastActions { get; }

        void Reset();
        bool Tick(bool autopilotEnabled);
    }
}