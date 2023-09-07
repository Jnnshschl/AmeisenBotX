using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Jobs.Profiles
{
    public interface IMiningProfile : IJobProfile
    {
        bool IsCirclePath { get; }

        List<Vector3> MailboxNodes { get; }

        List<WowOreId> OreTypes { get; }

        List<Vector3> Path { get; }
    }
}