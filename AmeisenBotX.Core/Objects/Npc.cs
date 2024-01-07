using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects
{
    public class Npc(string name, int entryId, WowMapId mapId, WowZoneId zoneId, Vector3 position, NpcType type, NpcSubType subType = NpcSubType.None)
    {
        public readonly int EntryId = entryId;
        public readonly NpcSubType SubType = subType;
        public readonly NpcType Type = type;
        public WowMapId MapId = mapId;
        public string Name = name;
        public Vector3 Position = position;
        public WowZoneId ZoneId = zoneId;
    }
}