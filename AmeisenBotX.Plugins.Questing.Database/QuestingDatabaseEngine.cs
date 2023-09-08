using AmeisenBotX.Common;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class QuestingDatabaseEngine : IQuestEngine
    {
        public QuestingDatabaseEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Profiles.Clear();
            Profiles.Add(item: new ZoneProfile(bot) { Name = "Testing Profile" });
            SelectedProfile = Profiles.First();
        }

        public AmeisenBotInterfaces Bot { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public List<int> CompletedQuests => throw new NotImplementedException();

        public IQuestProfile SelectedProfile { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool UpdatedCompletedQuests { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ICollection<IQuestProfile> Profiles => throw new NotImplementedException();

        public new void Enter()
        {

        }

        public new void Execute()
        {

        }
    }
}