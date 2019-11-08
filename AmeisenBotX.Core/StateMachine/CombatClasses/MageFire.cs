using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class MageFire : ICombatClass
    {
        private readonly string moltenArmorSpell = "Molten Armor";
        private readonly string arcaneIntellectSpell = "Arcane Intellect";
        private readonly string livingBombSpell = "Living Bomb";
        private readonly string scorchSpell = "Scorch";
        private readonly string improvedScorchSpell = "Improved Scorch";
        private readonly string fireballSpell = "Fireball";
        private readonly string hotstreakSpell = "Hot Streak";
        private readonly string pyroblastSpell = "Pyroblast";
        private readonly string evocationSpell = "Evocation";
        private readonly string manaShieldSpell = "Mana Shield";
        private readonly string counterspellSpell = "Counterspell";

        private readonly int buffCheckTime = 30;
        private readonly int debuffCheckTime = 1;
        private readonly int hotstreakCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;

        public MageFire(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastHotstreakCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleDebuffing();
            }

            if (IsSpellKnown(pyroblastSpell) || IsSpellKnown(manaShieldSpell) && DateTime.Now - LastHotstreakCheck > TimeSpan.FromSeconds(hotstreakCheckTime))
            {
                HandlePyroblastAndManaShield();
            }

            if (IsSpellKnown(pyroblastSpell) || IsSpellKnown(manaShieldSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
            {
                HandleCounterspell();
            }

            if (ObjectManager.Player.ManaPercentage < 40
                && IsSpellKnown(evocationSpell)
                && HasEnoughMana(evocationSpell)
                && !IsOnCooldown(evocationSpell))
            {
                HookManager.CastSpell(evocationSpell);
                return;
            }

            if (IsSpellKnown(fireballSpell)
                && HasEnoughMana(fireballSpell)
                && !IsOnCooldown(fireballSpell))
            {
                HookManager.CastSpell(fireballSpell);
                return;
            }
        }

        private void HandleCounterspell()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(counterspellSpell)
                && !IsOnCooldown(counterspellSpell))
            {
                HookManager.CastSpell(counterspellSpell);
                return;
            }

            LastEnemyCastingCheck = DateTime.Now;
        }

        private void HandlePyroblastAndManaShield()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());

            if (IsSpellKnown(pyroblastSpell)
                && myBuffs.Any(e => e.Equals(hotstreakSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(pyroblastSpell))
            {
                HookManager.CastSpell(pyroblastSpell);
                return;
            }

            if (IsSpellKnown(manaShieldSpell)
                && !myBuffs.Any(e => e.Equals(manaShieldSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(manaShieldSpell))
            {
                HookManager.CastSpell(manaShieldSpell);
                return;
            }

            LastHotstreakCheck = DateTime.Now;
        }

        private void HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target.ToString());

            if (IsSpellKnown(livingBombSpell)
                && !targetDebuffs.Any(e => e.Equals(livingBombSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(livingBombSpell))
            {
                HookManager.CastSpell(livingBombSpell);
                return;
            }

            if (IsSpellKnown(scorchSpell)
                && !targetDebuffs.Any(e => e.Equals(scorchSpell, StringComparison.OrdinalIgnoreCase))
                || !targetDebuffs.Any(e => e.Equals(improvedScorchSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(scorchSpell))
            {
                HookManager.CastSpell(scorchSpell);
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
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player.ToString());
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (IsSpellKnown(moltenArmorSpell)
                && !myBuffs.Any(e => e.Equals(moltenArmorSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(moltenArmorSpell))
            {
                HookManager.CastSpell(moltenArmorSpell);
                return;
            }

            if (IsSpellKnown(arcaneIntellectSpell)
                && !myBuffs.Any(e => e.Equals(arcaneIntellectSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(arcaneIntellectSpell))
            {
                HookManager.CastSpell(arcaneIntellectSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;
    }
}