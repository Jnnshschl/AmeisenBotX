using AmeisenBotX.Core;
using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Quest.Profiles;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class QuestingDatabaseEngine : IQuestEngine
    {
        public QuestingDatabaseEngine(AmeisenBotInterfaces bot)
        {
            Bot = bot;
            Profiles.Add(item: new StartingAreaProfile(bot) { Name = "Testing" });
            SelectedProfile = Profiles.First();
        }

        public List<int> CompletedQuests { get; } = new List<int>();

        public IQuestProfile SelectedProfile { get; set; }
        public bool UpdatedCompletedQuests { get; set; }

        public ICollection<IQuestProfile> Profiles { get; init; } = new List<IQuestProfile>();
        public AmeisenBotInterfaces Bot { get; }

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