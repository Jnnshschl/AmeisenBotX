﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Linq;

namespace AmeisenBotX.Core.Logic.Idle.Actions
{
    public class SitToChairIdleAction : IIdleAction
    {
        public SitToChairIdleAction(AmeisenBotInterfaces bot, double maxDistance)
        {
            Bot = bot;
            MaxDistance = maxDistance;
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public DateTime Cooldown { get; set; }

        public int MaxCooldown => 69 * 1000;

        public int MaxDuration => 90 * 1000;

        public int MinCooldown => 29 * 1000;

        public int MinDuration => 25 * 1000;

        private IWowGameobject CurrentSeat { get; set; }

        private double MaxDistance { get; }

        private bool SatDown { get; set; }

        public bool Enter()
        {
            SatDown = false;

            // get the center from where to cal the distance, this is needed
            // to prevent going out of the follow trigger radius, which
            // would cause a suspicous loop of running around
            Vector3 originPos = Bot.Player.Position; // StateMachine.Get<StateFollowing>().IsUnitToFollowThere(out IWowUnit unit, false) ? unit.Position : Bot.Player.Position;

            IWowGameobject seat = Bot.Objects.WowObjects.OfType<IWowGameobject>()
                .OrderBy(e => e.Position.GetDistance(originPos))
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Chair
                    // make sure no one sits on the chair besides ourself
                    && !Bot.Objects.WowObjects.OfType<IWowUnit>()
                        .Where(e => e.Guid != Bot.Wow.PlayerGuid)
                        .Any(x => e.Position.GetDistance(x.Position) < 0.6f)
                    && e.Position.GetDistance(originPos) < MaxDistance - 0.2f);

            if (seat != null)
            {
                CurrentSeat = seat;
                return true;
            }

            return false;
        }

        public void Execute()
        {
            if (!SatDown)
            {
                if (CurrentSeat.Position.GetDistance(Bot.Player.Position) > 1.5f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, CurrentSeat.Position);
                }
                else
                {
                    Bot.Movement.StopMovement();
                    Bot.Wow.InteractWithObject(CurrentSeat.BaseAddress);

                    SatDown = true;
                }
            }
        }
    }
}