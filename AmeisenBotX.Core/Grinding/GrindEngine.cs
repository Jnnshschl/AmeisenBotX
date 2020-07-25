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
        }

        public GrindingSpot GrindingSpot { get; private set; }

        public IGrindingProfile Profile { get; set; }
        public Vector3 TargetPosition { get; private set; }
        private int CurrentSpotIndex { get; set; }

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
                .Where(e => e.Level >= GrindingSpot.MinLevel && e.Level <= GrindingSpot.MaxLevel)
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .ToList();

            if (distanceToSpot < GrindingSpot.Radius)
            {
                if (nearUnits != null && nearUnits.Count > 0)
                {
                    WowUnit nearestUnit = nearUnits.First();

                    if (nearestUnit.Position.GetDistance(WowInterface.ObjectManager.Player.Position) < 20.0)
                    {
                        WowInterface.HookManager.TargetGuid(nearestUnit.Guid);
                        WowInterface.Globals.ForceCombat = true;
                    }
                    else
                    {
                        WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, nearestUnit.Position);
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
                if (TargetPosition == default)
                {
                    TargetPosition = WowInterface.PathfindingHandler.GetRandomPointAround((int)WowInterface.ObjectManager.MapId, WowInterface.ObjectManager.Player.Position, (float)GrindingSpot.Radius * 0.4f);
                }

                if (WowInterface.ObjectManager.Player.Position.GetDistance(TargetPosition) < GrindingSpot.Radius * 0.2)
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