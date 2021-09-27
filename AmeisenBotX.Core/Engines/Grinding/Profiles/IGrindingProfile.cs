using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Objects.Mail;
using AmeisenBotX.Core.Objects.Npc;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Grinding.Profiles
{
    public interface IGrindingProfile
    {
        bool RandomizeSpots { get; }

        List<Vendor> Vendors { get; }

        List<Trainer> Trainers { get; }

        List<Mailbox> Mailboxes { get; }

        List<GrindingSpot> Spots { get; }
    }
}