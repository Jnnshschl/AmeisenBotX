using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool UseUnitQuestObjectiveCondition();

    public class UseUnitQuestObjective : IQuestObjective
    {
        public UseUnitQuestObjective(AmeisenBotInterfaces bot, int objectDisplayId, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public UseUnitQuestObjective(AmeisenBotInterfaces bot, List<int> objectDisplayIds, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            Bot = bot;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private AmeisenBotInterfaces Bot { get; }

        private UseUnitQuestObjectiveCondition Condition { get; }

        private List<int> ObjectDisplayIds { get; }

        private bool QuestgiversOnly { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || Bot.Player.IsCasting) { return; }

            WowUnit = Bot.Objects.GetClosestWowUnitByDisplayId(Bot.Player.Position, ObjectDisplayIds, QuestgiversOnly);

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(Bot.Player.Position) < 3.0)
                {
                    Bot.Wow.WowStopClickToMove();
                    Bot.Movement.Reset();
                }

                Bot.Wow.WowUnitRightClick(WowUnit.BaseAddress);
            }
        }
    }
}