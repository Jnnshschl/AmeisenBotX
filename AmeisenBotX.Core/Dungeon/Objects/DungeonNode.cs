using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;

namespace AmeisenBotX.Core.Dungeon.Objects
{
    public class DungeonNode
    {
        public DungeonNode(Vector3 position, DungeonNodeType type, string extra = "")
        {
            Position = position;
            Type = type;
            Extra = extra;
        }

        public string Extra { get; }

        public Vector3 Position { get; }

        public DungeonNodeType Type { get; }
    }
}