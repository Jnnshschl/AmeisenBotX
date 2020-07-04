using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Dungeon
{
    public interface IDungeonEngine
    {
        List<DungeonNode> Nodes { get; }

        IDungeonProfile Profile { get; }

        void Enter();

        void Execute();

        void Exit();

        void OnDeath();
    }
}