using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles.Gathering
{
    public interface IGatheringProfile : IJobProfile
    {
        public List<int> DisplayIds { get; set; }

        public Vector3 MailboxPosition { get; set; }

        public List<string> MailItems { get; set; }

        public string MailReceiver { get; set; }

        public List<Vector3> Path { get; set; }
    }
}