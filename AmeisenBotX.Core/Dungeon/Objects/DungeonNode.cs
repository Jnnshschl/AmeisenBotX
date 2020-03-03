using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Pathfinding.Objects;

namespace AmeisenBotX.Core.Dungeon.Objects
{
    public struct DungeonNode
    {
        public DungeonNode(Vector3 position, DungeonNodeType type)
        {
            Position = position;
            Type = type;
        }

        public Vector3 Position { get; }

        public DungeonNodeType Type { get; }
    }
}