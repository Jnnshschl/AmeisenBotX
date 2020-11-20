using AmeisenBotX.BehaviorTree;
using AmeisenBotX.BehaviorTree.Enums;
using AmeisenBotX.BehaviorTree.Objects;
using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using AmeisenBotX.Core.Movement.SMovementEngine.Extra.Shortcuts;
using AmeisenBotX.Logging;
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
            Config = config;

            PathNodes = new ConcurrentQueue<Vector3>();
            PlayerVehicle = new BasicVehicle(wowInterface);

            Shortcuts = new List<IShortcut>()
            {
                // new DeeprunTramShortcut(wowInterface)
            };

            if (WowInterface.MovementSettings.EnableDistanceMovedJumpCheck)
            {
                MovementWatchdog = new Timer(250);
                MovementWatchdog.Elapsed += MovementWatchdog_Elapsed;
                MovementWatchdog.Start();
            }

            PathDecayEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(1000));
            ObstacleCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            MountCheck = new TimegatedEvent(TimeSpan.FromSeconds(3));

            BehaviorTree = new AmeisenBotBehaviorTree
            (
                "MovementTree",
                new Selector
                (
                    "DoINeedToMove",
                    () => TargetPosition != default
                        && !WowInterface.ObjectManager.Player.IsDead
                        && !WowInterface.ObjectManager.Player.IsCasting
                        && (WowInterface.ObjectManager.Vehicle == null || !WowInterface.ObjectManager.Vehicle.IsCasting)
                        && MovementAction != MovementAction.None
                        && DateTime.Now > PreventMovementUntil
                        && !IsNearPosition(TargetPosition),
                    new Selector
                    (
                        "DoINeedToJump",
                        () => JumpOnNextMove,
                        new Leaf(() =>
                        {
                            WowInterface.CharacterManager.Jump();
                            JumpOnNextMove = false;
                            return BehaviorTreeStatus.Success;
                        }),
                        new Selector
                        (
                            "DoINeedToUnstuck",
                            () => ShouldBeMoving && !ForceDirectMove && StuckCounter > WowInterface.MovementSettings.StuckCounterUnstuck,
                            new Leaf(DoUnstuck),
                            new Selector
                            (
                                "DoINeedToDirectlyMove",
                                () => IsDirectMovingState()
                                    || WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) < 3.0
                                    || WowInterface.ObjectManager.Player.IsSwimming
                                    || WowInterface.ObjectManager.Player.IsFlying
                                    || ForceDirectMove,
                                new Leaf(HandleDirectMoving),
                                new Selector
                                (
                                    "DoINeedToFindAPath",
                                    DoINeedToFindAPath,
                                    new Leaf
                                    (
                                        "FindPathToTargetPosition",
                                        FindPathToTargetPosition
                                    ),
                                    new Selector
                                    (
                                        "DoINeedToMount",
                                        DoINeedToMount,
                                        new Leaf("MountUp", MountUp),
                                        new Selector
                                        (
                                            "DoINeedToMove",
                                            () => PathNodes.TryPeek(out Vector3 node) && !IsNearPosition(node),
                                            new Leaf("MoveToNode", HandleMovement),
                                            new Leaf("CheckWaypoint", HandleWaypointCheck)
                                        )
                                    )
                                )
                            )
                        )
                    ),
                    new Leaf(() => { return BehaviorTreeStatus.Success; })
                )
            );
        }

        public IShortcut ActiveShortcut { get; set; }

        public AmeisenBotBehaviorTree BehaviorTree { get; }

        public bool ForceDirectMove { get; private set; }

        public bool IsAtTargetPosition => TargetPosition != default && TargetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.WaypointCheckThreshold;

        public bool IsCastingMount { get; set; }

        public bool IsGhost { get; set; }

        public bool JumpOnNextMove { get; private set; }

        public Vector3 LastPlayerPosition { get; private set; }

        public TimegatedEvent MountCheck { get; }

        public double MovedDistance { get; private set; }

        public MovementAction MovementAction { get; private set; }

        public List<Vector3> Path => PathNodes.ToList();

        public Vector3 PathFinalNode { get; private set; }

        public PathfindingStatus PathfindingStatus { get; private set; }

        public ConcurrentQueue<Vector3> PathNodes { get; private set; }

        public BasicVehicle PlayerVehicle { get; }

        public Vector3 ShortcutPosition { get; private set; }

        public bool ShouldBeMoving { get; private set; }

        public int StuckCounter { get; private set; }

        public Vector3 StuckPosition { get; private set; }

        public float StuckRotation { get; private set; }

        public Vector3 TargetPosition { get; private set; }

        public float TargetRotation { get; private set; }

        public bool Teleported { get; set; }

        public Vector3 UnstuckTargetPosition { get; private set; }

        private AmeisenBotConfig Config { get; }

        private Timer MovementWatchdog { get; }

        private TimegatedEvent ObstacleCheckEvent { get; }

        private TimegatedEvent PathDecayEvent { get; }

        private DateTime PreventMovementUntil { get; set; }

        private List<IShortcut> Shortcuts { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (MovementAction != MovementAction.None)
            {
                // check for obstacles in our way
                if (WowInterface.MovementSettings.EnableTracelineJumpCheck
                    && !JumpOnNextMove
                    && ObstacleCheckEvent.Run())
                {
                    Vector3 pos = BotUtils.MoveAhead(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, WowInterface.MovementSettings.ObstacleCheckDistance);

                    if (!WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, pos, WowInterface.MovementSettings.ObstacleCheckHeight))
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
                        PathNodes.Clear();
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
            return WowInterface.ObjectManager.Player.IsSwimming
                || WowInterface.ObjectManager.Player.IsFlying
                || (Path != null && Path.Count > 0 && Path.Last().GetDistance2D(position) < maxDistance);
        }

        public bool IsNearPosition(Vector3 position) => position.GetDistance2D(WowInterface.ObjectManager.Player.Position) < (WowInterface.ObjectManager.Player.IsMounted ? WowInterface.MovementSettings.WaypointCheckThresholdMounted : WowInterface.MovementSettings.WaypointCheckThreshold);

        public void PreventMovement(TimeSpan timeSpan)
        {
            PreventMovementUntil = DateTime.Now + timeSpan;
        }

        public void Reset()
        {
            AmeisenLogger.I.Log("Movement", "Resetting MovementEngine");

            MovementAction = MovementAction.None;
            PathfindingStatus = PathfindingStatus.None;
            TargetPosition = Vector3.Zero;
            TargetRotation = 0f;

            StuckCounter = 0;
            StuckPosition = default;
            ShouldBeMoving = false;

            if (!PathNodes.IsEmpty)
            {
                PathNodes.Clear();
            }
        }

        public void SetMovementAction(MovementAction movementAction, Vector3 positionToGoTo, float targetRotation = 0f, bool disableShortcuts = false, bool forceDirectMove = false)
        {
            if (ActiveShortcut != null && (disableShortcuts || ActiveShortcut.Finished))
            {
                ActiveShortcut = null;
            }

            ForceDirectMove = forceDirectMove;

            if (MovementAction == movementAction
                && TargetPosition == positionToGoTo
                && TargetRotation == targetRotation)
            {
                return;
            }

            MovementAction = movementAction;
            TargetPosition = positionToGoTo;
            TargetRotation = targetRotation;
            PathfindingStatus = PathfindingStatus.None;

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
            if (MovementAction != MovementAction.None)
            {
                AmeisenLogger.I.Log("Movement", "Stopping Movement");
                WowInterface.HookManager.WowStopClickToMove();
                ShouldBeMoving = false;

                Reset();
                PlayerVehicle.Reset();
            }
        }

        private bool DoINeedToFindAPath()
        {
            bool hasTeleported = Teleported;

            if (Teleported)
            {
                Teleported = false;
            }

            return PathDecayEvent.Run()
                || PathNodes == null
                || PathNodes.IsEmpty // we have no path
                || PathFinalNode.GetDistance(TargetPosition) > 3.0 // target position changed
                || hasTeleported;
        }

        private bool DoINeedToMount()
        {
            return Config.UseMounts
                && !WowInterface.ObjectManager.Player.IsMounted
                && !IsGhost
                && !WowInterface.ObjectManager.Player.IsInCombat
                && WowInterface.CharacterManager.Mounts != null
                && WowInterface.CharacterManager.Mounts.Any()
                && !WowInterface.ObjectManager.Player.HasBuffByName("Warsong Flag")
                && !WowInterface.ObjectManager.Player.HasBuffByName("Silverwing Flag")
                && TargetPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) > (WowInterface.Globals.IgnoreMountDistance ? 5.0 : 80.0)
                && WowInterface.HookManager.LuaIsOutdoors();
        }

        private BehaviorTreeStatus DoUnstuck()
        {
            if (StuckPosition == default || UnstuckTargetPosition == default)
            {
                StuckPosition = WowInterface.ObjectManager.Player.Position;
                StuckRotation = WowInterface.ObjectManager.Player.Rotation;

                Vector3 pos = BotMath.CalculatePositionBehind(WowInterface.ObjectManager.Player.Position, StuckRotation, WowInterface.MovementSettings.UnstuckDistance);
                UnstuckTargetPosition = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, pos, 4f);

                AmeisenLogger.I.Log("Movement", $"Calculated unstuck position: {UnstuckTargetPosition}");
            }
            else
            {
                if (StuckPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) > 0.0
                    && StuckPosition.GetDistance2D(WowInterface.ObjectManager.Player.Position) < WowInterface.MovementSettings.MinUnstuckDistance)
                {
                    AmeisenLogger.I.Log("Movement", "Unstucking");

                    PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction.Moving, UnstuckTargetPosition);
                    WowInterface.CharacterManager.Jump();
                }
                else
                {
                    AmeisenLogger.I.Log("Movement", "Unstuck finished");

                    Reset();
                    return BehaviorTreeStatus.Success;
                }
            }

            return BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus FindPathToTargetPosition()
        {
            AmeisenLogger.I.Log("Pathfinding", $"Finding path: {WowInterface.ObjectManager.Player.Position} -> {TargetPosition}");
            IEnumerable<Vector3> path = WowInterface.PathfindingHandler.GetPath((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, TargetPosition);

            if (path != null && path.Any())
            {
                int pathLenght = path.Count();

                PathNodes.Clear();

                for (int i = 0; i < pathLenght; ++i)
                {
                    PathNodes.Enqueue(path.ElementAt(i));
                }

                PathFinalNode = Path[pathLenght - 1];
                PathfindingStatus = PathFinalNode.GetDistance(TargetPosition) > 3.0 ? PathfindingStatus.PathIncomplete : PathfindingStatus.PathComplete;

                AmeisenLogger.I.Log("Pathfinding", $"Pathfinding status: {PathfindingStatus} Node count: {pathLenght}");
                return BehaviorTreeStatus.Success;
            }
            else
            {
                ShouldBeMoving = false;
                PathfindingStatus = PathfindingStatus.Failed;
                AmeisenLogger.I.Log("Pathfinding", "Pathfinding failed");
                return BehaviorTreeStatus.Failed;
            }
        }

        private BehaviorTreeStatus HandleDirectMoving()
        {
            if (ForceDirectMove)
            {
                WowInterface.CharacterManager.MoveToPosition(TargetPosition);
            }
            else
            {
                if (PathNodes.TryPeek(out Vector3 checkNodePos)
                    && IsNearPosition(checkNodePos))
                {
                    PathNodes.TryDequeue(out _);
                }

                // target pos is used to follow path in the water, we
                // need to move directly there to prevent going to
                // the top level of the water because we are
                // submerged a bit while swimming
                Vector3 targetPos = !PathNodes.IsEmpty && PathNodes.TryPeek(out Vector3 node) ? node : TargetPosition;

                if (WowInterface.ObjectManager.Player.IsSwimming || WowInterface.ObjectManager.Player.IsFlying)
                {
                    PlayerVehicle.IsOnWaterSurface = WowInterface.ObjectManager.Player.IsSwimming && !WowInterface.ObjectManager.Player.IsUnderwater;
                }
                else
                {
                    targetPos = WowInterface.PathfindingHandler.MoveAlongSurface((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, targetPos);
                }

                PlayerVehicle.Update((p) => WowInterface.CharacterManager.MoveToPosition(p), MovementAction, targetPos, TargetRotation);
                ShouldBeMoving = true;
            }

            return IsNearPosition(TargetPosition) ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Ongoing;
        }

        private BehaviorTreeStatus HandleMovement()
        {
            if (PathNodes.TryPeek(out Vector3 node))
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
        }

        private BehaviorTreeStatus HandleWaypointCheck()
        {
            if (PathNodes.TryDequeue(out _))
            {
                if (PathNodes.IsEmpty)
                {
                    StopMovement();
                }

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

        private BehaviorTreeStatus MountUp()
        {
            if (IsCastingMount)
            {
                if (WowInterface.ObjectManager.Player.IsCasting)
                {
                    return BehaviorTreeStatus.Ongoing;
                }
                else
                {
                    IsCastingMount = false;
                }
            }
            else if (MountCheck.Run())
            {
                IEnumerable<WowMount> filteredMounts;

                if (Config.UseOnlySpecificMounts)
                {
                    filteredMounts = WowInterface.CharacterManager.Mounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Contains(e.Name));
                }
                else
                {
                    filteredMounts = WowInterface.CharacterManager.Mounts;
                }

                if (filteredMounts != null && filteredMounts.Any())
                {
                    WowMount mount = filteredMounts.ElementAt(new Random().Next(0, filteredMounts.Count()));

                    StopMovement();

                    IsCastingMount = true;
                    WowInterface.HookManager.LuaCallCompanion(mount.Index);

                    return BehaviorTreeStatus.Ongoing;
                }
            }

            return WowInterface.ObjectManager.Player.IsMounted ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failed;
        }

        private void MovementWatchdog_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ForceDirectMove)
            {
                return;
            }

            if (WowInterface.ObjectManager.Player?.Position.GetDistance(LastPlayerPosition) > 25.0)
            {
                LastPlayerPosition = WowInterface.ObjectManager.Player.Position;
                Teleported = true;

                Reset();

                return;
            }

            if (MovementAction == MovementAction.None
                || WowInterface.ObjectManager.Player.IsCasting
                || IsNearPosition(TargetPosition))
            {
                ShouldBeMoving = false;
            }

            // check wether we should be moving or not
            if (ShouldBeMoving && PathfindingStatus != PathfindingStatus.PathIncomplete)
            {
                // if we already need to jump, dont check it again
                if (!JumpOnNextMove)
                {
                    MovedDistance = LastPlayerPosition.GetDistance(WowInterface.ObjectManager.Player.Position);
                    AmeisenLogger.I.Log("Movement", $"Moved {MovedDistance}m since last check");

                    LastPlayerPosition = WowInterface.ObjectManager.Player.Position;

                    if (MovedDistance > WowInterface.MovementSettings.MinDistanceMovedJumpUnstuck
                        && MovedDistance < WowInterface.MovementSettings.MaxDistanceMovedJumpUnstuck
                        && WowInterface.ObjectManager.IsWorldLoaded)
                    {
                        if (StuckCounter > 0 && WowInterface.ObjectManager.Player.IsMounted)
                        {
                            WowInterface.HookManager.LuaDismissCompanion();
                        }

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
    }
}