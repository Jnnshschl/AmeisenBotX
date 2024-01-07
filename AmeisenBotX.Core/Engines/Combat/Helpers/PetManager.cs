﻿using AmeisenBotX.Common.Utils;
using System;

namespace AmeisenBotX.Core.Engines.Combat.Helpers
{
    public class PetManager(AmeisenBotInterfaces bot, TimeSpan healPetCooldown, Func<bool> castMendPetFunction, Func<bool> castCallPetFunction, Func<bool> castRevivePetFunction)
    {
        public AmeisenBotInterfaces Bot { get; set; } = bot;

        public Func<bool> CastCallPet { get; set; } = castCallPetFunction;

        public Func<bool> CastMendPet { get; set; } = castMendPetFunction;

        public Func<bool> CastRevivePet { get; set; } = castRevivePetFunction;

        public TimeSpan HealPetCooldown { get; set; } = healPetCooldown;

        public DateTime LastMendPetUsed { get; private set; }

        private TimegatedEvent CallPetEvent { get; } = new(TimeSpan.FromSeconds(8));

        private bool CallReviveToggle { get; set; }

        private DateTime LastTimeMounted { get; set; }

        public bool Tick()
        {
            if (Bot.Player.IsMounted)
            {
                // dont summon pets while on mount, they despawn when mounted
                LastTimeMounted = DateTime.UtcNow;
                return false;
            }

            if (LastTimeMounted + TimeSpan.FromSeconds(1) > DateTime.UtcNow)
            {
                // only do stuff 1sec after we dismounted pets need a few ms to spawn
                return false;
            }

            if (Bot.Objects.Pet != null)
            {
                if (CastCallPet != null
                    && ((Bot.Objects.Pet.Guid == 0 && CastCallPet.Invoke())
                    || (CastRevivePet != null
                        && Bot.Objects.Pet != null
                        && (Bot.Objects.Pet.Health == 0 || Bot.Objects.Pet.IsDead) && CastRevivePet())))
                {
                    return true;
                }

                if (Bot.Objects.Pet == null || Bot.Objects.Pet.Health == 0 || Bot.Objects.Pet.IsDead)
                {
                    return true;
                }

                if (CastMendPet != null
                    && DateTime.UtcNow - LastMendPetUsed > HealPetCooldown
                    && Bot.Objects.Pet.HealthPercentage < 80.0
                    && CastMendPet.Invoke())
                {
                    LastMendPetUsed = DateTime.UtcNow;
                    return true;
                }
            }
            else if (CastCallPet != null && CallPetEvent.Run() && !Bot.Player.IsCasting)
            {
                if (CallReviveToggle)
                {
                    CastRevivePet();
                }
                else
                {
                    CastCallPet.Invoke();
                }

                CallReviveToggle = !CallReviveToggle;
            }

            return false;
        }
    }
}