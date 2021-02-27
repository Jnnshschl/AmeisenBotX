namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public interface IIdleAction
    {
        bool AutopilotOnly { get; }

        int MaxCooldown { get; }

        int MaxDuration { get; }

        int MinCooldown { get; }

        int MinDuration { get; }

        bool Enter();

        void Execute();
    }
}