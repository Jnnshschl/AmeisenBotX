using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Objects.Enums;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects
{
    public class Npc
    {
        public string Name;
        public readonly int EntryId;
        public WowMapId MapId;
        public WowZoneId ZoneId;
        public Vector3 Position;
        public readonly NpcType Type;
        public readonly NpcSubType SubType;

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