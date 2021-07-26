using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Engines.Movement.Objects;
using AmeisenBotX.Core.Fsm;
using AmeisenBotX.Wow.Objects;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Movement
{
    public class DefaultMovementEngine : IMovementEngine
    {
        public DefaultMovementEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config, AmeisenBotFsm stateMachine)
        {
            Bot = bot;
            Config = config;

            FindPathEvent = new(TimeSpan.FromMilliseconds(500));
            RefreshPathEvent = new(TimeSpan.FromMilliseconds(500));
            DistanceMovedCheckEvent = new(TimeSpan.FromMilliseconds(500));

            PathQueue = new();
            PlacesToAvoidList = new();

            PlayerVehicle = new(bot, config, stateMachine);
        }

        public float CurrentSpeed { get; private set; }

        public bool IsAllowedToMove => DateTime.UtcNow > MovementBlockedUntil;

        public bool IsMoving { get; private set; }

        public bool IsUnstucking { get; private set; }

        public DateTime LastMovement { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public IEnumerable<Vector3> Path => PathQueue;

        public IEnumerable<(Vector3 position, float radius)> PlacesToAvoid => PlacesToAvoidList.Where(e => DateTime.UtcNow <= e.until).Select(e => (e.position, e.radius));

        public MovementAction Status { get; private set; }

        public Vector3 UnstuckTarget { get; private set; }

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent DistanceMovedCheckEvent { get; }

        private TimegatedEvent FindPathEvent { get; }

        private Vector3 LastTargetPosition { get; set; }

        private DateTime MovementBlockedUntil { get; set; }

        private Queue<Vector3> PathQueue { get; set; }

        private List<(Vector3 position, float radius, DateTime until)> PlacesToAvoidList { get; set; }

        private BasicVehicle PlayerVehicle { get; set; }

        private TimegatedEvent RefreshPathEvent { get; }

        private bool TriedToMountUp { get; set; }

        public void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;

            PlacesToAvoidList.Add((position, radius, now + timeSpan));
            PlacesToAvoidList.RemoveAll(e => now > e.until);
        }

        public void Execute()
        {
            if (IsUnstucking && UnstuckTarget.GetDistance(Bot.Player.Position) < 2.0f)
            {
                IsUnstucking = false;
            }

            if (PathQueue.Count > 0)
            {
                Vector3 currentNode = IsUnstucking ? UnstuckTarget : PathQueue.Peek();
                float distanceToNode = Bot.Player.Position.GetDistance2D(currentNode);

                if (distanceToNode > 1.0f)
                {
                    if (!TriedToMountUp)
                    {
                        float distance = Bot.Player.Position.GetDistance(PathQueue.Last());

                        // try to mount only once per path
                        if (distance > 40.0f
                            && !Bot.Player.IsInCombat
                            && !Bot.Player.IsGhost
                            && !Bot.Player.IsMounted
                            && Bot.Player.IsOutdoors
                            && Bot.Character.Mounts != null
                            && Bot.Character.Mounts.Any())
                        {
                            MountUp();
                            TriedToMountUp = true;
                        }
                    }

                    // we need to move to the node
                    if (IsAllowedToMove && !Bot.Player.IsCasting)
                    {
                        PlayerVehicle.Update(MoveCharacter, Status, currentNode);
                    }
                }
                else
                {
                    // we are at the node
                    PathQueue.Dequeue();
                }
            }
            else
            {
                if (AvoidAoeStuff(Bot.Player.Position, out Vector3 newPosition))
                {
                    SetMovementAction(MovementAction.Move, newPosition);
                }
            }
        }

        public bool IsPositionReachable(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 5.0f)
        {
            // dont search a path into aoe effects
            if (AvoidAoeStuff(position, out Vector3 newPosition))
            {
                position = newPosition;
            }

            path = Bot.PathfindingHandler.GetPath((int)Bot.Objects.MapId, Bot.Player.Position, position);

            if (path != null && path.Any())
            {
                Vector3 lastNode = path.LastOrDefault();
                return lastNode != default && lastNode.GetDistance(position) < maxDistance;
            }

            return false;
        }

        public void PreventMovement(TimeSpan timeSpan)
        {
            StopMovement();
            MovementBlockedUntil = DateTime.UtcNow + timeSpan;
        }

        public void Reset()
        {
            PathQueue.Clear();
            Status = MovementAction.None;
            TriedToMountUp = false;
        }

        public bool SetMovementAction(MovementAction state, Vector3 position, float rotation = 0.0f)
        {
            if (IsAllowedToMove && (PathQueue.Count == 0 || RefreshPathEvent.Ready))
            {
                if (state == MovementAction.DirectMove || Bot.Player.IsFlying || Bot.Player.IsUnderwater)
                {
                    Bot.Character.MoveToPosition(IsUnstucking ? UnstuckTarget : position);
                    Status = state;
                    DistanceMovedJumpCheck();
                }
                else if (FindPathEvent.Run() && IsPositionReachable(position, out IEnumerable<Vector3> path))
                {
                    // if its a new path, we can try to mount again
                    if (path.Last().GetDistance(LastTargetPosition) > 10.0f)
                    {
                        TriedToMountUp = false;
                    }

                    PathQueue.Clear();

                    foreach (Vector3 node in path)
                    {
                        PathQueue.Enqueue(node);
                    }

                    RefreshPathEvent.Run();
                    Status = state;
                    LastTargetPosition = path.Last();
                    return true;
                }
            }

            return false;
        }

        public void StopMovement()
        {
            Reset();
            Bot.Wow.StopClickToMove();
        }

        private static Vector3 GetPositionOutsideOfAoeSpells(Vector3 targetPosition, IEnumerable<(Vector3 position, float radius)> aoeSpells)
        {
            if (aoeSpells.Any())
            {
                // build mean position and move away x meters from it
                // x is the biggest distance we have to move
                Vector3 meanAoePos = BotMath.GetMeanPosition(aoeSpells.Select(e => e.position));
                float distanceToMove = aoeSpells.Max(e => e.radius);

                // get average angle to produce the outgoing angle
                float outgoingAngle = aoeSpells.Average(e => BotMath.GetFacingAngle(e.position, meanAoePos));

                // "repell" the position from the aoe spell
                return BotUtils.MoveAhead(targetPosition, outgoingAngle, distanceToMove + 1.5f);
            }

            return targetPosition;
        }

        private bool AvoidAoeStuff(Vector3 position, out Vector3 newPosition)
        {
            List<(Vector3 position, float radius)> places = new(PlacesToAvoid);
            places.AddRange(Bot.GetAoeSpells(position)
                .Where(e => Bot.GetWowObjectByGuid<IWowUnit>(e.Caster)?.Type == WowObjectType.Unit)
                .Select(e => (e.Position, e.Radius)));

            if (places.Any())
            {
                newPosition = GetPositionOutsideOfAoeSpells(position, places);
                return true;
            }

            newPosition = position;
            return false;
        }

        private void DistanceMovedJumpCheck()
        {
            if (DistanceMovedCheckEvent.Ready)
            {
                if (LastMovement != default && DateTime.UtcNow - LastMovement < TimeSpan.FromSeconds(1))
                {
                    CurrentSpeed = LastPosition.GetDistance(Bot.Player.Position) / (float)(DateTime.UtcNow - LastMovement).TotalSeconds;

                    if (IsUnstucking && CurrentSpeed > 1.0f)
                    {
                        IsUnstucking = false;
                    }

                    if (CurrentSpeed == 0.0f && !IsUnstucking)
                    {
                        // hard stuck
                        IsUnstucking = true;

                        // get position behind us
                        Vector3 positionBehind = BotUtils.MoveAhead(Bot.Player.Position, Bot.Player.Rotation, -Config.MovementSettings.UnstuckDistance);
                        UnstuckTarget = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.Objects.MapId, positionBehind, 5.0f);

                        Reset();
                        SetMovementAction(MovementAction.Move, UnstuckTarget);
                    }
                    else if (CurrentSpeed < 0.1f)
                    {
                        // soft stuck
                        Bot.Character.Jump();
                    }
                }

                LastMovement = DateTime.UtcNow;
                LastPosition = Bot.Player.Position;
                DistanceMovedCheckEvent.Run();
            }
        }

        private void MountUp()
        {
            IEnumerable<WowMount> filteredMounts = Bot.Character.Mounts;

            if (Config.UseOnlySpecificMounts)
            {
                filteredMounts = filteredMounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Any(x => x.Equals(e.Name.Trim(), StringComparison.OrdinalIgnoreCase)));
            }

            if (filteredMounts != null && filteredMounts.Any())
            {
                WowMount mount = filteredMounts.ElementAt(new Random().Next(0, filteredMounts.Count()));
                PreventMovement(TimeSpan.FromSeconds(1));
                Bot.Wow.CallCompanion(mount.Index, "MOUNT");
            }
        }

        private void MoveCharacter(Vector3 positionToGoTo)
        {
            Vector3 node = Bot.PathfindingHandler.MoveAlongSurface((int)Bot.Objects.MapId, Bot.Player.Position, positionToGoTo);

            if (node != default)
            {
                Bot.Character.MoveToPosition(node);
            }

            if (Config.MovementSettings.EnableDistanceMovedJumpCheck)
            {
                DistanceMovedJumpCheck();
            }
        }
    }
}