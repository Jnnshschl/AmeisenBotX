namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public interface ICombatClass
    {
        bool HandlesMovement { get; }

        bool HandlesTargetSelection { get; }

        void Execute();
    }
}