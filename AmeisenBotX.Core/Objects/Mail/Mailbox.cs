using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects.Mail
{
    public class Mailbox
    {
        public WowMapId MapId;
        public WowZoneId ZoneId;
        public Vector3 Position;
        public readonly MailboxFactionType FactionType;

        public Mailbox(WowMapId mapId, WowZoneId zoneId, Vector3 position, MailboxFactionType factionType)
        {
            MapId = mapId;
            ZoneId = zoneId;
            Position = position;
            FactionType = factionType;
        }
    }
}