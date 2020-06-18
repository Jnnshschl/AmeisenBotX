using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Quest.Objects.Objectives
{
    public class QuestObjectiveChain : IQuestObjective
    {
        public QuestObjectiveChain(List<IQuestObjective> questObjectives)
        {
            QuestObjectives = questObjectives;
        }

        public bool Finished => Progress == 100.0;

        public double Progress => (double)QuestObjectives.Count(e => QuestObjectives.IndexOf(e) <= AlreadyCompletedIndex || e.Finished) / (double)QuestObjectives.Count * 100.0;

        private int AlreadyCompletedIndex
        {
            get
            {
                IQuestObjective questObjective = QuestObjectives.LastOrDefault(e => e.Finished);
                return QuestObjectives.IndexOf(questObjective);
            }
        }

        private List<IQuestObjective> QuestObjectives { get; }

        public void Execute()
        {
            QuestObjectives.FirstOrDefault(e => QuestObjectives.IndexOf(e) > AlreadyCompletedIndex && !e.Finished)?.Execute();
        }
    }
}