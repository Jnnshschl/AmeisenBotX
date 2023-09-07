using AmeisenBotX.Common.BehaviorTree.Interfaces;
using AmeisenBotX.Common.Math;
using AmeisenBotX.Wow.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Engines.Battleground.Jannis
{
    public interface ICtfBlackboard : IBlackboard
    {
        IWowUnit EnemyTeamFlagCarrier { get; set; }
        Vector3 EnemyTeamFlagPos { get; set; }
        bool EnemyTeamHasFlag { get; set; }
        int EnemyTeamMaxScore { get; set; }
        int EnemyTeamScore { get; set; }
        IWowUnit MyTeamFlagCarrier { get; set; }
        Vector3 MyTeamFlagPos { get; set; }
        bool MyTeamHasFlag { get; set; }
        int MyTeamMaxScore { get; set; }
        int MyTeamScore { get; set; }
        IEnumerable<IWowGameobject> NearFlags { get; set; }

        void Update();
    }
}