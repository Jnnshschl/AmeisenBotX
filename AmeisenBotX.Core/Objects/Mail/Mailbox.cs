using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects.Mail
{
    public class Mailbox
    {
        public int EntryId;
        public WowMapId MapId;
        public WowZoneId ZoneId;
        public Vector3 Position;
        public readonly MailboxFactionType FactionType;

        public Mailbox(int entryId, WowMapId mapId, WowZoneId zoneId, Vector3 position, MailboxFactionType factionType)
        {
            EntryId = entryId;
            MapId = mapId;
            ZoneId = zoneId;
            Position = position;
            FactionType = factionType;
        }
    }
}