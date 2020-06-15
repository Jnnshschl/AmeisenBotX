using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IMiningProfile : IJobProfile
    {
        List<Vector3> Path { get; }
        List<OreNodes> OreTypes { get; }
        bool IsCirclePath { get; }
        List<Vector3> Mailboxes { get; }
    }
}
