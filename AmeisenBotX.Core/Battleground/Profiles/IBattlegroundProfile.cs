using AmeisenBotX.Core.Battleground.Enums;
using AmeisenBotX.Core.Battleground.States;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Battleground.Profiles
{
    public interface IBattlegroundProfile
    {
        BattlegroundType BattlegroundType { get; }

        Dictionary<BattlegroundState, BasicBattlegroundState> States { get; }
    }
}