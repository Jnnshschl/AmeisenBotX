using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Battleground.Jannis
{
    public class JBgBlackboard
    {
        public WowUnit EnemyTeamFlagCarrier { get; set; }

        public Vector3 EnemyTeamFlagPos { get; set; }

        public bool EnemyTeamHasFlag { get; set; }

        public int EnemyTeamMaxScore { get; set; }

        public int EnemyTeamScore { get; set; }

        public WowUnit MyTeamFlagCarrier { get; set; }

        public Vector3 MyTeamFlagPos { get; set; }

        public bool MyTeamHasFlag { get; set; }

        public int MyTeamMaxScore { get; set; }

        public int MyTeamScore { get; set; }

        public List<WowGameobject> NearFlags { get; set; }
    }
}