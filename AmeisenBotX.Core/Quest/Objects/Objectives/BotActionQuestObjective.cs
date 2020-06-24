namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public delegate bool BotActionQuestObjectiveCondition();

    public class BotActionQuestObjective : IQuestObjective
    {
        public BotActionQuestObjective(BotActionQuestObjectiveCondition action)
        {
            Action = action;
        }

        public BotActionQuestObjectiveCondition Action { get; }

        public bool Finished => Progress == 100.0;

        public double Progress { get; set; }

        public void Execute()
        {
            if (Finished) { return; }

            Action();

            Progress = 100.0;
        }
    }
}