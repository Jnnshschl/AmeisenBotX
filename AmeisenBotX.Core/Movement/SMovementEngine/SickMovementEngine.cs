using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.Settings;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace AmeisenBotX.Core.Movement.SMovementEngine
{
    public class SickMovementEngine : IMovementEngine
    {
        public SickMovementEngine(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Nodes = new Queue<Vector3>();
            PlayerVehicle = new BasicVehicle(wowInterface, WowInterface.MovementSettings.MaxSteering, WowInterface.MovementSettings.MaxVelocity, WowInterface.MovementSettings.MaxAcceleration);

            LastDistanceEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            JumpCheckEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));

            Blackboard = new MovementBlackboard(UpdateBlackboard);
            BehaviorTree = new AmeisenBotBehaviorTree<MovementBlackboard>
            (
                "MovementTree",
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
                                (b) => Nodes.Peek().GetDistance(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold,
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
                ),
                Blackboard
            );
        }

        private bool IsDirectMovingState()
        {
            return MovementAction != MovementAction.Moving
                && MovementAction != MovementAction.Following;
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

        private void UpdateBlackboard()
        {

        }

        private bool DoINeedToFindAPath()
        {
            if (MovementAction == MovementAction.DirectMove
                || MovementAction == MovementAction.Moving
                || MovementAction == MovementAction.Following)
            {
                return Path == null || Path.Count == 0 || TargetPositionLastPathfinding.GetDistance(TargetPosition) > 1.5;
            }
            else
            {
                return false;
            }
        }

        public AmeisenBotBehaviorTree<MovementBlackboard> BehaviorTree { get; }

        public MovementBlackboard Blackboard { get; }

        public Vector3 TargetPosition { get; private set; }

        public Vector3 TargetPositionLastPathfinding { get; private set; }

        public float TargetRotation { get; private set; }

        public double MovedDistance { get; private set; }

        public bool JumpOnNextMove { get; private set; }

        public Vector3 LastPlayerPosition { get; private set; }

        public bool IsAtTargetPosition => TargetPosition != default && TargetPosition.GetDistanceIgnoreZ(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold;

        public MovementAction MovementAction { get; private set; }

        public Queue<Vector3> Nodes { get; private set; }

        public List<Vector3> Path => Nodes.ToList();

        internal BasicVehicle PlayerVehicle { get; }

        internal WowInterface WowInterface { get; }

        private TimegatedEvent LastDistanceEvent { get; }

        private TimegatedEvent JumpCheckEvent { get; }

        public void Reset()
        {
            MovementAction = MovementAction.None;

            if (Nodes.Count > 0)
            {
                Nodes.Clear();
            }
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f)
        {
            MovementAction = movementAction;
            TargetPosition = positionToGoTo;
            TargetRotation = targetRotation;
        }

        public void Execute()
        {
            if (MovementAction != MovementAction.None)
            {
                if (LastDistanceEvent.Run())
                {
                    MovedDistance = LastPlayerPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
                    LastPlayerPosition = WowInterface.ObjectManager.Player.Position;

                    if (MovedDistance > 0.0 && MovedDistance < 0.2)
                    {
                        JumpOnNextMove = true;
                    }
                }

                if (!JumpOnNextMove
                    && JumpCheckEvent.Run()
                    && !WowInterface.HookManager.IsInLineOfSight
                    (
                        WowInterface.ObjectManager.Player.Position,
                        BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Rotation, WowInterface.ObjectManager.Player.Position, 0.2),
                        0.3f
                    ))
                {
                    JumpOnNextMove = true;
                }

                BehaviorTree.Tick();
            }
        }
    }
}