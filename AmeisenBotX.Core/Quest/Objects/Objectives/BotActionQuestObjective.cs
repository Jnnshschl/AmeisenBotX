using System;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class BotActionQuestObjective : IQuestObjective
    {
        public BotActionQuestObjective(Action action)
        {
            Action = action;
        }

        public Action Action { get; }

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