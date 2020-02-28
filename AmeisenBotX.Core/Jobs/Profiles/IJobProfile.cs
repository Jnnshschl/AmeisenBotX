using AmeisenBotX.Core.Jobs.Enums;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IJobProfile
    {
        public string Author { get; }

        public string Description { get; }

        public JobType JobType { get; }

        public string Name { get; }
    }
}