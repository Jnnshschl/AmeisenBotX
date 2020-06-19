using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;

namespace AmeisenBotX.Core.Movement.SMovementEngine.States
{
    public class StateMovementMoveToNode : BasicMovementState
    {
        public StateMovementMoveToNode(StateBasedMovementEngine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            LastPositionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            UnstuckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1700));
            JumpCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
        }

        private Vector3 LastCompletedPosition { get; set; }

        private double LastDistance { get; set; }

        private Vector3 LastPosition { get; set; }

        private TimegatedEvent LastPositionEvent { get; }

        private Vector3 StartPosition { get; set; }

        private Vector3 TargetPosition { get; set; }

        private TimegatedEvent JumpCheckEvent { get; }

        private TimegatedEvent UnstuckEvent { get; }

        public override void Enter()
        {
            TargetPosition = default;
            StartPosition = default;
            LastPosition = default;
            LastCompletedPosition = default;
            LastDistance = 0.0;
        }

        public override void Execute()
        {
            // if (!UnstuckEvent.Ready)
            // {
            //     return;
            // }

            if (StateMachine.MovementAction == MovementAction.DirectMove && StateMachine.FinalTargetPosition != default)
            {
                double distanceToFinalNode = WowInterface.ObjectManager.Player.Position.GetDistanceIgnoreZ(StateMachine.FinalTargetPosition);

                if (distanceToFinalNode < StateMachine.MovementSettings.WaypointCheckThreshold)
                {
                    StateMachine.Reset();
                    // WowInterface.HookManager.StopClickToMoveIfActive(WowInterface.ObjectManager.Player);
                    return;
                }

                WowInterface.CharacterManager.MoveToPosition(StateMachine.FinalTargetPosition);
                return;
            }

            double distanceToNode = WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition);

            if (StateMachine.Path?.Count == 0 && TargetPosition == default
                || (LastDistance > 0.0 && (LastDistance + 3.0) < distanceToNode)) // maybe we fell down somewhere
            {
                StateMachine.Reset();
            }
            else
            {
                distanceToNode = WowInterface.ObjectManager.Player.Position.GetDistanceIgnoreZ(TargetPosition);

                if (TargetPosition == default)
                {
                    TargetPosition = StateMachine.Nodes.Peek();
                    StartPosition = WowInterface.ObjectManager.Player.Position;
                    return;
                }
                else
                {
                    if (distanceToNode < StateMachine.MovementSettings.WaypointCheckThreshold)
                    {
                        if (StateMachine.Nodes.Count > 0)
                        {
                            LastCompletedPosition = StateMachine.Nodes.Dequeue();
                            LastDistance = 0.0;
                        }
                        else
                        {
                            StateMachine.Reset();
                        }

                        TargetPosition = default;
                        return;
                    }

                    // move the character
                    WowInterface.CharacterManager.MoveToPosition(BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, TargetPosition, 1.0));

                    // check wether we need to jump up or down
                    double distanceToNodeIgnoreZ = WowInterface.ObjectManager.Player.Position.GetDistanceIgnoreZ(TargetPosition);

                    if (distanceToNodeIgnoreZ < 1.5)
                    {
                        double zDiff = StateMachine.VehicleTargetPosition.Z - WowInterface.ObjectManager.Player.Position.Z;

                        if (zDiff > 2)
                        {
                            // target position is above us, jump up
                            WowInterface.CharacterManager.Jump();
                        }
                        else if (zDiff < -2)
                        {
                            // target position is below us, jump down
                            WowInterface.CharacterManager.Jump();
                        }
                    }


                    if (JumpCheckEvent.Run())
                    {
                        Vector3 extends = new Vector3(WowInterface.ObjectManager.Player.Position);
                        extends = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Rotation, extends, 2.0);

                        if (!WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, extends, 0.1f))
                        {
                            WowInterface.CharacterManager.Jump();
                        }
                    }

                    // check for beeing stuck
                    if (LastPositionEvent.Run())
                    {
                        double distanceMovedSinceLastTick = WowInterface.ObjectManager.Player.Position.GetDistanceIgnoreZ(LastPosition);

                        if (distanceMovedSinceLastTick < 0.3)
                        {
                            WowInterface.CharacterManager.Jump();

                            // get a random position behind us
                            double angle = Math.PI + ((new Random().NextDouble() * Math.PI) - Math.PI / 2.0);
                            WowInterface.CharacterManager.MoveToPosition(BotMath.CalculatePositionAround(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, angle, 4.0));
                            // UnstuckEvent.Run();
                        }

                        LastPosition = WowInterface.ObjectManager.Player.Position;
                        LastDistance = WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition);
                    }
                }
            }
        }

        public override void Exit()
        {
        }
    }
}