using AmeisenBotX.Core;
using AmeisenBotX.Core.Engines.Quest;
using AmeisenBotX.Core.Engines.Quest.Profiles;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class QuestingDatabaseEngine : DefaultQuestEngine
    {
        public QuestingDatabaseEngine(AmeisenBotInterfaces bot):base(bot)
        {
            Bot = bot;
            Profiles.Clear();
            Profiles.Add(item: new ZoneProfile(bot) { Name = "Testing" });
            SelectedProfile = Profiles.First();
        }

        public new void Enter()
        {
            base.Enter();
            //throw new NotImplementedException();
        }

        public new void Execute()
        {
            base.Execute();
            //throw new NotImplementedException();
            
        }
    }
}