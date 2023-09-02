using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Quest.Profiles;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class QuestingDatabaseEngine : IQuestEngine
    {
        public List<int> CompletedQuests => throw new NotImplementedException();

        public IQuestProfile SelectedProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UpdatedCompletedQuests { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Enter()
        {
            //throw new NotImplementedException();
        }

        public void Execute()
        {
            //throw new NotImplementedException();
        }
    }
}