using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects
{
    public class Npc
    {
        public readonly int EntryId;
        public readonly NpcSubType SubType;
        public readonly NpcType Type;
        public WowMapId MapId;
        public string Name;
        public Vector3 Position;
        public WowZoneId ZoneId;

        public Npc(string name, int entryId, WowMapId mapId, WowZoneId zoneId, Vector3 position,
            NpcType type, NpcSubType subType = NpcSubType.None)
        {
            Name = name;
            EntryId = entryId;
            MapId = mapId;
            ZoneId = zoneId;
            Position = position;
            Type = type;
            SubType = subType;
        }
    }
}