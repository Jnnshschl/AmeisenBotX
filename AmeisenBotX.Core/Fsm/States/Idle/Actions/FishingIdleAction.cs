﻿using AmeisenBotX.Common.Math;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Character.Inventory.Objects;
using AmeisenBotX.Core.Data.Objects;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Wow.Cache.Enums;
using AmeisenBotX.Wow.Objects.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Fsm.States.Idle.Actions
{
    public class FishingIdleAction : IIdleAction
    {
        public FishingIdleAction(AmeisenBotInterfaces bot)
        {
            Bot = bot;
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

        public AmeisenBotInterfaces Bot { get; }

        private Random Rnd { get; }

        public bool Enter()
        {
            bool fishingPoleEquipped = IsFishingRodEquipped();
            bool status =  // we cant fish while swimming
                    !Bot.Player.IsSwimming && !Bot.Player.IsFlying
                    // do i have the fishing skill
                    && Bot.Character.Skills.Any(e => e.Key.Contains("fishing", StringComparison.OrdinalIgnoreCase))
                    // do i have a fishing pole in my inventory or equipped
                    && (Bot.Character.Inventory.Items.OfType<WowWeapon>().Any(e => e.WeaponType == WowWeaponType.FISHING_POLES)
                        || IsFishingRodEquipped())
                    // do i know any fishing spot around here
                    && Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.FishingSpot, Bot.Player.Position, 256.0f, out IEnumerable<Vector3> pois);

            if (status)
            {
                Bot.Character.ItemSlotsToSkip.Add(WowEquipmentSlot.INVSLOT_MAINHAND);
                Bot.Character.ItemSlotsToSkip.Add(WowEquipmentSlot.INVSLOT_OFFHAND);
            }

            return status;
        }

        public void Execute()
        {
            if ((CurrentSpot == default || SpotSelected + SpotDuration <= DateTime.UtcNow)
                && !Bot.Player.IsCasting
                && Bot.Db.TryGetPointsOfInterest(Bot.Objects.MapId, PoiType.FishingSpot, Bot.Player.Position, 256.0f, out IEnumerable<Vector3> pois))
            {
                CurrentSpot = pois.ElementAt(Rnd.Next(0, pois.Count() - 1));
                SpotSelected = DateTime.UtcNow;
                SpotDuration = TimeSpan.FromSeconds(new Random().Next(MinDuration, MaxDuration));
            }

            if (CurrentSpot != default)
            {
                if (Bot.Player.Position.GetDistance(CurrentSpot) > 3.5f)
                {
                    Bot.Movement.SetMovementAction(MovementAction.Move, CurrentSpot);
                    return;
                }
                else if (Bot.Wow.WowIsClickToMoveActive())
                {
                    Bot.Movement.StopMovement();
                    return;
                }

                if (!BotMath.IsFacing(Bot.Player.Position, Bot.Player.Rotation, CurrentSpot))
                {
                    Bot.Wow.WowFacePosition(Bot.Player.BaseAddress, Bot.Player.Position, CurrentSpot);
                    return;
                }
            }

            if (!IsFishingRodEquipped())
            {
                IWowItem fishingRod = Bot.Character.Inventory.Items.OfType<WowWeapon>()
                    .FirstOrDefault(e => e.WeaponType == WowWeaponType.FISHING_POLES);

                if (fishingRod != null)
                {
                    Bot.Wow.LuaEquipItem(fishingRod.Name);
                }
            }

            WowGameobject fishingBobber = Bot.Objects.WowObjects.OfType<WowGameobject>()
                .FirstOrDefault(e => e.GameobjectType == WowGameobjectType.FishingBobber && e.CreatedBy == Bot.Wow.PlayerGuid);

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

            if (!Bot.Player.IsCasting || fishingBobber == null)
            {
                Bot.Wow.LuaCastSpell("Fishing");
            }
            else if (fishingBobber.Flags[(int)WowGameobjectFlags.DoesNotDespawn])
            {
                Bot.Wow.WowObjectRightClick(fishingBobber.BaseAddress);
                Bot.Wow.LuaLootEveryThing();
            }
        }

        private bool IsFishingRodEquipped()
        {
            return (Bot.Character.Equipment.Items[WowEquipmentSlot.INVSLOT_MAINHAND] != null
                && ((WowWeapon)Bot.Character.Equipment.Items[WowEquipmentSlot.INVSLOT_MAINHAND]).WeaponType == WowWeaponType.FISHING_POLES);
        }
    }
}