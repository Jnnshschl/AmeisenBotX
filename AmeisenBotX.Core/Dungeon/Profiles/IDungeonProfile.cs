using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IDungeonProfile
    {
        string Author { get; }

        string Description { get; }

        DungeonFactionType FactionType { get; }

        int GroupSize { get; }

        MapId MapId { get; }

        int MaxLevel { get; }

        string Name { get; }

        List<DungeonNode> Path { get; }

        List<string> PriorityUnits { get; }

        int RequiredItemLevel { get; }

        int RequiredLevel { get; }

        Vector3 WorldEntry { get; }

        MapId WorldEntryMapId { get; }
    }
}