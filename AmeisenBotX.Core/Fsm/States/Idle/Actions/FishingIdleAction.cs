using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class FishingIdleAction : IIdleAction
    {
        public FishingIdleAction(WowInterface wowInterface)
        {
            WowInterface = wowInterface;
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

        public WowInterface WowInterface { get; }

        private Random Rnd { get; }

        public bool Enter()
        {
            bool fishingPoleEquipped = IsFishingRodEquipped();
            bool status =  // we cant fish while swimming
                    !WowInterface.ObjectManager.Player.IsSwimming && !WowInterface.ObjectManager.Player.IsFlying
                    // do i have the fishing skill
                    && WowInterface.CharacterManager.Skills.Any(e => e.Key.Contains("fishing", StringComparison.OrdinalIgnoreCase))
                    // do i have a fishing pole in my inventory or equipped
                    && (WowInterface.CharacterManager.Inventory.Items.OfType<WowWeapon>().Any(e => e.WeaponType == WowWeaponType.FISHING_POLES)
                        || IsFishingRodEquipped())
                    // do i know any fishing spot around here
                    && WowInterface.Db.TryGetPointsOfInterest(WowInterface.ObjectManager.MapId, Data.Db.Enums.PoiType.FishingSpot, WowInterface.ObjectManager.Player.Position, 256.0, out IEnumerable<Vector3> pois);

            if (status)
            {
                WowInterface.CharacterManager.ItemSlotsToSkip.Add(WowEquipmentSlot.INVSLOT_MAINHAND);
                WowInterface.CharacterManager.ItemSlotsToSkip.Add(WowEquipmentSlot.INVSLOT_OFFHAND);
            }

            return status;
        }

        public void Execute()
        {
            if ((CurrentSpot == default || SpotSelected + SpotDuration <= DateTime.UtcNow)
                && !WowInterface.ObjectManager.Player.IsCasting
                && WowInterface.Db.TryGetPointsOfInterest(WowInterface.ObjectManager.MapId, Data.Db.Enums.PoiType.FishingSpot, WowInterface.ObjectManager.Player.Position, 256.0, out IEnumerable<Vector3> pois))
            {
                CurrentSpot = pois.ElementAt(Rnd.Next(0, pois.Count() - 1));
                SpotSelected = DateTime.UtcNow;
                SpotDuration = TimeSpan.FromSeconds(new Random().Next(MinDuration, MaxDuration));
            }

            if (CurrentSpot != default)
            {
                if (WowInterface.ObjectManager.Player.Position.GetDistance(CurrentSpot) > 3.5f)
                {
                    WowInterface.MovementEngine.SetMovementAction(MovementAction.Move, CurrentSpot);
                    return;
                }
                else if (WowInterface.HookManager.WowIsClickToMoveActive())
                {
                    WowInterface.MovementEngine.StopMovement();
                    return;
                }

                if (!BotMath.IsFacing(WowInterface.ObjectManager.Player.Position, WowInterface.ObjectManager.Player.Rotation, CurrentSpot))
                {
                    WowInterface.HookManager.WowFacePosition(WowInterface.ObjectManager.Player, CurrentSpot);
                    return;
                }
            }

            if (!IsFishingRodEquipped())
            {
                IWowItem fishingRod = WowInterface.CharacterManager.Inventory.Items.OfType<WowWeapon>()
                    .FirstOrDefault(e => e.WeaponType == WowWeaponType.FISHING_POLES);

                if (fishingRod != null)
                {
                    WowInterface.HookManager.LuaEquipItem(fishingRod);
                }
            }

            WowGameobject fishingBobber = WowInterface.ObjectManager.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.FishingBobber && e.CreatedBy == WowInterface.ObjectManager.Player.Guid);

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

            if (!WowInterface.ObjectManager.Player.IsCasting || fishingBobber == null)
            {
                WowInterface.HookManager.LuaCastSpell("Fishing");
            }
            else if (fishingBobber.Flags[(int)WowGameobjectFlags.DoesNotDespawn])
            {
                WowInterface.HookManager.WowObjectRightClick(fishingBobber);
                WowInterface.HookManager.LuaLootEveryThing();
            }
        }

        private bool IsFishingRodEquipped()
        {
            return (WowInterface.CharacterManager.Equipment.Items[WowEquipmentSlot.INVSLOT_MAINHAND] != null
                && ((WowWeapon)WowInterface.CharacterManager.Equipment.Items[WowEquipmentSlot.INVSLOT_MAINHAND]).WeaponType == WowWeaponType.FISHING_POLES);
        }
    }
}