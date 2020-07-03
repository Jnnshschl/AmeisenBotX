using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.SMovementEngine
{
    public class SickMovementEngine : IMovementEngine
    {
        public SickMovementEngine(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Nodes = new Queue<Vector3>();
            PlayerVehicle = new BasicVehicle(wowInterface);

            LastDistanceEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            JumpCheckEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            PathDecayEvent = new TimegatedEvent(TimeSpan.FromSeconds(4));

            Blackboard = new MovementBlackboard(UpdateBlackboard);
            BehaviorTree = new AmeisenBotBehaviorTree<MovementBlackboard>
            (
                "MovementTree",
                new Selector<MovementBlackboard>
                (
                    "DoINeedToMove",
                    (b) => WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) > MinDistanceToMove,
                    new Selector<MovementBlackboard>
                    (
                        "NeedToUnstuck",
                        (b) => StuckCounter > WowInterface.MovementSettings.StuckCounterUnstuck,
                        new Leaf<MovementBlackboard>((b) => DoUnstuck()),
                        new Selector<MovementBlackboard>
                        (
                            "NeedToJump",
                            (b) => JumpOnNextMove,
                            new Leaf<MovementBlackboard>((b) =>
                            {
                                WowInterface.CharacterManager.Jump();
                                JumpOnNextMove = false;
                                return BehaviorTreeStatus.Success;
                            }),
                            new Selector<MovementBlackboard>
                            (
                                "IsDirectMovingState",
                                (b) => IsDirectMovingState(),
                                new Leaf<MovementBlackboard>((b) =>
                                {
                                    if (Nodes.Count > 0)
                                    {
                                        Nodes.Clear();
                                    }

                                    PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction, TargetPosition, TargetRotation);
                                    return WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) < WowInterface.MovementSettings.WaypointCheckThreshold ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Ongoing;
                                }),
                                new Selector<MovementBlackboard>
                                (
                                    "DoINeedToFindAPath",
                                    (b) => DoINeedToFindAPath(),
                                    new Leaf<MovementBlackboard>("FindPathToTargetPosition", FindPathToTargetPosition),
                                    new Selector<MovementBlackboard>
                                    (
                                        "NeedToCheckANode",
                                        (b) => Nodes.Peek().GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold,
                                        new Leaf<MovementBlackboard>("CheckWaypoint", (b) =>
                                        {
                                            Nodes.Dequeue();

                                            if (Nodes.Count == 0)
                                            {
                                                MovementAction = MovementAction.None;
                                            }

                                            return BehaviorTreeStatus.Success;
                                        }),
                                        new Leaf<MovementBlackboard>("Move", (b) =>
                                        {
                                            PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction, Nodes.Peek(), TargetRotation);
                                            return BehaviorTreeStatus.Ongoing;
                                        })
                                    )
                                )
                            )
                        )
                    ),
                    new Leaf<MovementBlackboard>((b) => { return BehaviorTreeStatus.Success; })
                ),
                Blackboard
            );
        }

        public AmeisenBotBehaviorTree<MovementBlackboard> BehaviorTree { get; }

        public MovementBlackboard Blackboard { get; }

        public bool IsAtTargetPosition => TargetPosition != default && TargetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold;

        public bool JumpOnNextMove { get; private set; }

        public Vector3 LastPlayerPosition { get; private set; }

        public double MinDistanceToMove { get; private set; }

        public double MovedDistance { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public Queue<Vector3> Nodes { get; private set; }

        public List<Vector3> Path => Nodes.ToList();

        public int StuckCounter { get; private set; }

        public Vector3 StuckPosition { get; private set; }

        public float StuckRotation { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public Vector3 TargetPositionLastPathfinding { get; private set; }

        public float TargetRotation { get; private set; }

        public Vector3 UnstuckTargetPosition { get; private set; }

        public TimegatedEvent PathDecayEvent { get; private set; }

        internal BasicVehicle PlayerVehicle { get; }

        internal WowInterface WowInterface { get; }

        private TimegatedEvent JumpCheckEvent { get; }

        private TimegatedEvent LastDistanceEvent { get; }

        public void Execute()
        {
            if (MovementAction != MovementAction.None)
            {
                if (WowInterface.MovementSettings.EnableDistanceMovedJumpCheck
                    && !JumpOnNextMove
                    && LastDistanceEvent.Run())
                {
                    MovedDistance = LastPlayerPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
                    LastPlayerPosition = WowInterface.ObjectManager.Player.Position;

                    if (MovedDistance > WowInterface.MovementSettings.MinDistanceMovedJumpUnstuck && MovedDistance < WowInterface.MovementSettings.MaxDistanceMovedJumpUnstuck)
                    {
                        ++StuckCounter;
                        JumpOnNextMove = true;
                    }
                    else
                    {
                        StuckCounter = 0;
                    }
                }

                if (WowInterface.MovementSettings.EnableTracelineJumpCheck
                    && !JumpOnNextMove
                    && JumpCheckEvent.Run())
                {

                    Vector3 pos = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, WowInterface.MovementSettings.JumpCheckDistance);

                    if (!WowInterface.HookManager.IsInLineOfSight
                    (
                        WowInterface.ObjectManager.Player.Position,
                        pos,
                        WowInterface.MovementSettings.JumpCheckHeight
                    ))
                    {
                        JumpOnNextMove = true;
                    }
                }

                BehaviorTree.Tick();
            }
        }

        public void Reset()
        {
            MovementAction = MovementAction.None;
            TargetPosition = Vector3.Zero;
            TargetRotation = 0f;

            StuckCounter = 0;
            StuckPosition = default;

            if (Nodes.Count > 0)
            {
                Nodes.Clear();
            }
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f, double minDistanceToMove = 1.5)
        {
            MovementAction = movementAction;
            TargetPosition = positionToGoTo;
            TargetRotation = targetRotation;
            MinDistanceToMove = minDistanceToMove;
        }

        public void StopMovement()
        {
            WowInterface.HookManager.StopClickToMoveIfActive();
            Reset();
        }

        private bool DoINeedToFindAPath()
        {
            if (MovementAction == MovementAction.DirectMove
                || MovementAction == MovementAction.Moving
                || MovementAction == MovementAction.Following)
            {
                return Path == null || Path.Count == 0 || PathDecayEvent.Run() || TargetPositionLastPathfinding.GetDistance(TargetPosition) > 1.5;
            }
            else
            {
                return false;
            }
        }

        private BehaviorTreeStatus DoUnstuck()
        {
            if (StuckPosition == default)
            {
                StuckPosition = WowInterface.ObjectManager.Player.Position;
                StuckRotation = WowInterface.ObjectManager.Player.Rotation;

                double angle = Math.PI + ((new Random().NextDouble() * Math.PI) - Math.PI / 2.0);
                UnstuckTargetPosition = BotMath.CalculatePositionAround(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, (float)angle, 10.0f);
            }
            else
            {
                if (StuckPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.MinUnstuckDistance)
                {
                    PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction.DirectMove, UnstuckTargetPosition);
                    WowInterface.CharacterManager.Jump();
                }
                else
                {
                    Reset();
                    return BehaviorTreeStatus.Success;
                }
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus FindPathToTargetPosition(MovementBlackboard blackboard)
        {
            Nodes.Clear();
            List<Vector3> path = WowInterface.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

            if (path != null && path.Count > 0)
            {
                for (int i = 0; i < path.Count; ++i)
                {
                    Nodes.Enqueue(path[i]);
                }

                TargetPositionLastPathfinding = TargetPosition;
                return BehaviorTreeStatus.Success;
            }
            else
            {
                return BehaviorTreeStatus.Failed;
            }
        }

        private bool IsDirectMovingState()
        {
            return MovementAction != MovementAction.Moving
                && MovementAction != MovementAction.Following;
        }

        private void UpdateBlackboard()
        {
        }
    }
}