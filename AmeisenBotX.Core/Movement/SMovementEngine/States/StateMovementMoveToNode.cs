using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Movement.SMovementEngine.States
{
    public class StateMovementMoveToNode : BasicMovementState
    {
        public StateMovementMoveToNode(StateBasedMovementEngine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
            LastPositionEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        private Vector3 LastPosition { get; set; }

        private TimegatedEvent LastPositionEvent { get; }

        private Vector3 StartPosition { get; set; }

        private Vector3 TargetPosition { get; set; }

        public override void Enter()
        {
            TargetPosition = default;
            StartPosition = default;
        }

        public override void Execute()
        {
            if (StateMachine.MovementAction == MovementAction.DirectMoving)
            {
                WowInterface.CharacterManager.MoveToPosition(StateMachine.TargetPosition);
                return;
            }

            if (StateMachine.Path?.Count == 0 && TargetPosition == default)
            {
                StateMachine.SetState((int)MovementState.None);
            }
            else
            {
                double distanceToNode = WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition);

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
                            StateMachine.Nodes.Dequeue();
                        }

                        TargetPosition = default;
                        return;
                    }

                    List<Vector3> forces = GetForces(TargetPosition, StateMachine.TargetRotation);
                    StateMachine.PlayerVehicle.Update(forces);

                    if (LastPositionEvent.Run())
                    {
                        double distanceMovedSinceLastTick = WowInterface.ObjectManager.Player.Position.GetDistance(LastPosition);

                        if (distanceMovedSinceLastTick < 0.1)
                        {
                            WowInterface.CharacterManager.Jump();
                        }

                        LastPosition = WowInterface.ObjectManager.Player.Position;
                    }
                }
            }
        }

        public override void Exit()
        {
        }

        private List<Vector3> GetForces(Vector3 targetPosition, float rotation = 0f)
        {
            List<Vector3> forces = new List<Vector3>();

            switch (StateMachine.MovementAction)
            {
                case MovementAction.Moving:
                    forces.Add(StateMachine.PlayerVehicle.Seek(targetPosition, 1f));
                    forces.Add(StateMachine.PlayerVehicle.AvoidObstacles(2f));
                    break;

                case MovementAction.Following:
                    forces.Add(StateMachine.PlayerVehicle.Seek(targetPosition, 1f));
                    forces.Add(StateMachine.PlayerVehicle.Seperate(1f));
                    forces.Add(StateMachine.PlayerVehicle.AvoidObstacles(2f));
                    break;

                case MovementAction.Chasing:
                    forces.Add(StateMachine.PlayerVehicle.Seek(targetPosition, 1f));
                    break;

                case MovementAction.Fleeing:
                    forces.Add(StateMachine.PlayerVehicle.Flee(targetPosition, 1f));
                    break;

                case MovementAction.Evading:
                    forces.Add(StateMachine.PlayerVehicle.Evade(targetPosition, 1f, rotation));
                    break;

                case MovementAction.Wandering:
                    forces.Add(StateMachine.PlayerVehicle.Wander(1f));
                    break;

                case MovementAction.Stuck:
                    forces.Add(StateMachine.PlayerVehicle.Unstuck(1f));
                    break;

                default:
                    break;
            }

            return forces;
        }
    }
}