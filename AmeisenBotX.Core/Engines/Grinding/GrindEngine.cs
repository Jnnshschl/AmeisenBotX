using AmeisenBotX.Common.Math;
using AmeisenBotX.Common.Utils;
using AmeisenBotX.Core.Engines.Grinding.Objects;
using AmeisenBotX.Core.Engines.Grinding.Profiles;
using AmeisenBotX.Core.Engines.Movement.Enums;
using AmeisenBotX.Core.Logic;
using AmeisenBotX.Core.Logic.Enums;
using AmeisenBotX.Core.Logic.States;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Engines.Grinding
{
    public class DefaultGrindingEngine : IGrindingEngine
    {
        public DefaultGrindingEngine(AmeisenBotInterfaces bot, AmeisenBotConfig config)
        {
            Bot = bot;
            Config = config;

            Blacklist = new();
            TargetInLosEvent = new(TimeSpan.FromMilliseconds(500));
        }

        public GrindingSpot GrindingSpot { get; private set; }

        public IGrindingProfile Profile { get; set; }

        public Vector3 TargetPosition { get; private set; }

        private List<ulong> Blacklist { get; }

        private int BlacklistCounter { get; set; }

        private AmeisenBotInterfaces Bot { get; }

        private AmeisenBotConfig Config { get; }

        private int CurrentSpotIndex { get; set; }

        private DateTime LookingForEnemiesSince { get; set; }

        private ulong TargetGuid { get; set; }

        private bool TargetInLos { get; set; }

        private TimegatedEvent TargetInLosEvent { get; }

        public void Enter()
        {
        }

        public void Execute()
        {
            if (Bot.Character.Equipment.Items.Any(e => e.Value.MaxDurability > 0
                && (e.Value.Durability / (double)e.Value.MaxDurability * 100.0) <= Config.ItemRepairThreshold)
                && Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.Repair, Bot.Player.Position, 4096.0f, out IEnumerable<Vector3> repairNpcs))
            {
                GoToNpcAndRepair(repairNpcs);
                return;
            }

            if (GrindingSpot == null)
            {
                GrindingSpot = SelectNextGrindingSpot();
                return;
            }

            double distanceToSpot = GrindingSpot.Position.GetDistance(Bot.Player.Position);

            IEnumerable<IWowUnit> nearUnits = Bot.GetNearEnemies<IWowUnit>(GrindingSpot.Position, GrindingSpot.Radius)
                .Where(e => e.Level >= GrindingSpot.MinLevel
                         && e.Level <= GrindingSpot.MaxLevel
                         && !Blacklist.Contains(e.Guid)
                         && e.Position.GetDistance(GrindingSpot.Position) < GrindingSpot.Radius)
                .OrderBy(e => e.Position.GetDistance2D(Bot.Player.Position));

            if (Bot.Player.IsInCombat && Bot.Player.IsMounted)
            {
                Bot.Wow.DismissCompanion("MOUNT");
            }

            if (distanceToSpot < GrindingSpot.Radius)
            {
                if (nearUnits != null && nearUnits.Any())
                {
                    LookingForEnemiesSince = default;
                    IWowUnit nearestUnit = nearUnits.FirstOrDefault(e => e.Guid == TargetGuid);

                    bool switchedTarget = false;

                    if (nearestUnit == null)
                    {
                        TargetGuid = nearUnits.First().Guid;
                        nearestUnit = nearUnits.FirstOrDefault(e => e.Guid == TargetGuid);
                        switchedTarget = true;
                    }

                    if (TargetInLosEvent.Run() || switchedTarget)
                    {
                        TargetInLos = Bot.Wow.IsInLineOfSight(Bot.Player.Position, nearestUnit.Position);
                    }

                    if (nearestUnit.Position.GetDistance(Bot.Player.Position) < 20.0f && TargetInLos)
                    {
                        Bot.Wow.ChangeTarget(nearestUnit.Guid);
                        // StateMachine.Get<StateCombat>().Mode = CombatMode.Force;
                    }
                    else
                    {
                        if (!Bot.Movement.SetMovementAction(MovementAction.Move, nearestUnit.Position))
                        {
                            ++BlacklistCounter;

                            if (BlacklistCounter > 2)
                            {
                                Bot.Movement.StopMovement();
                                Blacklist.Add(nearestUnit.Guid);
                                BlacklistCounter = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (DateTime.UtcNow - LookingForEnemiesSince > TimeSpan.FromSeconds(30))
                    {
                        GrindingSpot = SelectNextGrindingSpot();
                        TargetPosition = default;
                        return;
                    }
                    else if (!Bot.Movement.Path.Any() || Bot.Player.Position.GetDistance(TargetPosition) < 3.0f)
                    {
                        MoveToRandomPositionOnSpot();
                    }
                }
            }
            else
            {
                if (Bot.Objects.Partymembers.Any(e => e.IsDead || e.Position.GetDistance(Bot.Player.Position) > 30.0f))
                {
                    Bot.Movement.StopMovement();
                    return;
                }

                if (!Bot.Movement.Path.Any() || Bot.Player.Position.GetDistance(TargetPosition) < 3.0f)
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
            Vector3 repairNpc = repairNpcs.OrderBy(e => e.GetDistance(Bot.Player.Position)).First();

            if (repairNpc.GetDistance(Bot.Player.Position) > 4.0f)
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, repairNpc);
            }
            else
            {
                Bot.Movement.StopMovement();
                // StateMachine.SetState(BotState.Repairing);
            }
        }

        private void MoveToRandomPositionOnSpot()
        {
            if (TargetPosition == default)
            {
                TargetPosition = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.Objects.MapId, GrindingSpot.Position, (float)GrindingSpot.Radius * 0.2f);
            }

            if (Bot.Player.Position.GetDistance(TargetPosition) < 4.0f)
            {
                TargetPosition = default;
            }
            else
            {
                Bot.Movement.SetMovementAction(MovementAction.Move, TargetPosition);
            }
        }

        private GrindingSpot SelectNextGrindingSpot()
        {
            if (Profile == null)
            {
                Vector3 pos = Bot.PathfindingHandler.GetRandomPointAround((int)Bot.Objects.MapId, Bot.Player.Position, 100.0f);

                return new()
                {
                    Position = pos != default ? pos : Bot.Player.Position,
                    Radius = 100.0f
                };
            }

            List<GrindingSpot> spots = Profile.Spots.Where(e => Bot.Player.Level >= e.MinLevel && Bot.Player.Level <= e.MaxLevel).ToList();

            if (spots.Count == 0)
            {
                spots.AddRange(Profile.Spots.Where(e => e.MinLevel >= Profile.Spots.Max(e => e.MinLevel)));
            }

            if (Profile.RandomizeSpots)
            {
                Random rnd = new();
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