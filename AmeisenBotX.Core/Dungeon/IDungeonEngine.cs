using AmeisenBotX.Core.Dungeon.Objects;
using AmeisenBotX.Core.Jobs.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Dungeon
{
    public interface IDungeonEngine
    {
        List<DungeonNode> Nodes { get; }

        IDungeonProfile Profile { get; }

        void Enter();

        void Exit();

        void Execute();

        void OnDeath();
    }
}
