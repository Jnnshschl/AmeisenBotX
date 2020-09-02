using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using System.Collections.Generic;
using System.Numerics;

namespace AmeisenBotX.Core.Dungeon
{
    public interface IDungeonEngine
    {
        Vector3 DeathEntrancePosition { get; }

        List<DungeonNode> Nodes { get; }

        IDungeonProfile Profile { get; }

        void Enter();

        void Execute();

        void Exit();

        void OnDeath();

        IDungeonProfile TryGetProfileByMapId(MapId mapId);
    }
}