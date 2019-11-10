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

            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }

            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(deBuffCheckTime))
            {
                HandleDebuffing();
            }

            if (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
            {
                HandleEnemyCasting();
            }

            if (ObjectManager.Player.HealthPercentage < 60
                && IsSpellKnown(iceboundFortitudeSpell)
                && HasEnoughRuneenergy(iceboundFortitudeSpell)
                && !IsOnCooldown(iceboundFortitudeSpell))
            {
                HookManager.CastSpell(iceboundFortitudeSpell);
                return;
            }

            if (IsSpellKnown(bloodStrikeSpell)
                && (HookManager.IsRuneReady(0) || HookManager.IsRuneReady(1))
                && !IsOnCooldown(bloodStrikeSpell))
            {
                HookManager.CastSpell(bloodStrikeSpell);
                return;
            }

            if (IsSpellKnown(scourgeStrikeSpell)
                && (HookManager.IsRuneReady(2) || HookManager.IsRuneReady(3))
                && (HookManager.IsRuneReady(4) || HookManager.IsRuneReady(5))
                && !IsOnCooldown(scourgeStrikeSpell))
            {
                HookManager.CastSpell(scourgeStrikeSpell);
                return;
            }

            if (IsSpellKnown(deathCoilSpell)
                && HasEnoughRuneenergy(deathCoilSpell)
                && !IsOnCooldown(deathCoilSpell))
            {
                HookManager.CastSpell(deathCoilSpell);
                return;
            }

            if (IsSpellKnown(summonGargoyleSpell)
                && HasEnoughRuneenergy(summonGargoyleSpell)
                && !IsOnCooldown(summonGargoyleSpell))
            {
                HookManager.CastSpell(summonGargoyleSpell);
                return;
            }
        }

        private void HandleEnemyCasting()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(mindFreezeSpell)
                && HasEnoughRuneenergy(mindFreezeSpell)
                && !IsOnCooldown(mindFreezeSpell))
            {
                HookManager.CastSpell(mindFreezeSpell);
                return;
            }

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(strangulateSpell)
                && (HookManager.IsRuneReady(0) || HookManager.IsRuneReady(1))
                && !IsOnCooldown(strangulateSpell))
            {
                HookManager.CastSpell(strangulateSpell);
                return;
            }

            LastEnemyCastingCheck = DateTime.Now;
        }

        private void HandleDebuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (IsSpellKnown(icyTouchSpell)
                && (HookManager.IsRuneReady(2) || HookManager.IsRuneReady(3))
                && !myBuffs.Any(e => e.Equals(frostFeverSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(icyTouchSpell))
            {
                HookManager.CastSpell(icyTouchSpell);
                return;
            }

            if (IsSpellKnown(plagueStrikeSpell)
                && (HookManager.IsRuneReady(4) || HookManager.IsRuneReady(5))
                && !myBuffs.Any(e => e.Equals(bloodPlagueSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(plagueStrikeSpell))
            {
                HookManager.CastSpell(plagueStrikeSpell);
                return;
            }

            LastDebuffCheck = DateTime.Now;
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();
            }
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (IsSpellKnown(hornOfWinterSpell)
                && !myBuffs.Any(e => e.Equals(hornOfWinterSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(hornOfWinterSpell))
            {
                HookManager.CastSpell(hornOfWinterSpell);
                return;
            }

            if (IsSpellKnown(unholyPresenceSpell)
                && !myBuffs.Any(e => e.Equals(unholyPresenceSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(unholyPresenceSpell))
            {
                HookManager.CastSpell(unholyPresenceSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private bool HasEnoughRuneenergy(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Runeenergy;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
