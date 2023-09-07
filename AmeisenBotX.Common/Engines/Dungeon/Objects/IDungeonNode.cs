using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Dungeon.Enums;

namespace AmeisenBotX.Core.Engines.Dungeon.Objects
{
    public interface IDungeonNode
    {
        string Extra { get; }
        Vector3 Position { get; }
        DungeonNodeType Type { get; }
    }
}