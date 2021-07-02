namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public delegate bool WaitUntilQuestObjectiveCondition();

    public class WaitUntilQuestObjective : IQuestObjective
    {
        public WaitUntilQuestObjective(WaitUntilQuestObjectiveCondition condition)
        {
            Condition = condition;
        }

        public WaitUntilQuestObjectiveCondition Condition { get; }

        public bool Finished { get; set; }

        public double Progress => Condition() ? 100.0 : 0.0;

        public void Execute()
        {
            if (Finished || Progress == 100.0)
            {
                Finished = true;
                return;
            }
        }
    }
}