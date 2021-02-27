using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Personality.Enums;
using System;

namespace AmeisenBotX.Core.Personality.Objects
{
    [Serializable]
    public struct Relationship
    {
        public DateTime FirstSeen { get; set; }

        public WowMapId FirstSeenMapId { get; set; }

        public Vector3 FirstSeenPosition { get; set; }

        public DateTime LastSeen { get; set; }

        public WowMapId LastSeenMapId { get; set; }

        public Vector3 LastSeenPosition { get; set; }

        public RelationshipLevel Level => (RelationshipLevel)(int)MathF.Floor(Score);

        public float Score { get; set; }

        public TimeSpan TimeSpentWith { get; set; }

        public void Poll(WowInterface wowInterface, WowUnit unit)
        {
            LastSeen = DateTime.Now;
            LastSeenMapId = wowInterface.ObjectManager.MapId;
            LastSeenPosition = unit.Position;
        }
    }
}