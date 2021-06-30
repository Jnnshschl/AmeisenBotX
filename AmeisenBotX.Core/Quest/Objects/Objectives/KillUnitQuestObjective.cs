using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool KillUnitQuestObjectiveCondition();

    public class KillUnitQuestObjective : IQuestObjective
    {
        public KillUnitQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, KillUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = new Dictionary<int, int>() { { 0, objectDisplayId } };
            Condition = condition;
        }

        public KillUnitQuestObjective(AmeisenBotInterfaces bot, Dictionary<int, int> objectDisplayIds, KillUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private KillUnitQuestObjectiveCondition Condition { get; }

        private Dictionary<int, int> ObjectDisplayIds { get; }

        private AmeisenBotInterfaces Bot { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (Bot.Target != null
                && !Bot.Target.IsDead
                && !Bot.Target.IsNotAttackable
                && Bot.Db.GetReaction(Bot.Player, Bot.Target) != WowUnitReaction.Friendly)
            {
                WowUnit = Bot.Target;
            }
            else
            {
                Bot.Wow.WowClearTarget();

                WowUnit = Bot.Objects.WowObjects
                    .OfType<WowUnit>()
                    .Where(e => !e.IsDead && ObjectDisplayIds.Values.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .OrderBy(e => ObjectDisplayIds.First(x => x.Value == e.DisplayId).Key)
                    .FirstOrDefault();
            }

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.WowStopClickToMove();
                    Bot.Movement.Reset();
                    Bot.Wow.WowUnitRightClick(WowUnit.BaseAddress);
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, WowUnit.Position);
                }
            }
        }
    }
}