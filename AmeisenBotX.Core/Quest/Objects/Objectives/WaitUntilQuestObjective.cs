using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool WaitUntilQuestObjectiveCondition();

    public class WaitUntilQuestObjective : IQuestObjective
    {
        public WaitUntilQuestObjective(WaitUntilQuestObjectiveCondition condition)
        {
            Condition = condition;
        }

        public WaitUntilQuestObjectiveCondition Condition { get; }

        public bool Finished => Progress == 100.0;

        public double Progress => Condition() ? 100.0 : 0.0;

        public void Execute()
        {
            if (Finished) { return; }
        }
    }
}