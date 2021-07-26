using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Character.Spells.Objects;
using AmeisenBotX.Core.Engines.Combat.Classes.Jannis;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.CombatClasses.Shino
{
    public abstract class TemplateCombatClass : BasicCombatClass
    {
        public TemplateCombatClass(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine) : base(bot, stateMachine)
        {
            //this line cause a bug because it run out of index
            //Bot.EventHookManager.Subscribe("UI_ERROR_MESSAGE", (t, a) => OnUIErrorMessage(a[0]));
        }

        public new string Author { get; } = "Shino";

        private DateTime LastFailedOpener { get; set; } = DateTime.Now;

        public override void AttackTarget()
        {
            IWowUnit target = Bot.Target;
            if (target == null)
            {
                return;
            }

            if (IsTargetAttackable(target))
            {
                Spell openingSpell = GetOpeningSpell();
                Bot.Wow.StopClickToMove();
                Bot.Movement.StopMovement();
                Bot.Movement.Reset();
                TryCastSpell(openingSpell.Name, target.Guid, openingSpell.Costs > 0);
            }
            else if (Bot.Player.Position.GetDistance(target.Position) < 3.5f || Bot.Movement.Status == MovementAction.None)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, target.Position);
            }
        }

        public bool IsTargetAttackable(IWowUnit target)
        {
            Spell openingSpell = GetOpeningSpell();
            float posOffset = 0.5f;
            Vector3 currentPos = Bot.Player.Position;
            Vector3 posXLeft = Bot.Player.Position;
            posXLeft.X -= posOffset;
            Vector3 posXRight = Bot.Player.Position;
            posXRight.X += posOffset;
            Vector3 posYRight = Bot.Player.Position;
            posYRight.Y += posOffset;
            Vector3 posYLeft = Bot.Player.Position;
            posYLeft.Y -= posOffset;

            return IsInRange(openingSpell, target)
                    && DateTime.Now.Subtract(LastFailedOpener).TotalSeconds > 3
                    && Bot.Wow.IsInLineOfSight(currentPos, target.Position)
                    && Bot.Wow.IsInLineOfSight(posXLeft, target.Position)
                    && Bot.Wow.IsInLineOfSight(posXRight, target.Position)
                    && Bot.Wow.IsInLineOfSight(posYRight, target.Position)
                    && Bot.Wow.IsInLineOfSight(posYLeft, target.Position);
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

        protected bool SelectTarget(out IWowUnit target)
        {
            IWowUnit currentTarget = Bot.Target;
            IEnumerable<IWowUnit> nearAttackingEnemies = Bot
                .GetEnemiesInCombatWithParty<IWowUnit>(Bot.Player.Position, 64.0f)
                .Where(e => !e.IsDead && !e.IsNotAttackable)
                .OrderBy(e => e.Auras.All(aura => Bot.Db.GetSpellName(aura.SpellId) != polymorphSpell));

            if (currentTarget != null && currentTarget.Guid != 0
               && (currentTarget.IsDead
                   || currentTarget.IsNotAttackable
                   || (currentTarget.Auras.Any(e => Bot.Db.GetSpellName(e.SpellId) == polymorphSpell) &&
                       nearAttackingEnemies.Where(e => e.Auras.All(aura => Bot.Db.GetSpellName(aura.SpellId) != polymorphSpell)).Any(e => e.Guid != currentTarget.Guid))
                   || (!currentTarget.IsInCombat && nearAttackingEnemies.Any())
                   || !IWowUnit.IsValidUnit(Bot.Target)
                   || Bot.Db.GetReaction(Bot.Player, currentTarget) == WowUnitReaction.Friendly))
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
                IWowUnit potTarget = nearAttackingEnemies.FirstOrDefault();
                target = potTarget;
                Bot.Wow.ChangeTarget(potTarget.Guid);
                return true;
            }

            target = null;
            return false;
        }
    }
}