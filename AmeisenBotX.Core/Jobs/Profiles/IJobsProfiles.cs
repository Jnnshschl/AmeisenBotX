using AmeisenBotX.Core.Jobs.Enums;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IJobProfile
    {
        JobType JobType { get; }
    }
}