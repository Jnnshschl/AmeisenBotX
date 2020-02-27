using AmeisenBotX.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Jobs.Profiles.Gathering
{
    public interface IGatheringProfile : IJobProfile
    {
        public List<int> DisplayIds { get; set; }

        public Vector3 MailboxPosition { get; set; }

        public List<Vector3> Path { get; set; }
    }
}
