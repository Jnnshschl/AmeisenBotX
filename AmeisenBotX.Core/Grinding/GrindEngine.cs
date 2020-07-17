using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Grinding.Objects;
using AmeisenBotX.Core.Grinding.Profiles;
using AmeisenBotX.Core.Movement.Enums;
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

        private int CurrentSpotIndex { get; set; }

        private WowInterface WowInterface { get; }

        public void Execute()
        {
            if (Profile == null)
            {
                return;
            }

            if (GrindingSpot == null)
            {
                GrindingSpot = SelectNextGrindingSpot();
                return;
            }

            double distanceToSpot = GrindingSpot.Position.GetDistance(WowInterface.ObjectManager.Player.Position);

            List<WowUnit> nearUnits = WowInterface.ObjectManager.GetNearEnemies<WowUnit>(GrindingSpot.Position, GrindingSpot.Radius)
                .OrderBy(e => e.Position.GetDistance(WowInterface.ObjectManager.Player.Position))
                .ToList();

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
                if (distanceToSpot < 8.0)
                {
                    GrindingSpot = SelectNextGrindingSpot();
                    return;
                }
                else
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Moving, GrindingSpot.Position);
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
            if (Profile.RandomizeSpots)
            {
                Random rnd = new Random();
                return Profile.Spots[rnd.Next(0, Profile.Spots.Count)];
            }
            else
            {
                ++CurrentSpotIndex;

                if (CurrentSpotIndex >= Profile.Spots.Count)
                {
                    CurrentSpotIndex = 0;
                }

                return Profile.Spots[CurrentSpotIndex];
            }
        }
    }
}