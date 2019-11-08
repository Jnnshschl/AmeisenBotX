using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class HunterBeastmastery : ICombatClass
    {
        private readonly string arcaneShotSpell = "Arcane Shot";
        private readonly string aspectOfTheDragonhawkSpell = "Aspect of the Dragonhawk";
        private readonly string beastialWrathSpell = "Beastial Wrath";
        private readonly int buffCheckTime = 30;
        private readonly string callPetSpell = "Call Pet";
        private readonly string concussiveShotSpell = "Concussive Shot";
        private readonly int debuffCheckTime = 3;
        private readonly string deterrenceSpell = "Deterrence";
        private readonly string disengageSpell = "Disengage";
        private readonly int enemyCastingCheckTime = 1;
        private readonly string feignDeathSpell = "Feign Death";
        private readonly string frostTrapSpell = "Frost Trap";
        private readonly string huntersMarkSpell = "Hunter's Mark";
        private readonly string intimidationSpell = "Intimidation";
        private readonly string killCommandSpell = "Kill Command";
        private readonly string killShotSpell = "Kill Shot";
        private readonly string mendPetSpell = "Mend Pet";
        private readonly int petstatusCheckTime = 3;
        private readonly string rapidFireSpell = "Rapid Fire";
        private readonly string revivePetSpell = "Revive Pet";
        private readonly string serpentStingSpell = "Serpent Sting";
        private readonly string steadyShotSpell = "Steady Shot";
        private readonly string wingClipSpell = "Wing Clip";

        public HunterBeastmastery(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private bool Disengaged { get; set; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private DateTime LastMendPetUsed { get; set; }

        private ObjectManager ObjectManager { get; }

        private DateTime PetStatusCheck { get; set; }

        public void Execute()
        {
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (DateTime.Now - PetStatusCheck > TimeSpan.FromSeconds(petstatusCheckTime))
            {
                CheckPetStatus();
            }

            if (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
            {
                HandleIntimmidation();
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleDebuffing();
            }

            WowUnit target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target == null)
            {
                HookManager.TargetNearestEnemy();
                HookManager.StartAutoAttack();
            }

            double distanceToTarget = target.Position.GetDistance2D(ObjectManager.Player.Position);

            if (ObjectManager.Player.HealthPercentage < 15
                && IsSpellKnown(feignDeathSpell)
                && !IsOnCooldown(feignDeathSpell))
            {
                HookManager.CastSpell(feignDeathSpell);
            }

            if (distanceToTarget < 6
                && IsSpellKnown(disengageSpell)
                && HasEnoughMana(disengageSpell)
                && !IsOnCooldown(disengageSpell))
            {
                HookManager.CastSpell(disengageSpell);
                Disengaged = true;
                return;
            }

            if (distanceToTarget < 10)
            {
                if (ObjectManager.Player.HealthPercentage < 50
                    && IsSpellKnown(deterrenceSpell)
                    && HasEnoughMana(deterrenceSpell)
                    && !IsOnCooldown(deterrenceSpell))
                {
                    HookManager.CastSpell(deterrenceSpell);
                }

                if (IsSpellKnown(frostTrapSpell)
                && HasEnoughMana(frostTrapSpell)
                    && !IsOnCooldown(frostTrapSpell))
                {
                    HookManager.CastSpell(frostTrapSpell);
                    return;
                }
            }

            if (distanceToTarget > 3)
            {
                if (Disengaged
                    && IsSpellKnown(concussiveShotSpell)
                    && HasEnoughMana(concussiveShotSpell)
                    && !IsOnCooldown(concussiveShotSpell))
                {
                    HookManager.CastSpell(concussiveShotSpell);
                    Disengaged = false;
                    return;
                }

                if (IsSpellKnown(killShotSpell)
                    && target.HealthPercentage < 20
                    && HasEnoughMana(killShotSpell)
                    && !IsOnCooldown(killShotSpell))
                {
                    HookManager.CastSpell(killShotSpell);
                    return;
                }

                if (IsSpellKnown(killCommandSpell)
                    && HasEnoughMana(killCommandSpell)
                    && !IsOnCooldown(killCommandSpell))
                {
                    HookManager.CastSpell(killCommandSpell);
                }

                if (IsSpellKnown(beastialWrathSpell)
                    && HasEnoughMana(beastialWrathSpell)
                    && !IsOnCooldown(beastialWrathSpell))
                {
                    HookManager.CastSpell(beastialWrathSpell);
                }

                if (IsSpellKnown(rapidFireSpell)
                    && !IsOnCooldown(rapidFireSpell))
                {
                    HookManager.CastSpell(rapidFireSpell);
                }

                if (IsSpellKnown(arcaneShotSpell)
                    && HasEnoughMana(arcaneShotSpell)
                    && !IsOnCooldown(arcaneShotSpell))
                {
                    HookManager.CastSpell(arcaneShotSpell);
                    return;
                }

                if (IsSpellKnown(steadyShotSpell)
                    && HasEnoughMana(steadyShotSpell)
                    && !IsOnCooldown(steadyShotSpell))
                {
                    HookManager.CastSpell(steadyShotSpell);
                    return;
                }
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            Disengaged = false;
        }

        private void CheckPetStatus()
        {
            WowUnit pet = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.PetGuid);

            if (pet?.Health == 0
                && IsSpellKnown(revivePetSpell)
                && !IsOnCooldown(revivePetSpell))
            {
                HookManager.CastSpell(revivePetSpell);
                return;
            }

            // mend pet has a 15 sec HoT
            if (DateTime.Now - LastMendPetUsed > TimeSpan.FromSeconds(15)
                && pet?.HealthPercentage < 80
                && IsSpellKnown(mendPetSpell)
                && !IsOnCooldown(mendPetSpell))
            {
                HookManager.CastSpell(mendPetSpell);
                LastMendPetUsed = DateTime.Now;
                return;
            }

            PetStatusCheck = DateTime.Now;
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (IsSpellKnown(aspectOfTheDragonhawkSpell)
                && !myBuffs.Any(e => e.Equals(aspectOfTheDragonhawkSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(aspectOfTheDragonhawkSpell))
            {
                HookManager.CastSpell(aspectOfTheDragonhawkSpell);
                return;
            }

            if (IsSpellKnown(callPetSpell)
                && ObjectManager.PetGuid == 0
                && !IsOnCooldown(callPetSpell))
            {
                HookManager.CastSpell(callPetSpell);
                return;
            }

            if (DateTime.Now - PetStatusCheck > TimeSpan.FromSeconds(petstatusCheckTime))
            {
                CheckPetStatus();
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target.ToString());

            if (IsSpellKnown(huntersMarkSpell)
                && !targetDebuffs.Any(e => e.Equals(huntersMarkSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(huntersMarkSpell))
            {
                HookManager.CastSpell(huntersMarkSpell);
                return;
            }

            if (IsSpellKnown(serpentStingSpell)
                && !targetDebuffs.Any(e => e.Equals(serpentStingSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(serpentStingSpell))
            {
                HookManager.CastSpell(serpentStingSpell);
                return;
            }

            LastDebuffCheck = DateTime.Now;
        }

        private void HandleIntimmidation()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(intimidationSpell)
                && !IsOnCooldown(intimidationSpell))
            {
                HookManager.CastSpell(intimidationSpell);
                return;
            }

            LastEnemyCastingCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
                    => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
