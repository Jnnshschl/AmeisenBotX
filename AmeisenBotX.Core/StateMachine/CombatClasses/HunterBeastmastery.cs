using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Common.Enums;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class HunterBeastmastery : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string arcaneShotSpell = "Arcane Shot";
        private readonly string aspectOfTheDragonhawkSpell = "Aspect of the Dragonhawk";
        private readonly string beastialWrathSpell = "Beastial Wrath";
        private readonly string callPetSpell = "Call Pet";
        private readonly string concussiveShotSpell = "Concussive Shot";
        private readonly string deterrenceSpell = "Deterrence";
        private readonly string disengageSpell = "Disengage";
        private readonly string feignDeathSpell = "Feign Death";
        private readonly string frostTrapSpell = "Frost Trap";
        private readonly string huntersMarkSpell = "Hunter's Mark";
        private readonly string intimidationSpell = "Intimidation";
        private readonly string killCommandSpell = "Kill Command";
        private readonly string killShotSpell = "Kill Shot";
        private readonly string mendPetSpell = "Mend Pet";
        private readonly string rapidFireSpell = "Rapid Fire";
        private readonly string revivePetSpell = "Revive Pet";
        private readonly string serpentStingSpell = "Serpent Sting";
        private readonly string steadyShotSpell = "Steady Shot";
        private readonly string wingClipSpell = "Wing Clip";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 3;
        private readonly int enemyCastingCheckTime = 1;
        private readonly int petstatusCheckTime = 2;

        public HunterBeastmastery(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager, XMemory xMemory)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            XMemory = xMemory;
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

        private XMemory XMemory { get; }

        private DateTime PetStatusCheck { get; set; }

        public void Execute()
        {
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (DateTime.Now - PetStatusCheck > TimeSpan.FromSeconds(petstatusCheckTime)
                && CheckPetStatus())
            {
                return;
            }

            if (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                && HandleIntimmidation())
            {
                return;
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleDebuffing())
            {
                return;
            }

            WowUnit target = (WowUnit)ObjectManager.WowObjects.FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                double distanceToTarget = target.Position.GetDistance2D(ObjectManager.Player.Position);

                if (ObjectManager.Player.HealthPercentage < 15
                    && CastSpellIfPossible(feignDeathSpell))
                {
                    return;
                }

                if (distanceToTarget < 3)
                {
                    BotUtils.SendKey(XMemory.Process.MainWindowHandle, new IntPtr((int)VirtualKeys.VK_S), 750, 1250);
                }

                if (distanceToTarget < 10)
                {
                    if (CastSpellIfPossible(frostTrapSpell, true))
                    {
                        return;
                    }

                    CastSpellIfPossible(deterrenceSpell, true);
                }

                if (distanceToTarget < 9
                    && CastSpellIfPossible(disengageSpell, true))
                {
                    Disengaged = true;
                    return;
                }

                if (distanceToTarget > 3)
                {
                    if (Disengaged
                        && CastSpellIfPossible(concussiveShotSpell,true))
                    {
                        Disengaged = false;
                        return;
                    }

                    if (target.HealthPercentage < 20
                        && CastSpellIfPossible(killShotSpell,true))
                    {
                        return;
                    }

                    CastSpellIfPossible(killCommandSpell, true);
                    CastSpellIfPossible(beastialWrathSpell, true);
                    CastSpellIfPossible(rapidFireSpell);

                    if (CastSpellIfPossible(arcaneShotSpell, true))
                    {
                        return;
                    }

                    if (CastSpellIfPossible(steadyShotSpell,true))
                    {
                        return;
                    }
                }
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }

            if (DateTime.Now - PetStatusCheck > TimeSpan.FromSeconds(petstatusCheckTime)
                && CheckPetStatus())
            {
                return;
            }

            Disengaged = false;
        }

        private bool CheckPetStatus()
        {
            WowUnit pet = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.PetGuid);

            if (ObjectManager.PetGuid == 0
                && CastSpellIfPossible(callPetSpell))
            {
                return true;
            }

            if (pet != null
                && (pet.Health == 0 || pet.IsDead)
                && CastSpellIfPossible(revivePetSpell))
            {
                return true;
            }

            // mend pet has a 15 sec HoT
            if (DateTime.Now - LastMendPetUsed > TimeSpan.FromSeconds(15)
                && pet?.HealthPercentage < 80
                && CastSpellIfPossible(mendPetSpell))
            {
                LastMendPetUsed = DateTime.Now;
                return true;
            }

            PetStatusCheck = DateTime.Now;
            return false;
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (!myBuffs.Any(e => e.Equals(aspectOfTheDragonhawkSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(aspectOfTheDragonhawkSpell))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(huntersMarkSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(huntersMarkSpell, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(serpentStingSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(serpentStingSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleIntimmidation()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && CastSpellIfPossible(intimidationSpell))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
        }

        private bool CastSpellIfPossible(string spellname, bool needsMana = false)
        {
            if (IsSpellKnown(spellname)
                && (needsMana && HasEnoughMana(spellname))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));
    }
}
