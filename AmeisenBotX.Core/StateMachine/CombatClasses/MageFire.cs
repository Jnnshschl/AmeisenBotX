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
        // author: Jannis Höschele

        private readonly string arcaneIntellectSpell = "Arcane Intellect";
        private readonly string counterspellSpell = "Counterspell";
        private readonly string evocationSpell = "Evocation";
        private readonly string fireballSpell = "Fireball";
        private readonly string hotstreakSpell = "Hot Streak";
        private readonly string improvedScorchSpell = "Improved Scorch";
        private readonly string livingBombSpell = "Living Bomb";
        private readonly string manaShieldSpell = "Mana Shield";
        private readonly string moltenArmorSpell = "Molten Armor";
        private readonly string pyroblastSpell = "Pyroblast";
        private readonly string scorchSpell = "Scorch";
        private readonly string mirrorImageSpell = "Mirror Image";
        private readonly string iceBlockSpell = "Ice Block";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;
        private readonly int hotstreakCheckTime = 1;

        public MageFire(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private DateTime LastHotstreakCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleDebuffing())
            {
                return;
            }

            if (DateTime.Now - LastHotstreakCheck > TimeSpan.FromSeconds(hotstreakCheckTime)
                && HandlePyroblastAndManaShield())
            {
                return;
            }

            if (DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                && HandleCounterspell())
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 16
                && CastSpellIfPossible(iceBlockSpell,true))
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 40
                && CastSpellIfPossible(evocationSpell,true))
            {
                return;
            }

            if (CastSpellIfPossible(fireballSpell,true))
            {
                return;
            }
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

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (!myBuffs.Any(e => e.Equals(moltenArmorSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(moltenArmorSpell, true))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(arcaneIntellectSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(arcaneIntellectSpell, true))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleCounterspell()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && CastSpellIfPossible(counterspellSpell, true))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(livingBombSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(livingBombSpell, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(scorchSpell, StringComparison.OrdinalIgnoreCase))
                || !targetDebuffs.Any(e => e.Equals(improvedScorchSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(scorchSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
        }

        private bool HandlePyroblastAndManaShield()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (myBuffs.Any(e => e.Equals(hotstreakSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(pyroblastSpell))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(manaShieldSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(manaShieldSpell))
            {
                return true;
            }

            LastHotstreakCheck = DateTime.Now;
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
