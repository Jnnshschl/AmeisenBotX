using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class SitToChairIdleAction : IIdleAction
    {
        public SitToChairIdleAction(WowInterface wowInterface, AmeisenBotFsm stateMachine, double maxDistance)
        {
            WowInterface = wowInterface;
            StateMachine = stateMachine;
            MaxDistance = maxDistance;
        }

        public bool AutopilotOnly => false;

        public int MaxCooldown => 16 * 1000;

        public int MaxDuration => 90 * 1000;

        public int MinCooldown => 6 * 1000;

        public int MinDuration => 25 * 1000;

        public WowInterface WowInterface { get; }

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
            Vector3 originPos = StateMachine.GetState<StateIdle>().IsUnitToFollowThere(out WowUnit unit, false) ? unit.Position : WowInterface.ObjectManager.Player.Position;

            WowGameobject seat = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                .OrderBy(e => e.Position.GetDistance(originPos))
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.Chair
                    // make sure no one sits on the chair besides ourself
                    && !WowInterface.ObjectManager.WowObjects.OfType<WowUnit>()
                        .Where(e => e.Guid != WowInterface.ObjectManager.PlayerGuid)
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
                if (CurrentSeat.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 1.5f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentSeat.Position);
                }
                else
                {
                    WowInterface.MovementEngine.StopMovement();
                    WowInterface.HookManager.WowObjectRightClick(CurrentSeat);

                    SatDown = true;
                }
            }
        }
    }
}