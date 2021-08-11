using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
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

        private AmeisenBotInterfaces Bot { get; }

        private KillUnitQuestObjectiveCondition Condition { get; }

        private IWowUnit IWowUnit { get; set; }

        private Dictionary<int, int> ObjectDisplayIds { get; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            if (Bot.Target != null
                && !Bot.Target.IsDead
                && !Bot.Target.IsNotAttackable
                && Bot.Db.GetReaction(Bot.Player, Bot.Target) != WowUnitReaction.Friendly)
            {
                IWowUnit = Bot.Target;
            }
            else
            {
                Bot.Wow.ClearTarget();

                IWowUnit = Bot.Objects.WowObjects
                    .OfType<IWowUnit>()
                    .Where(e => !e.IsDead && ObjectDisplayIds.Values.Contains(e.DisplayId))
                    .OrderBy(e => e.Position.GetDistance(Bot.Player.Position))
                    .OrderBy(e => ObjectDisplayIds.First(x => x.Value == e.DisplayId).Key)
                    .FirstOrDefault();
            }

            if (IWowUnit != null)
            {
                if (IWowUnit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.StopClickToMove();
                    Bot.Movement.Reset();
                    Bot.Wow.InteractWithUnit(IWowUnit.BaseAddress);
                }
                else
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, IWowUnit.Position);
                }
            }
        }
    }
}