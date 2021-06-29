using AmeisenBotX.Common.Utils;
using System;

namespace AmeisenBotX.Core.Utils
{
    public class PetManager
    {
        public PetManager(WowInterface wowInterface, TimeSpan healPetCooldown, Func<bool> castMendPetFunction, Func<bool> castCallPetFunction, Func<bool> castRevivePetFunction)
        {
            WowInterface = wowInterface;
            HealPetCooldown = healPetCooldown;
            CastMendPet = castMendPetFunction;
            CastCallPet = castCallPetFunction;
            CastRevivePet = castRevivePetFunction;

            CallPetEvent = new(TimeSpan.FromSeconds(8));
        }

        public Func<bool> CastCallPet { get; set; }

        public Func<bool> CastMendPet { get; set; }

        public Func<bool> CastRevivePet { get; set; }

        public TimeSpan HealPetCooldown { get; set; }

        public DateTime LastMendPetUsed { get; private set; }

        public WowInterface WowInterface { get; set; }

        private TimegatedEvent CallPetEvent { get; }

        private bool CallReviveToggle { get; set; }

        public bool Tick()
        {
            if (WowInterface.Objects.Pet != null)
            {
                if (CastCallPet != null
                    && ((WowInterface.Objects.Pet.Guid == 0 && CastCallPet.Invoke())
                    || CastRevivePet != null
                    && WowInterface.Objects.Pet != null
                    && (WowInterface.Objects.Pet.Health == 0 || WowInterface.Objects.Pet.IsDead) && CastRevivePet()))
                {
                    return true;
                }

                if (WowInterface.Objects.Pet == null || WowInterface.Objects.Pet.Health == 0 || WowInterface.Objects.Pet.IsDead)
                {
                    return true;
                }

                if (CastMendPet != null
                    && DateTime.UtcNow - LastMendPetUsed > HealPetCooldown
                    && WowInterface.Objects.Pet.HealthPercentage < 80.0
                    && CastMendPet.Invoke())
                {
                    LastMendPetUsed = DateTime.UtcNow;
                    return true;
                }
            }
            else if (CastCallPet != null && CallPetEvent.Run() && !WowInterface.Player.IsCasting)
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