using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Npc
{
    public class Vendor
    {
        public string Name;
        public int EntryId;
        public int MapId;
        public Vector3 Position;
        public NpcType Type;

        public Vendor(string name, int entryId, int mapId, Vector3 position, NpcType type)
        {
            Name = name;
            EntryId = entryId;
            MapId = mapId;
            Position = position;
            Type = type;
        }
    }
}