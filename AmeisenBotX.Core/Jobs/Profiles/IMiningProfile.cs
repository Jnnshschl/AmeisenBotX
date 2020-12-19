using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IMiningProfile : IJobProfile
    {
        bool IsCirclePath { get; }

        List<Vector3> MailboxNodes { get; }

        List<OreNode> OreTypes { get; }

        List<Vector3> Path { get; }
    }
}