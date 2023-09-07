using AmeisenBotX.Common;
using AmeisenBotX.Common.Engines.Quest.Interfaces;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class ZoneProfile : IQuestProfile
    {
        public ZoneProfile(AmeisenBotInterfaces bot)
        {

        }

        public Queue<List<IBotQuest>> Quests => throw new NotImplementedException();

        public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}
