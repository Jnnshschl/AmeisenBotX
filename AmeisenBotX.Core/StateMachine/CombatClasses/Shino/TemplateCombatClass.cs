using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.CombatClasses.Jannis;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Shino
{
    public abstract class TemplateCombatClass : BasicCombatClass
    {
        public TemplateCombatClass(AmeisenBotStateMachine stateMachine) : base(stateMachine)
        {
        }

        public string Author { get; } = "Shino";

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }

        protected abstract Spell GetOpeningSpell();

        public override void AttackTarget()
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

        protected bool SelectTarget(out WowUnit target)
        {
            if (WowInterface.ObjectManager.TargetGuid != 0 && WowInterface.ObjectManager.Target != null
               && (WowInterface.ObjectManager.Target.IsDead
                   || WowInterface.ObjectManager.Target.IsNotAttackable
                   || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                   || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, WowInterface.ObjectManager.Target) == WowUnitReaction.Friendly))
            {
                WowInterface.HookManager.WowClearTarget();
                target = null;
                return false;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                target = WowInterface.ObjectManager.Target;
                return true;
            }

            IEnumerable<WowUnit> nearAttackingEnemies = WowInterface.ObjectManager
                .GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 64.0);

            if (nearAttackingEnemies.Any())
            {
                target = nearAttackingEnemies.FirstOrDefault();
                WowInterface.HookManager.WowTargetGuid(target.Guid);
                return true;
            }

            WowInterface.HookManager.WowClearTarget();
            target = null;
            return false;
        }
    }
}
