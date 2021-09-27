using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Dungeon.Enums;
using AmeisenBotX.Core.Engines.Dungeon.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Dungeon.Profiles
{
    public interface IDungeonProfile
    {
        string Author { get; }

        string Description { get; }

        Vector3 DungeonExit { get; }

        DungeonFactionType FactionType { get; }

        int GroupSize { get; }

        WowMapId MapId { get; }

        int MaxLevel { get; }

        string Name { get; }

        List<DungeonNode> Nodes { get; }

        List<int> PriorityUnits { get; }

        int RequiredItemLevel { get; }

        int RequiredLevel { get; }

        Vector3 WorldEntry { get; }

        WowMapId WorldEntryMapId { get; }
    }
}