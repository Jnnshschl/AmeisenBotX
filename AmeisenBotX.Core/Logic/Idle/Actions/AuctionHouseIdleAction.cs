using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class AuctionHouseIdleAction(AmeisenBotInterfaces bot) : IIdleAction
    {
        public bool AutopilotOnly => true;

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 5 * 60 * 1000;

        public int MaxDuration => 3 * 60 * 1000;

        public int MinCooldown => 4 * 60 * 1000;

        public int MinDuration => 2 * 60 * 1000;

        private DateTime AuctioneerTalkTime { get; set; }

        private AmeisenBotInterfaces Bot { get; } = bot;

        private Vector3 CurrentAuctioneer { get; set; }

        private Vector3 OriginPosition { get; set; }

        private bool ReturnedToOrigin { get; set; }

        private Random Rnd { get; } = new Random();

        private bool TalkedToAuctioneer { get; set; }

        public bool Enter()
        {
            TalkedToAuctioneer = false;
            AuctioneerTalkTime = default;
            OriginPosition = Bot.Player.Position;

            if (Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.Auctioneer, Bot.Player.Position, 256.0f, out IEnumerable<Vector3> auctioneers))
            {
                CurrentAuctioneer = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.Objects.MapId, auctioneers.OrderBy(e => e.GetDistance(Bot.Player.Position)).First(), 2.5f);
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!TalkedToAuctioneer)
            {
                if (CurrentAuctioneer.GetDistance(Bot.Player.Position) > 3.2f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, CurrentAuctioneer);
                }
                else
                {
                    Bot.Movement.StopMovement();

                    IWowUnit auctioneer = Bot.Objects.All.OfType<IWowUnit>()
                        .FirstOrDefault(e => e.IsAuctioneer && e.Position.GetDistance(CurrentAuctioneer) < 1.0f);

                    if (auctioneer != null)
                    {
                        Bot.Wow.FacePosition(Bot.Player.BaseAddress, Bot.Player.Position, auctioneer.Position);
                        Bot.Wow.InteractWithUnit(auctioneer);
                    }

                    TalkedToAuctioneer = true;
                    AuctioneerTalkTime = DateTime.UtcNow + TimeSpan.FromSeconds(Rnd.Next(120, 180));
                }
            }
            else if (!ReturnedToOrigin && AuctioneerTalkTime < DateTime.UtcNow)
            {
                if (CurrentAuctioneer.GetDistance(OriginPosition) > 8.0f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, OriginPosition);
                }
                else
                {
                    Bot.Movement.StopMovement();
                    ReturnedToOrigin = true;
                }
            }
        }

        public override string ToString()
        {
            return $"{(AutopilotOnly ? "(🤖) " : "")}Go to Auctionhouse";
        }
    }
}