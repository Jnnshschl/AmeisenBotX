using AmeisenBotX.Core.Common;
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
            if (WowInterface.ObjectManager.Pet != null)
            {
                if (CastCallPet != null
                    && ((WowInterface.ObjectManager.Pet.Guid == 0
                        && CastCallPet.Invoke())
                    || CastRevivePet != null
                        && WowInterface.ObjectManager.Pet != null && (WowInterface.ObjectManager.Pet.Health == 0 || WowInterface.ObjectManager.Pet.IsDead)
                            && CastRevivePet()))
                {
                    return true;
                }

                if (WowInterface.ObjectManager.Pet == null || WowInterface.ObjectManager.Pet.Health == 0 || WowInterface.ObjectManager.Pet.IsDead)
                {
                    return true;
                }

                if (CastMendPet != null
                    && DateTime.Now - LastMendPetUsed > HealPetCooldown
                        && WowInterface.ObjectManager.Pet.HealthPercentage < 80
                        && CastMendPet.Invoke())
                {
                    LastMendPetUsed = DateTime.Now;
                    return true;
                }
            }
            else if (CastCallPet != null && CallPetEvent.Run() && !WowInterface.ObjectManager.Player.IsCasting)
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