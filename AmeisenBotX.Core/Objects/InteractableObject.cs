using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects
{
    public class InteractableObject(int entryId, WowMapId mapId, WowZoneId zoneId, Vector3 position, InteractableObjectType objectType, MailboxFactionType factionType = MailboxFactionType.None)
    {
        public readonly MailboxFactionType FactionType = factionType;
        public readonly InteractableObjectType ObjectType = objectType;
        public int EntryId = entryId;
        public WowMapId MapId = mapId;
        public Vector3 Position = position;
        public WowZoneId ZoneId = zoneId;
    }
}