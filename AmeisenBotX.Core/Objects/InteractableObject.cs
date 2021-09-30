using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects
{
    public class InteractableObject
    {
        public int EntryId;
        public WowMapId MapId;
        public WowZoneId ZoneId;
        public Vector3 Position;
        public readonly InteractableObjectType ObjectType;
        public readonly MailboxFactionType FactionType;

        public InteractableObject(int entryId, WowMapId mapId, WowZoneId zoneId, Vector3 position,
            InteractableObjectType objectType, MailboxFactionType factionType = MailboxFactionType.None)
        {
            EntryId = entryId;
            MapId = mapId;
            ZoneId = zoneId;
            Position = position;
            ObjectType = objectType;
            FactionType = factionType;
        }
    }
}