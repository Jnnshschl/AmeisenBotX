using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Combat.Classes.Jannis;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.CombatClasses.Shino
{
    public abstract class TemplateCombatClass : BasicCombatClass
    {
        public TemplateCombatClass(WowInterface wowInterface, AmeisenBotFsm stateMachine) : base(wowInterface, stateMachine)
        {
            //this line cause a bug because it run out of index
            //WowInterface.EventHookManager.Subscribe("UI_ERROR_MESSAGE", (t, a) => OnUIErrorMessage(a[0]));
        }

        public new string Author { get; } = "Shino";

        private DateTime LastFailedOpener { get; set; } = DateTime.Now;

        public override void AttackTarget()
        {
            WowUnit target = WowInterface.ObjectManager.Target;
            if (target == null)
            {
                return;
            }

            if (IsTargetAttackable(target))
            {
                Spell openingSpell = GetOpeningSpell();
                WowInterface.HookManager.WowStopClickToMove();
                WowInterface.MovementEngine.StopMovement();
                WowInterface.MovementEngine.Reset();
                TryCastSpell(openingSpell.Name, target.Guid, openingSpell.Costs > 0);
            }
            else if (WowInterface.Player.Position.GetDistance(target.Position) < 3.5f || WowInterface.MovementEngine.Status == MovementAction.None)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public override bool IsTargetAttackable(WowUnit target)
        {
            Spell openingSpell = GetOpeningSpell();
            float posOffset = 0.5f;
            Vector3 currentPos = WowInterface.ObjectManager.Player.Position;
            Vector3 posXLeft = WowInterface.ObjectManager.Player.Position;
            posXLeft.X -= posOffset;
            Vector3 posXRight = WowInterface.ObjectManager.Player.Position;
            posXRight.X += posOffset;
            Vector3 posYRight = WowInterface.ObjectManager.Player.Position;
            posYRight.Y += posOffset;
            Vector3 posYLeft = WowInterface.ObjectManager.Player.Position;
            posYLeft.Y -= posOffset;

            return IsInRange(openingSpell, target)
                    && DateTime.Now.Subtract(LastFailedOpener).TotalSeconds > 3
                    && WowInterface.HookManager.WowIsInLineOfSight(currentPos, target.Position)
                    && WowInterface.HookManager.WowIsInLineOfSight(posXLeft, target.Position)
                    && WowInterface.HookManager.WowIsInLineOfSight(posXRight, target.Position)
                    && WowInterface.HookManager.WowIsInLineOfSight(posYRight, target.Position)
                    && WowInterface.HookManager.WowIsInLineOfSight(posYLeft, target.Position);
        }

        public void OnUIErrorMessage(string message)
        {
            if (string.Equals(message, "target not in line of sight", StringComparison.InvariantCultureIgnoreCase))
            {
                LastFailedOpener = DateTime.Now;
            }
        }

        public override string ToString()
        {
            return $"[{WowClass}] [{Role}] {Displayname} ({Author})";
        }

        protected abstract Spell GetOpeningSpell();

        protected bool SelectTarget(out WowUnit target)
        {
            WowUnit currentTarget = WowInterface.ObjectManager.Target;
            IEnumerable<WowUnit> nearAttackingEnemies = WowInterface.ObjectManager
                .GetEnemiesInCombatWithUs<WowUnit>(WowInterface.ObjectManager.Player.Position, 64.0)
                .Where(e => !e.IsDead && !e.IsNotAttackable)
                .OrderBy(e => e.Auras.All(aura => aura.Name != polymorphSpell));

            if (currentTarget != null && currentTarget.Guid != 0
               && (currentTarget.IsDead
                   || currentTarget.IsNotAttackable
                   || (currentTarget.Auras.Any(e => e.Name == polymorphSpell) &&
                       nearAttackingEnemies.Where(e => e.Auras.All(aura => aura.Name != polymorphSpell)).Any(e => e.Guid != currentTarget.Guid))
                   || (!currentTarget.IsInCombat && nearAttackingEnemies.Any())
                   || !BotUtils.IsValidUnit(WowInterface.ObjectManager.Target)
                   || WowInterface.HookManager.WowGetUnitReaction(WowInterface.ObjectManager.Player, currentTarget) == WowUnitReaction.Friendly))
            {
                currentTarget = null;
                target = null;
            }

            if (currentTarget != null)
            {
                target = currentTarget;
                return true;
            }

            if (nearAttackingEnemies.Any())
            {
                var potTarget = nearAttackingEnemies.FirstOrDefault();
                target = potTarget;
                WowInterface.HookManager.WowTargetGuid(potTarget.Guid);
                return true;
            }

            target = null;
            return false;
        }
    }
}