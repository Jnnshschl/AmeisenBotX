namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public interface ICombatClass
    {
        bool HandlesMovement { get; }

        bool HandlesTargetSelection { get; }

        bool IsMelee { get; }

        void Execute();

        void OutOfCombatExecute();
    }
}
