using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Npc
{
    public class Trainer
    {
        public string Name;
        public int EntryId;
        public int MapId;
        public Vector3 Position;
        public NpcType Type;
        public NpcSubType SubType;

        public Trainer(string name, int entryId, int mapId, Vector3 position, NpcType type, NpcSubType subType)
        {
            Name = name;
            EntryId = entryId;
            MapId = mapId;
            Position = position;
            Type = type;
            SubType = subType;
        }
    }
}