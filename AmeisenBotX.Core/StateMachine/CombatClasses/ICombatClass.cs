using AmeisenBotX.Core.Character.Comparators;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public interface ICombatClass
    {
        bool HandlesMovement { get; }

        bool HandlesTargetSelection { get; }

        bool IsMelee { get; }

        IWowItemComparator ItemComparator { get; }

        void Execute();

        void OutOfCombatExecute();
    }
}
