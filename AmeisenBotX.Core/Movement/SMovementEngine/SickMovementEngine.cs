using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Extra.Shortcuts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace AmeisenBotX.Core.Movement.SMovementEngine
{
    public class SickMovementEngine : IMovementEngine
    {
        public SickMovementEngine(WowInterface wowInterface, AmeisenBotConfig config)
        {
            WowInterface = wowInterface;
            Nodes = new ConcurrentQueue<Vector3>();
            PlayerVehicle = new BasicVehicle(wowInterface);

            Shortcuts = new List<IShortcut>()
            {
                // new DeeprunTramShortcut(wowInterface)
            };

            MovementWatchdog = new Timer(1000);
            MovementWatchdog.Elapsed += MovementWatchdog_Elapsed;

            if (WowInterface.MovementSettings.EnableDistanceMovedJumpCheck)
            {
                MovementWatchdog.Start();
            }

            PathRefreshEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            JumpCheckEvent = new TimegatedEvent(TimeSpan.FromSeconds(1));
            MountCheck = new TimegatedEvent(TimeSpan.FromSeconds(3));
            PathDecayEvent = new TimegatedEvent(TimeSpan.FromSeconds(10));

            Blackboard = new MovementBlackboard(UpdateBlackboard);
            BehaviorTree = new AmeisenBotBehaviorTree<MovementBlackboard>
            (
                "MovementTree",
                new Selector<MovementBlackboard>
                (
                    "DoINeedToMove",
                    (b) => WowInterface.ObjectManager.Player.Position.GetDistance2D(TargetPosition) > MinDistanceToMove,
                    new Selector<MovementBlackboard>
                    (
                        "NeedToUnstuck",
                        (b) => ShouldBeMoving && !ForceDirectMove && StuckCounter > WowInterface.MovementSettings.StuckCounterUnstuck,
                        new Leaf<MovementBlackboard>
                        (
                            (b) => DoUnstuck()
                        ),
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
                                (b) => IsDirectMovingState() || WowInterface.ObjectManager.Player.IsSwimming || WowInterface.ObjectManager.Player.IsFlying || ForceDirectMove,
                                new Leaf<MovementBlackboard>((b) =>
                                {
                                    if (Nodes.TryPeek(out Vector3 checkNodePos)
                                        && checkNodePos.GetDistance2D(WowInterface.ObjectManager.Player.Position) < (WowInterface.ObjectManager.Player.IsMounted ? WowInterface.MovementSettings.WaypointCheckThresholdMounted : WowInterface.MovementSettings.WaypointCheckThreshold))
                                    {
                                        Nodes.TryDequeue(out _);
                                    }

                                    Vector3 targetPos = Nodes.Count > 1 && Nodes.TryPeek(out Vector3 node) ? node : TargetPosition;

                                    ShouldBeMoving = true;

                                    Vector3 positionToGoTo;

                                    if (WowInterface.ObjectManager.Player.IsSwimming || WowInterface.ObjectManager.Player.IsFlying)
                                    {
                                        PlayerVehicle.IsOnWaterSurface = WowInterface.ObjectManager.Player.IsSwimming && !WowInterface.ObjectManager.Player.IsUnderwater;

                                        positionToGoTo = targetPos;
                                    }
                                    else
                                    {
                                        positionToGoTo = targetPos;  // WowInterface.PathfindingHandler.MoveAlongSurface((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);
                                    }

                                    PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction, positionToGoTo, TargetRotation);

                                    return WowInterface.ObjectManager.Player.Position.GetDistance2D(TargetPosition) < WowInterface.MovementSettings.WaypointCheckThreshold
                                           ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Ongoing;
                                }),
                                new Selector<MovementBlackboard>
                                (
                                    "DoINeedToFindAPath",
                                    (b) => DoINeedToFindAPath() && (Nodes == null || Nodes.Count == 0 || PathRefreshEvent.Run()),
                                    new Leaf<MovementBlackboard>
                                    (
                                        "FindPathToTargetPosition",
                                        FindPathToTargetPosition
                                    ),
                                    new Selector<MovementBlackboard>
                                    (
                                        "NeedToCheckANode",
                                        (b) => Nodes.TryPeek(out Vector3 node) && node.GetDistance2D(WowInterface.ObjectManager.Player.Position) < (WowInterface.ObjectManager.Player.IsMounted ? WowInterface.MovementSettings.WaypointCheckThresholdMounted : WowInterface.MovementSettings.WaypointCheckThreshold),
                                        new Leaf<MovementBlackboard>("CheckWaypoint", (b) =>
                                        {
                                            if (Nodes.TryDequeue(out _))
                                            {
                                                if (Nodes.Count == 0)
                                                {
                                                    MovementAction = MovementAction.None;
                                                }

                                                return BehaviorTreeStatus.Success;
                                            }
                                            else
                                            {
                                                return BehaviorTreeStatus.Failed;
                                            }
                                        }),
                                        new Leaf<MovementBlackboard>("Move", (b) =>
                                        {
                                            if (config.UseMounts)
                                            {
                                                if (MountCheck.Run() || IsCastingMount)
                                                {
                                                    if (!WowInterface.ObjectManager.Player.HasBuffByName("Warsong Flag")
                                                    && !WowInterface.ObjectManager.Player.HasBuffByName("Silverwing Flag")
                                                    && !IsGhost
                                                    && wowInterface.CharacterManager.Mounts?.Count > 0
                                                    && TargetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) > (WowInterface.Globals.IgnoreMountDistance ? 5.0 : 80.0)
                                                    && !WowInterface.ObjectManager.Player.IsMounted
                                                    && wowInterface.HookManager.IsOutdoors())
                                                    {
                                                        List<WowMount> filteredMounts;

                                                        if (config.UseOnlySpecificMounts)
                                                        {
                                                            filteredMounts = WowInterface.CharacterManager.Mounts.Where(e => config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(e.Name)).ToList();
                                                        }
                                                        else
                                                        {
                                                            filteredMounts = WowInterface.CharacterManager.Mounts;
                                                        }

                                                        if (filteredMounts != null && filteredMounts.Count >= 0)
                                                        {
                                                            WowMount mount = filteredMounts[new Random().Next(0, filteredMounts.Count)];
                                                            WowInterface.MovementEngine.StopMovement();
                                                            IsCastingMount = true;
                                                            WowInterface.HookManager.Mount(mount.Index);
                                                        }

                                                        return BehaviorTreeStatus.Ongoing;
                                                    }
                                                }

                                                if (IsCastingMount)
                                                {
                                                    if (wowInterface.ObjectManager.Player.IsCasting)
                                                    {
                                                        return BehaviorTreeStatus.Ongoing;
                                                    }
                                                    else
                                                    {
                                                        IsCastingMount = false;
                                                    }
                                                }
                                            }

                                            if (Nodes.TryPeek(out Vector3 node))
                                            {
                                                ShouldBeMoving = true;
                                                PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction, node, TargetRotation);
                                                return BehaviorTreeStatus.Ongoing;
                                            }
                                            else
                                            {
                                                ShouldBeMoving = false;
                                                return BehaviorTreeStatus.Failed;
                                            }
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

        public IShortcut ActiveShortcut { get; set; }

        public AmeisenBotBehaviorTree<MovementBlackboard> BehaviorTree { get; }

        public MovementBlackboard Blackboard { get; }

        public bool ForceDirectMove { get; private set; }

        public bool IsAtTargetPosition => TargetPosition != default && TargetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold;

        public bool IsCastingMount { get; set; }

        public bool IsGhost { get; set; }

        public bool IsPathIncomplete { get; private set; }

        public bool JumpOnNextMove { get; private set; }

        public Vector3 LastPlayerPosition { get; private set; }

        public double MinDistanceToMove { get; private set; }

        public TimegatedEvent MountCheck { get; }

        public bool MountInProgress { get; private set; }

        public double MovedDistance { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public ConcurrentQueue<Vector3> Nodes { get; private set; }

        public List<Vector3> Path => Nodes.ToList();

        public TimegatedEvent PathDecayEvent { get; private set; }

        public TimegatedEvent PathRefreshEvent { get; }

        public BasicVehicle PlayerVehicle { get; }

        public Vector3 ShortcutPosition { get; private set; }

        public bool ShouldBeMoving { get; private set; }

        public int StuckCounter { get; private set; }

        public Vector3 StuckPosition { get; private set; }

        public float StuckRotation { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public Vector3 TargetPositionLastPathfinding { get; private set; }

        public float TargetRotation { get; private set; }

        public Vector3 UnstuckTargetPosition { get; private set; }

        private TimegatedEvent JumpCheckEvent { get; }

        private Timer MovementWatchdog { get; }

        private bool PathfindingFailed { get; set; }

        private List<IShortcut> Shortcuts { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (MovementAction != MovementAction.None)
            {
                // check for obstacles in our way
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

                if (ShortcutPosition == default)
                {
                    ShortcutPosition = TargetPosition;
                }

                if (ActiveShortcut != null && ActiveShortcut.UseShortcut(WowInterface.ObjectManager.Player.Position, ShortcutPosition, out Vector3 shortcutPosition, out bool shortcutUsePathfinding))
                {
                    TargetPosition = shortcutPosition;

                    if (!shortcutUsePathfinding)
                    {
                        Nodes.Clear();
                        MovementAction = MovementAction.DirectMove;
                        ForceDirectMove = true;
                    }
                }
                else
                {
                    ShortcutPosition = default;
                }

                BehaviorTree.Tick();
                ForceDirectMove = false;
            }
        }

        public bool HasCompletePathToPosition(Vector3 position, double maxDistance)
        {
            return WowInterface.ObjectManager.Player.IsSwimming || WowInterface.ObjectManager.Player.IsFlying || (Path != null && Path.Count > 0 && Path.Last().GetDistance2D(position) < maxDistance);
        }

        public void Reset()
        {
            MovementAction = MovementAction.None;
            TargetPosition = Vector3.Zero;
            TargetRotation = 0f;

            StuckCounter = 0;
            StuckPosition = default;
            ShouldBeMoving = false;

            if (Nodes.Count > 0)
            {
                Nodes.Clear();
            }
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f, double minDistanceToMove = 1.5, bool disableShortcuts = false)
        {
            if (ActiveShortcut != null && (disableShortcuts || ActiveShortcut.Finished))
            {
                ActiveShortcut = null;
            }

            if (MovementAction == movementAction
                && TargetPosition == positionToGoTo
                && TargetRotation == targetRotation
                && MinDistanceToMove == minDistanceToMove)
            {
                return;
            }

            PathfindingFailed = false;

            MovementAction = movementAction;
            TargetPosition = positionToGoTo;
            TargetRotation = targetRotation;
            MinDistanceToMove = minDistanceToMove;

            double distanceToTargetPos = WowInterface.ObjectManager.Player.Position.GetDistance(positionToGoTo) * 2.0;

            if (!disableShortcuts && Shortcuts.Any(e => distanceToTargetPos >= e.MinDistanceUntilWorth))
            {
                IShortcut useableShortcut = Shortcuts.FirstOrDefault(e => e.MapToUseOn == WowInterface.ObjectManager.MapId && e.IsUseable(WowInterface.ObjectManager.Player.Position, positionToGoTo));

                if (useableShortcut != null)
                {
                    ActiveShortcut = useableShortcut;
                }
            }
        }

        public void StopMovement()
        {
            WowInterface.HookManager.StopClickToMoveIfActive();
            Reset();
        }

        private bool DoINeedToFindAPath()
        {
            return Path == null
                || Path.Count == 0
                || PathDecayEvent.Run()
                || TargetPositionLastPathfinding.GetDistance2D(TargetPosition) > 1.0;
        }

        private BehaviorTreeStatus DoUnstuck()
        {
            if (StuckPosition == default)
            {
                StuckPosition = WowInterface.ObjectManager.Player.Position;
                StuckRotation = WowInterface.ObjectManager.Player.Rotation;

                Vector3 pos = BotMath.CalculatePositionBehind(WowInterface.ObjectManager.Player.Position, StuckRotation, WowInterface.MovementSettings.UnstuckDistance);
                UnstuckTargetPosition = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, pos, 4f);
            }
            else
            {
                if (StuckPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) > 0.0
                    && StuckPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.MinUnstuckDistance)
                {
                    PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction.Moving, UnstuckTargetPosition);
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
            if (PathfindingFailed)
            {
                return BehaviorTreeStatus.Failed;
            }

            List<Vector3> path = WowInterface.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

            if (path != null && path.Count > 0)
            {
                ConcurrentQueue<Vector3> newPath = new ConcurrentQueue<Vector3>();

                for (int i = 0; i < path.Count; ++i)
                {
                    newPath.Enqueue(path[i]);
                }

                Nodes = newPath;

                IsPathIncomplete = Nodes.Last().GetDistance(TargetPosition) > 3.0;
                TargetPositionLastPathfinding = TargetPosition;
                return BehaviorTreeStatus.Success;
            }
            else
            {
                PathfindingFailed = true;
                IsPathIncomplete = true;
                return BehaviorTreeStatus.Failed;
            }
        }

        private bool IsDirectMovingState()
        {
            return MovementAction != MovementAction.Moving
                && MovementAction != MovementAction.Following;
        }

        private void MovementWatchdog_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (MovementAction == MovementAction.None)
            {
                ShouldBeMoving = false;
                return;
            }

            // check wether we should be moving or not
            if (ShouldBeMoving)
            {
                // if we already need to jump, dont check it again
                if (!JumpOnNextMove)
                {
                    MovedDistance = LastPlayerPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position);
                    LastPlayerPosition = WowInterface.ObjectManager.Player.Position;

                    if (MovedDistance > WowInterface.MovementSettings.MinDistanceMovedJumpUnstuck
                        && MovedDistance < WowInterface.MovementSettings.MaxDistanceMovedJumpUnstuck)
                    {
                        ++StuckCounter;
                        JumpOnNextMove = true;
                    }
                    else
                    {
                        StuckCounter = 0;
                    }
                }
            }
            else
            {
                StuckCounter = 0;
            }
        }

        private void UpdateBlackboard()
        {
        }
    }
}