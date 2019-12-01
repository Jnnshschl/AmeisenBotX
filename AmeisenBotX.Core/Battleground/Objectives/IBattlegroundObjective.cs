using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmeisenBotX.Core.Battleground.BattlegroundEngine;

namespace AmeisenBotX.Core.Battleground.Objectives
{
    public interface IBattlegroundObjective
    {
        int Priority { get; }

        bool IsAvailable { get; }

        void Execute();
    }
}
