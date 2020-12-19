using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.CombatClasses.Jannis;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Shino
{
    public abstract class TemplateCombatClass : BasicCombatClass
    {
        public TemplateCombatClass(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
        }

        protected abstract Spell GetOpeningSpell();

        public void AttackTarget()
        {
            WowUnit target = WowInterface.ObjectManager.Target;
            if (target == null)
            {
                return;
            }

            Spell openingSpell = GetOpeningSpell();
            if (IsInRange(openingSpell, target) && WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, target.Position))
            {
                WowInterface.HookManager.WowStopClickToMove();
                WowInterface.MovementEngine.StopMovement();
                WowInterface.MovementEngine.Reset();
                TryCastSpell(openingSpell.Name, target.Guid, openingSpell.Costs > 0);
            }
            else
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, target.Position);
            }
        }
    }
}
