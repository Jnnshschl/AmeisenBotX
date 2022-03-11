using AmeisenBotX.Core.Engines.Quest.Objects.Objectives;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Quest.Objects.Quests
{
    internal class GrindingBotQuest : IBotQuest
    {
        public GrindingBotQuest(string name, List<IQuestObjective> objectives)
        {
            Name = name;
            Objectives = objectives;
        }

        public bool Accepted => true;

        public bool Finished => Objectives != null && Objectives.All(e => e.Finished);

        public int Id => -1;

        public string Name { get; }

        public List<IQuestObjective> Objectives { get; }

        public bool Returned => Finished;

        public void AcceptQuest()
        {
        }

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