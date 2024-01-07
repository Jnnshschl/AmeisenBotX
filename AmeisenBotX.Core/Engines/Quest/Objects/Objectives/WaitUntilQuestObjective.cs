using System;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Objectives
{
    public class WaitUntilQuestObjective(Func<bool> condition) : IQuestObjective
    {
        public Func<bool> Condition { get; } = condition;

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