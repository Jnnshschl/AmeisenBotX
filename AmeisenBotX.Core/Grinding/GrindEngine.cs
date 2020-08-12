using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Cache.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Grinding.Objects;
using AmeisenBotX.Core.Grinding.Profiles;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Movement.SMovementEngine.Enums;
using AmeisenBotX.Core.Statemachine;
using AmeisenBotX.Core.Statemachine.States;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Grinding
{
    public class GrindingEngine
    {
        public GrindingEngine(WowInterface wowInterface, AmeisenBotConfig config, AmeisenBotStateMachine stateMachine)
        {
            WowInterface = wowInterface;
            Config = config;
            StateMachine = stateMachine;

            Blacklist = new List<ulong>();
            TargetInLosEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
        }

        public GrindingSpot GrindingSpot { get; private set; }

        public IGrindingProfile Profile { get; set; }

        public Vector3 TargetPosition { get; private set; }

        private List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private AmeisenBotConfig Config { get; }

        private int CurrentSpotIndex { get; set; }

        private DateTime LookingForEnemiesSince { get; set; }

        private AmeisenBotStateMachine StateMachine { get; }

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

            if (WowInterface.CharacterManager.Equipment.Items.Any(e => e.Value.MaxDurability > 0 && ((double)e.Value.Durability * (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
                && WowInterface.BotCache.PointsOfInterest.TryGetValue((WowInterface.ObjectManager.MapId, PoiType.Repair), out List<Vector3> repairNpcs)
                && repairNpcs.Any(e => e.GetDistance(WowInterface.ObjectManager.Player.Position) < 4096.0))
            {
                GoToNpcAndRepair(repairNpcs);
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
                    LookingForEnemiesSince = default;
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

                        if (WowInterface.MovementEngine.PathfindingStatus == PathfindingStatus.PathIncomplete)
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
                    if (DateTime.Now - LookingForEnemiesSince > TimeSpan.FromSeconds(30))
                    {
                        GrindingSpot = SelectNextGrindingSpot();
                        TargetPosition = default;
                        return;
                    }
                    else
                    {
                        MoveToRandomPositionOnSpot();
                    }
                }
            }
            else
            {
                if (WowInterface.ObjectManager.Partymembers.Count > 0
                    && WowInterface.ObjectManager.Partymembers.Any(e => e.IsDead || e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 30.0))
                {
                    WowInterface.MovementEngine.StopMovement();
                    return;
                }

                MoveToRandomPositionOnSpot();
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

        private void GoToNpcAndRepair(List<Vector3> repairNpcs)
        {
            Vector3 repairNpc = repairNpcs.OrderBy(e => e.GetDistance(WowInterface.ObjectManager.Player.Position)).First();

            if (repairNpc.GetDistance(WowInterface.ObjectManager.Player.Position) > 4.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, repairNpc);
            }
            else
            {
                WowInterface.MovementEngine.StopMovement();
                StateMachine.SetState(BotState.Repairing);
            }
        }

        private void MoveToRandomPositionOnSpot()
        {
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