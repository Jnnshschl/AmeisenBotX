using AmeisenBotX.Core.Data.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool UseUnitQuestObjectiveCondition();

    public class UseUnitQuestObjective : IQuestObjective
    {
        public UseUnitQuestObjective(WowInterface wowInterface, int objectDisplayId, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = new List<int>() { objectDisplayId };
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public UseUnitQuestObjective(WowInterface wowInterface, List<int> objectDisplayIds, bool questgiversOnly, UseUnitQuestObjectiveCondition condition)
        {
            WowInterface = wowInterface;
            ObjectDisplayIds = objectDisplayIds;
            Condition = condition;
            QuestgiversOnly = questgiversOnly;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        private UseUnitQuestObjectiveCondition Condition { get; }

        private List<int> ObjectDisplayIds { get; }

        private bool QuestgiversOnly { get; }

        private WowInterface WowInterface { get; }

        private WowUnit WowUnit { get; set; }

        public void Execute()
        {
            if (Finished || WowInterface.Player.IsCasting) { return; }

            WowUnit = WowInterface.Objects.GetClosestWowUnitByDisplayId(WowInterface.Player.Position, ObjectDisplayIds, QuestgiversOnly);

            if (WowUnit != null)
            {
                if (WowUnit.Position.GetDistance(WowInterface.Player.Position) < 3.0)
                {
                    WowInterface.NewWowInterface.WowStopClickToMove();
                    WowInterface.MovementEngine.Reset();
                }

                WowInterface.NewWowInterface.WowUnitRightClick(WowUnit.BaseAddress);
            }
        }
    }
}