using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Dungeon.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Jobs.Profiles
{
    public interface IDungeonProfile
    {
        string Author { get; }

        string Description { get; }

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