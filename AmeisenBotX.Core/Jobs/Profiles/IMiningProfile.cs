using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Jobs.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IMiningProfile : IJobProfile
    {
        bool IsCirclePath { get; }

        List<Vector3> MailboxNodes { get; }

        //List<MailBox> MailBoxes { get; }
        List<OreNodes> OreTypes { get; }

        List<Vector3> Path { get; }
    }
}