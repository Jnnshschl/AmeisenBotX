namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public interface ICombatClass
    {
        bool HandlesMovement { get; }

        void Execute();
    }
}