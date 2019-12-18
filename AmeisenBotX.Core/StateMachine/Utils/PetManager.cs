using AmeisenBotX.Core.Data.Objects.WowObject;
using System;

namespace AmeisenBotX.Core.StateMachine.Utils
{
    public class PetManager
    {
        public PetManager(WowUnit pet, TimeSpan healPetCooldown, CastMendPetFunction castMendPetFunction, CastCallPetFunction castCallPetFunction, CastRevivePetFunction castRevivePetFunction)
        {
            Pet = pet;
            HealPetCooldown = healPetCooldown;
            CastMendPet = castMendPetFunction;
            CastCallPet = castCallPetFunction;
            CastRevivePet = castRevivePetFunction;
        }

        public delegate bool CastMendPetFunction();
        public delegate bool CastCallPetFunction();
        public delegate bool CastRevivePetFunction();

        public CastMendPetFunction CastMendPet { get; set; }

        public CastCallPetFunction CastCallPet { get; set; }

        public CastRevivePetFunction CastRevivePet { get; set; }

        public TimeSpan HealPetCooldown { get; set; }

        public WowUnit Pet { get; set; }

        public DateTime LastMendPetUsed { get; private set; }

        public bool Tick()
        {
            if (Pet != null)
            {
                if (CastCallPet != null
                    && ((Pet.Guid == 0
                        && CastCallPet.Invoke())
                    || CastRevivePet != null
                        && (Pet != null && ((Pet.Health == 0 || Pet.IsDead)
                            && CastRevivePet.Invoke()))))
                {
                    return true;
                }

                if (Pet == null || Pet.Health == 0 || Pet.IsDead)
                {
                    return true;
                }

                if (CastMendPet != null
                    && (DateTime.Now - LastMendPetUsed > HealPetCooldown
                        && Pet?.HealthPercentage < 80
                        && CastMendPet.Invoke()))
                {
                    LastMendPetUsed = DateTime.Now;
                    return true;
                }
            }

            return false;
        }
    }
}
