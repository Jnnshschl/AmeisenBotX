using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Pathfinding.Objects;

namespace AmeisenBotX.Core.Dungeon.Objects
{
    public struct DungeonNode
    {
        public DungeonNode(Vector3 position, DungeonNodeType type, string extra = "")
        {
            Position = position;
            Type = type;
            Extra = extra;
        }

        public Vector3 Position { get; }

        public DungeonNodeType Type { get; }

        public string Extra { get; }
    }
}