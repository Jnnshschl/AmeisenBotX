using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Grinding.Objects;
using AmeisenBotX.Core.Grinding.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Grinding
{
    public class GrindingEngine
    {
        public GrindingEngine(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
            Blacklist = new List<ulong>();
            TargetInLosEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        public GrindingSpot GrindingSpot { get; private set; }

        public IGrindingProfile Profile { get; set; }

        public Vector3 TargetPosition { get; private set; }

        private List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private int CurrentSpotIndex { get; set; }

        private ulong TargetGuid { get; set; }

        private bool TargetInLos { get; set; }

        private TimegatedEvent TargetInLosEvent { get; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (GrindingSpot == null)
            {
                GrindingSpot = SelectNextGrindingSpot();
                return;
            }

            double distanceToSpot = GrindingSpot.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

            List<WowUnit> nearUnits = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(GrindingSpot.Position, GrindingSpot.Radius)
                .Where(e => e.Level >= GrindingSpot.MinLevel
                         && e.Level <= GrindingSpot.MaxLevel
                         && !Blacklist.Contains(e.Guid)
                         && e.Position.GetDistance(GrindingSpot.Position) < GrindingSpot.Radius)
                .OrderBy(e => e.Position.GetDistance2D(WowInterface.ObjectManager.Player.Position))
                .ToList();

            if (distanceToSpot < GrindingSpot.Radius)
            {
                if (nearUnits != null && nearUnits.Count > 0)
                {
                    WowUnit nearestUnit = nearUnits.FirstOrDefault(e => e.Guid == TargetGuid);

                    bool switchedTarget = false;

                    if (nearestUnit == null)
                    {
                        TargetGuid = nearUnits.First().Guid;
                        nearestUnit = nearUnits.FirstOrDefault(e => e.Guid == TargetGuid);
                        switchedTarget = true;
                    }

                    if (TargetInLosEvent.Run() || switchedTarget)
                    {
                        TargetInLos = WowInterface.HookManager.IsInLineOfSight(WowInterface.ObjectManager.Player.Position, nearestUnit.Position);
                    }

                    if (nearestUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 20.0 && TargetInLos)
                    {
                        WowInterface.HookManager.TargetGuid(nearestUnit.Guid);
                        WowInterface.Globals.ForceCombat = true;
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearestUnit.Position);

                        if (WowInterface.MovementEngine.IsPathIncomplete)
                        {
                            ++BlacklistCounter;

                            if (BlacklistCounter > 2)
                            {
                                WowInterface.MovementEngine.StopMovement();
                                Blacklist.Add(nearestUnit.Guid);
                                BlacklistCounter = 0;
                            }
                        }
                    }
                }
                else
                {
                    GrindingSpot = SelectNextGrindingSpot();
                    TargetPosition = default;
                    return;
                }
            }
            else
            {
                if (WowInterface.ObjectManager.Partymembers.Count > 0
                    && WowInterface.ObjectManager.Partymembers.Any(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 30.0))
                {
                    WowInterface.MovementEngine.StopMovement();
                    return;
                }

                if (TargetPosition == default)
                {
                    TargetPosition = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, GrindingSpot.Position, (float)GrindingSpot.Radius * 0.2f);
                }

                if (WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) < 4.0)
                {
                    TargetPosition = default;
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, TargetPosition);
                }
            }
        }

        public void Exit()
        {
            GrindingSpot = null;
            CurrentSpotIndex = -1;
        }

        public void LoadProfile(IGrindingProfile questProfile)
        {
            Profile = questProfile;
        }

        private GrindingSpot SelectNextGrindingSpot()
        {
            if (Profile == null)
            {
                Vector3 pos = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, 100f);

                return new GrindingSpot()
                {
                    Position = pos != default ? pos : WowInterface.ObjectManager.Player.Position,
                    Radius = 100.0
                };
            }

            List<GrindingSpot> spots = Profile.Spots.Where(e => WowInterface.ObjectManager.Player.Level >= e.MinLevel && WowInterface.ObjectManager.Player.Level <= e.MaxLevel).ToList();

            if (spots.Count == 0)
            {
                spots.AddRange(Profile.Spots.Where(e => e.MinLevel >= Profile.Spots.Max(e => e.MinLevel)));
            }

            if (Profile.RandomizeSpots)
            {
                Random rnd = new Random();
                return spots[rnd.Next(0, spots.Count)];
            }
            else
            {
                ++CurrentSpotIndex;

                if (CurrentSpotIndex >= spots.Count)
                {
                    CurrentSpotIndex = 0;
                }

                return spots[CurrentSpotIndex];
            }
        }
    }
}