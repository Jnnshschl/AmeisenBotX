using System;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class BotActionQuestObjective(Action action) : IQuestObjective
    {
        public Action Action { get; } = action;

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