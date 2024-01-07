using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Dungeon.Enums;

namespace AmeisenBotX.Core.Engines.Dungeon.Objects
{
    public class DungeonNode(Vector3 position, DungeonNodeType type = DungeonNodeType.Normal, string extra = "")
    {
        public string Extra { get; } = extra;

        public Vector3 Position { get; } = position;

        public DungeonNodeType Type { get; } = type;
    }
}