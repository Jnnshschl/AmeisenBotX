using AmeisenBotX.Core.Engines.Jobs.Enums;

namespace AmeisenBotX.Core.Engines.Jobs.Profiles
{
    public interface IJobProfile
    {
        JobType JobType { get; }
    }
}