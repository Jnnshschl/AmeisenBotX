using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class AuctionHouseIdleAction : IIdleAction
    {
        public AuctionHouseIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Rnd = new Random();
        }

        public bool AutopilotOnly => true;

        public int MaxCooldown => 20 * 60 * 1000;

        public int MaxDuration => 4 * 60 * 1000;

        public int MinCooldown => 20 * 60 * 1000;

        public int MinDuration => 2 * 60 * 1000;

        private DateTime AuctioneerTalkTime { get; set; }

        private Vector3 CurrentAuctioneer { get; set; }

        private Vector3 OriginPosition { get; set; }

        private bool ReturnedToOrigin { get; set; }

        private Random Rnd { get; }

        private bool TalkedToAuctioneer { get; set; }

        private WowInterface WowInterface { get; }

        public bool Enter()
        {
            TalkedToAuctioneer = false;
            AuctioneerTalkTime = default;
            OriginPosition = WowInterface.Player.Position;

            if (WowInterface.Db.TryGetPointsOfInterest(WowInterface.ObjectManager.MapId, Data.Db.Enums.PoiType.Auctioneer, WowInterface.Player.Position, 256.0f, out IEnumerable<Vector3> auctioneers))
            {
                CurrentAuctioneer = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, auctioneers.OrderBy(e => e.GetDistance(WowInterface.Player.Position)).First(), 2.5f);
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!TalkedToAuctioneer)
            {
                if (CurrentAuctioneer.GetDistance(WowInterface.Player.Position) > 3.2f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentAuctioneer);
                }
                else
                {
                    WowInterface.MovementEngine.StopMovement();

                    WowUnit auctioneer = WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                        .FirstOrDefault(e => e.IsAuctioneer && e.Position.GetDistance(CurrentAuctioneer) < 1.0f);

                    if (auctioneer != null)
                    {
                        WowInterface.HookManager.WowFacePosition(WowInterface.Player, auctioneer.Position);
                        WowInterface.HookManager.WowUnitRightClick(auctioneer);
                    }

                    TalkedToAuctioneer = true;
                    AuctioneerTalkTime = DateTime.UtcNow + TimeSpan.FromSeconds(Rnd.Next(120, 180));
                }
            }
            else if (!ReturnedToOrigin && AuctioneerTalkTime < DateTime.UtcNow)
            {
                if (CurrentAuctioneer.GetDistance(OriginPosition) > 8.0f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, OriginPosition);
                }
                else
                {
                    WowInterface.MovementEngine.StopMovement();
                    ReturnedToOrigin = true;
                }
            }
        }
    }
}