using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class KillUnitQuestObjective : IQuestObjective
    {
        public KillUnitQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, Func<bool> condition)
        {
            Bot = bot;
            ObjectDisplayIds = new Dictionary<int, int>() { { 0, objectDisplayId } };
            Condition = condition;
        }

        public KillUnitQuestObjective(AmeisenBotInterfaces bot, Dictionary<int, int> objectDisplayIds, Func<bool> condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private Func<bool> Condition { get; }

        private Dictionary<int, int> ObjectDisplayIds { get; }

        private IWowUnit Unit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (Bot.Target != null
                && !Bot.Target.IsDead
                && !Bot.Target.IsNotAttackable
                && Bot.Db.GetReaction(Bot.Player, Bot.Target) != WowUnitReaction.Friendly)
            {
                Unit = Bot.Target;
            }
            else
            {
                Bot.Wow.ClearTarget();

                Unit = Bot.Objects.All
                    .OfType<IWowUnit>()
                    .Where(e => !e.IsDead && ObjectDisplayIds.ContainsValue(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .OrderBy(e => ObjectDisplayIds.First(x => x.Value == e.DisplayId).Key)
                    .FirstOrDefault();
            }

            if (Unit != null)
            {
                if (Unit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Movement.Reset();
                    Bot.Wow.InteractWithUnit(Unit);
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, Unit.Position);
                }
            }
        }
    }
}