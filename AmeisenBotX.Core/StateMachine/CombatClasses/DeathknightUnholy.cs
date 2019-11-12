using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class DeathknightUnholy : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string unholyPresenceSpell = "Unholy Presence";
        private readonly string icyTouchSpell = "Icy Touch";
        private readonly string scourgeStrikeSpell = "Scourge Strike";
        private readonly string bloodStrikeSpell = "Blood Strike";
        private readonly string plagueStrikeSpell = "Plague Strike";
        private readonly string runeStrikeSpell = "Rune Strike";
        private readonly string strangulateSpell = "Strangulate";
        private readonly string mindFreezeSpell = "Mind Freeze";
        private readonly string summonGargoyleSpell = "Summon Gargoyle";
        private readonly string frostFeverSpell = "Frost Fever";
        private readonly string bloodPlagueSpell = "Blood Plague";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string hornOfWinterSpell = "Horn of Winter";
        private readonly string iceboundFortitudeSpell = "Icebound Fortitude";
        private readonly string armyOfTheDeadSpell = "Army of the Dead";

        private readonly int buffCheckTime = 4;
        private readonly int deBuffCheckTime = 4;
        private readonly int enemyCastingCheckTime = 1;

        public DeathknightUnholy(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

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

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(deBuffCheckTime)
                && HandleDebuffing())
            {
                return;
            }

            if (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                && HandleEnemyCasting())
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 60
                && CastSpellIfPossible(iceboundFortitudeSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(bloodStrikeSpell, false, true))
            {
                return;
            }

            if (CastSpellIfPossible(scourgeStrikeSpell, false, false, true, true))
            {
                return;
            }

            if (CastSpellIfPossible(deathCoilSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(summonGargoyleSpell, true))
            {
                return;
            }
        }

        private bool HandleEnemyCasting()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && (CastSpellIfPossible(mindFreezeSpell, true)
                || CastSpellIfPossible(strangulateSpell, false, true)))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(frostFeverSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(icyTouchSpell, false, false, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(bloodPlagueSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(plagueStrikeSpell, false, false, false, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!myBuffs.Any(e => e.Equals(hornOfWinterSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(hornOfWinterSpell))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(unholyPresenceSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(unholyPresenceSpell))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool CastSpellIfPossible(string spellname, bool needsRuneenergy = false, bool needsBloodrune = false, bool needsFrostrune = false, bool needsUnholyrune = false)
        {
            if (IsSpellKnown(spellname)
                && (needsRuneenergy && HasEnoughRuneenergy(spellname))
                && (needsBloodrune && (HookManager.IsRuneReady(0) || HookManager.IsRuneReady(1)))
                && (needsFrostrune && (HookManager.IsRuneReady(2) || HookManager.IsRuneReady(3)))
                && (needsUnholyrune && (HookManager.IsRuneReady(4) || HookManager.IsRuneReady(5)))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughRuneenergy(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Runeenergy;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));
    }
}
