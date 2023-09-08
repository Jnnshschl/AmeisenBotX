using AmeisenBotX.Common;
using AmeisenBotX.Core.Engines.Quest.Objects.Quests;

namespace AmeisenBotX.Plugins.Questing.Database
{
    public class ZoneProfile : IQuestProfile
    {
        public ZoneProfile(AmeisenBotInterfaces bot)
        {

        }

        public Queue<ICollection<IBotQuest>> Quests => throw new NotImplementedException();

        public string Name { get; init; }

        public override string ToString()
        {
            return Name;
        }
    }
}
