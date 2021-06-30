using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class SitToChairIdleAction : IIdleAction
    {
        public SitToChairIdleAction(AmeisenBotInterfaces bot, AmeisenBotFsm stateMachine, double maxDistance)
        {
            Bot = bot;
            StateMachine = stateMachine;
            MaxDistance = maxDistance;
        }

        public bool AutopilotOnly => false;

        public AmeisenBotInterfaces Bot { get; }

        public int MaxCooldown => 16 * 1000;

        public int MaxDuration => 90 * 1000;

        public int MinCooldown => 6 * 1000;

        public int MinDuration => 25 * 1000;

        private WowGameobject CurrentSeat { get; set; }

        private double MaxDistance { get; }

        private bool SatDown { get; set; }

        private AmeisenBotFsm StateMachine { get; }

        public bool Enter()
        {
            SatDown = false;

            // get the center from where to cal the distance, this is needed
            // to prevent going out of the follow trigger radius, which
            // would cause a suspicous loop of running around
            Vector3 originPos = StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit unit, false) ? unit.Position : Bot.Player.Position;

            WowGameobject seat = Bot.Objects.WowObjects.OfType<WowGameobject>()
                .OrderBy(e => e.Position.GetDistance(originPos))
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Chair
                    // make sure no one sits on the chair besides ourself
                    && !Bot.Objects.WowObjects.OfType<WowUnit>()
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
                    Bot.Wow.WowObjectRightClick(CurrentSeat.BaseAddress);

                    SatDown = true;
                }
            }
        }
    }
}