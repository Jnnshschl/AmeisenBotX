using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Db.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects;
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

            if (WowInterface.CharacterManager.Equipment.Items.Any(e => e.Value.MaxDurability > 0 
            && ((double)e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
            && WowInterface.Db.TryGetPointsOfInterest(WowInterface.ObjectManager.MapId, PoiType.Repair, WowInterface.ObjectManager.Player.Position, 4096.0, out IEnumerable<Vector3> repairNpcs))
            {
                GoToNpcAndRepair(repairNpcs);
                return;
            }

            if (GrindingSpot == null)
            {
                GrindingSpot = SelectNextGrindingSpot();
                return;
            }

            double distanceToSpot = GrindingSpot.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

            IEnumerable<WowUnit> nearUnits = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(GrindingSpot.Position, GrindingSpot.Radius)
                .Where(e => e.Level >= GrindingSpot.MinLevel
                         && e.Level <= GrindingSpot.MaxLevel
                         && !Blacklist.Contains(e.Guid)
                         && e.Position.GetDistance(GrindingSpot.Position) < GrindingSpot.Radius)
                .OrderBy(e => e.Position.GetDistance2D(WowInterface.ObjectManager.Player.Position));

            if (WowInterface.ObjectManager.Player.IsInCombat && WowInterface.ObjectManager.Player.IsMounted)
            {
                WowInterface.HookManager.LuaDismissCompanion();
            }

            if (distanceToSpot < GrindingSpot.Radius)
            {
                if (nearUnits != null && nearUnits.Any())
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
                        TargetInLos = WowInterface.HookManager.WowIsInLineOfSight(WowInterface.ObjectManager.Player.Position, nearestUnit.Position);
                    }

                    if (nearestUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 20.0 && TargetInLos)
                    {
                        WowInterface.HookManager.WowTargetGuid(nearestUnit.Guid);
                        WowInterface.Globals.ForceCombat = true;
                    }
                    else
                    {
                        if (!WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, nearestUnit.Position))
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
                    else if (!WowInterface.MovementEngine.Path.Any() || WowInterface.Player.Position.GetDistance(TargetPosition) < 3.0f)
                    {
                        MoveToRandomPositionOnSpot();
                    }
                }
            }
            else
            {
                if (WowInterface.ObjectManager.Partymembers.Any(e => e.IsDead || e.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 30.0))
                {
                    WowInterface.MovementEngine.StopMovement();
                    return;
                }

                if (!WowInterface.MovementEngine.Path.Any() || WowInterface.Player.Position.GetDistance(TargetPosition) < 3.0f)
                {
                    MoveToRandomPositionOnSpot();
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

        private void GoToNpcAndRepair(IEnumerable<Vector3> repairNpcs)
        {
            Vector3 repairNpc = repairNpcs.OrderBy(e => e.GetDistance(WowInterface.ObjectManager.Player.Position)).First();

            if (repairNpc.GetDistance(WowInterface.ObjectManager.Player.Position) > 4.0)
            {
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, repairNpc);
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
                WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, TargetPosition);
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