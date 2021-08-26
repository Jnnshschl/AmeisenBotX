using AmeisenBotX.Common.Math;

namespace AmeisenBotX.Core.Engines.Npc
{
    public class Trainer
    {
        private string Name;
        public int EntryId;
        public int MapId;
        private Vector3 Position;
        private NpcType Type;

        public Trainer(string name, int entryId, int mapId, Vector3 position, NpcType type)
        {
            Name = name;
            EntryId = entryId;
            MapId = mapId;
            Position = position;
            Type = type;
        }
    }
}