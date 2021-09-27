using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects.Enums;

namespace AmeisenBotX.Core.Objects.Npc
{
    public class Vendor
    {
        public string Name;
        public int EntryId;
        public WowMapId MapId;
        public WowZoneId ZoneId;
        public Vector3 Position;
        public readonly NpcType Type;

        public Vendor(string name, int entryId, WowMapId mapId, WowZoneId zoneId,  Vector3 position, NpcType type)
        {
            Name = name;
            EntryId = entryId;
            MapId = mapId;
            ZoneId = zoneId;
            Position = position;
            Type = type;
        }
    }
}