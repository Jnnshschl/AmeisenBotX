using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public interface IBattlegroundProfile
    {
        BattlegroundType BattlegroundType { get; }

        List<IBattlegroundObjective> Objectives { get; }
    }
}
