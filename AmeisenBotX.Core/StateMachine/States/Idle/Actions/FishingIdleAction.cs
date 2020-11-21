using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.States.Idle.Actions
{
    public class FishingIdleAction : IIdleAction
    {
        public FishingIdleAction()
        {
            Rnd = new Random();
        }

        public bool AutopilotOnly => true;

        public DateTime CooldownStart { get; set; }

        public Vector3 CurrentSpot { get; set; }

        public TimeSpan Duration { get; set; }

        public int MaxCooldown => 25 * 60 * 1000;

        public int MaxDuration { get; } = 20 * 60 * 1000;

        public int MinCooldown => 15 * 60 * 1000;

        public int MinDuration { get; } = 15 * 60 * 1000;

        public TimeSpan SpotDuration { get; set; }

        public DateTime SpotSelected { get; set; }

        public bool Started { get; set; }

        private Random Rnd { get; }

        public bool Enter()
        {
            bool fishingPoleEquipped = IsFishingRodEquipped();
            bool status =  // we cant fish while swimming
                    !WowInterface.I.ObjectManager.Player.IsSwimming && !WowInterface.I.ObjectManager.Player.IsFlying
                    // do i have the fishing skill
                    && WowInterface.I.CharacterManager.Skills.Any(e => e.Key.Contains("fishing", StringComparison.OrdinalIgnoreCase))
                    // do i have a fishing pole in my inventory or equipped
                    && (WowInterface.I.CharacterManager.Inventory.Items.OfType<WowWeapon>().Any(e => e.WeaponType == WeaponType.FISHING_POLES)
                        || IsFishingRodEquipped())
                    // do i know any fishing spot around here
                    && WowInterface.I.Db.TryGetPointsOfInterest(WowInterface.I.ObjectManager.MapId, Data.Db.Enums.PoiType.FishingSpot, WowInterface.I.ObjectManager.Player.Position, 256.0, out IEnumerable<Vector3> pois);

            if (status)
            {
                WowInterface.I.CharacterManager.ItemSlotsToSkip.Add(EquipmentSlot.INVSLOT_MAINHAND);
                WowInterface.I.CharacterManager.ItemSlotsToSkip.Add(EquipmentSlot.INVSLOT_OFFHAND);
            }

            return status;
        }

        public void Execute()
        {
            if ((CurrentSpot == default || SpotSelected + SpotDuration <= DateTime.UtcNow)
                && !WowInterface.I.ObjectManager.Player.IsCasting
                && WowInterface.I.Db.TryGetPointsOfInterest(WowInterface.I.ObjectManager.MapId, Data.Db.Enums.PoiType.FishingSpot, WowInterface.I.ObjectManager.Player.Position, 256.0, out IEnumerable<Vector3> pois))
            {
                CurrentSpot = pois.ElementAt(Rnd.Next(0, pois.Count() - 1));
                SpotSelected = DateTime.UtcNow;
                SpotDuration = TimeSpan.FromSeconds(new Random().Next(MinDuration, MaxDuration));
            }

            if (CurrentSpot != default)
            {
                if (WowInterface.I.ObjectManager.Player.Position.GetDistance(CurrentSpot) > 3.5f)
                {
                    WowInterface.I.MovementEngine.SetMovementAction(MovementAction.Moving, CurrentSpot);
                    return;
                }
                else if (WowInterface.I.HookManager.WowIsClickToMoveActive())
                {
                    WowInterface.I.MovementEngine.StopMovement();
                    return;
                }

                if (!BotMath.IsFacing(WowInterface.I.ObjectManager.Player.Position, WowInterface.I.ObjectManager.Player.Rotation, CurrentSpot))
                {
                    WowInterface.I.HookManager.WowFacePosition(WowInterface.I.ObjectManager.Player, CurrentSpot);
                    return;
                }
            }

            if (!IsFishingRodEquipped())
            {
                IWowItem fishingRod = WowInterface.I.CharacterManager.Inventory.Items.OfType<WowWeapon>()
                    .FirstOrDefault(e => e.WeaponType == WeaponType.FISHING_POLES);

                if (fishingRod != null)
                {
                    WowInterface.I.HookManager.LuaEquipItem(fishingRod);
                }
            }

            WowGameobject fishingBobber = WowInterface.I.ObjectManager.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.FishingBobber && e.CreatedBy == WowInterface.I.ObjectManager.Player.Guid);

            if (!Started)
            {
                Started = true;
                CooldownStart = DateTime.UtcNow;
                Duration = TimeSpan.FromSeconds(Rnd.Next(MinDuration, MaxDuration));
            }
            else if (CooldownStart + Duration <= DateTime.UtcNow)
            {
                Started = false;
                CooldownStart = default;
                Duration = default;
                CurrentSpot = default;
                return;
            }

            if (!WowInterface.I.ObjectManager.Player.IsCasting || fishingBobber == null)
            {
                WowInterface.I.HookManager.LuaCastSpell("Fishing");
            }
            else if (fishingBobber.Flags[(int)WowGameobjectFlags.DoesNotDespawn])
            {
                WowInterface.I.HookManager.WowObjectRightClick(fishingBobber);
                WowInterface.I.HookManager.LuaLootEveryThing();
            }
        }

        private bool IsFishingRodEquipped()
        {
            return (WowInterface.I.CharacterManager.Equipment.Items[EquipmentSlot.INVSLOT_MAINHAND] != null
                && ((WowWeapon)WowInterface.I.CharacterManager.Equipment.Items[EquipmentSlot.INVSLOT_MAINHAND]).WeaponType == WeaponType.FISHING_POLES);
        }
    }
}