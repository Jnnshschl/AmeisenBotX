using System;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public interface IIdleAction
    {
        bool AutopilotOnly { get; }

        DateTime Cooldown { get; set; }

        int MaxCooldown { get; }

        int MaxDuration { get; }

        int MinCooldown { get; }

        int MinDuration { get; }

        bool Enter();

        void Execute();
    }
}