using System.Collections.Generic;
using System.Linq;
using AmeisenBotX.Core.Quest.Objects.Objectives;

namespace AmeisenBotX.Core.Quest.Objects.Quests
{
    class GrindingBotQuest : IBotQuest
    {
        public GrindingBotQuest(string name, List<IQuestObjective> objectives)
        {
            Name = name;
            Objectives = objectives;
        }
        public string Name { get; }
        public bool Accepted => true;
        public bool Finished => Objectives != null && Objectives.All(e => e.Finished);
        public bool Returned => Finished;
        public int Id => -1;

        private List<IQuestObjective> Objectives { get; }

        public void AcceptQuest() {}

        public bool CompleteQuest()
        {
            return false;
        }

        public void Execute()
        {
            Objectives.FirstOrDefault(e => !e.Finished)?.Execute();
        }
    }
}
