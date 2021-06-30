using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Dungeon.Enums;

namespace AmeisenBotX.Core.Dungeon.Objects
{
    public class DungeonNode
    {
        public DungeonNode(Vector3 position, DungeonNodeType type = DungeonNodeType.Normal, string extra = "")
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