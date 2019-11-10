using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.StateMachine.CombatClasses
{
    public class MageArcane : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string arcaneIntellectSpell = "Arcane Intellect";
        private readonly string counterspellSpell = "Counterspell";
        private readonly string evocationSpell = "Evocation";
        private readonly string arcaneBlastSpell = "Arcane Blast";
        private readonly string arcaneBarrageSpell = "Arcane Barrage";
        private readonly string arcaneMissilesSpell = "Arcane Missiles";
        private readonly string missileBarrageSpell = "Missile Barrage";
        private readonly string manaShieldSpell = "Mana Shield";
        private readonly string mageArmorSpell = "Mage Armor";
        private readonly string mirrorImageSpell = "Mirror Image";
        private readonly string iceBlockSpell = "Ice Block";
        private readonly string icyVeinsSpell = "Icy Veins";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;
        private readonly int manashieldCheckTime = 1;

        public MageArcane(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        private DateTime LastManashieldCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private int BarrageCounter { get; set; }

        public void Execute()
        {
            if (IsSpellKnown(arcaneMissilesSpell) && DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleArcaneMissiles();
            }

            if (IsSpellKnown(manaShieldSpell) && DateTime.Now - LastManashieldCheck > TimeSpan.FromSeconds(manashieldCheckTime))
            {
                HandleManaShield();
            }

            if (IsSpellKnown(counterspellSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
            {
                HandleCounterspell();
            }

            if (ObjectManager.Player.HealthPercentage < 16
                && IsSpellKnown(iceBlockSpell)
                && !IsOnCooldown(iceBlockSpell))
            {
                HookManager.CastSpell(iceBlockSpell);
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 40
                && IsSpellKnown(evocationSpell)
                && HasEnoughMana(evocationSpell)
                && !IsOnCooldown(evocationSpell))
            {
                HookManager.CastSpell(evocationSpell);
                return;
            }

            if (IsSpellKnown(mirrorImageSpell)
                && HasEnoughMana(mirrorImageSpell)
                && !IsOnCooldown(mirrorImageSpell))
            {
                HookManager.CastSpell(mirrorImageSpell);
                return;
            }

            if (IsSpellKnown(arcaneBarrageSpell)
                && HasEnoughMana(arcaneBarrageSpell)
                && !IsOnCooldown(arcaneBarrageSpell))
            {
                HookManager.CastSpell(arcaneBarrageSpell);
                return;
            }

            if (IsSpellKnown(arcaneBlastSpell)
                && HasEnoughMana(arcaneBlastSpell)
                && !IsOnCooldown(arcaneBlastSpell))
            {
                HookManager.CastSpell(arcaneBlastSpell);
                BarrageCounter++;
                return;
            }
        }

        private void HandleArcaneMissiles()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (IsSpellKnown(arcaneMissilesSpell)
                && myBuffs.Any(e => e.Equals(arcaneMissilesSpell, StringComparison.OrdinalIgnoreCase))
                && HasEnoughMana(arcaneMissilesSpell)
                && !IsOnCooldown(arcaneMissilesSpell))
            {
                HookManager.CastSpell(arcaneMissilesSpell);
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

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (IsSpellKnown(mageArmorSpell)
                && !myBuffs.Any(e => e.Equals(mageArmorSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(mageArmorSpell))
            {
                HookManager.CastSpell(mageArmorSpell);
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

        private void HandleManaShield()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (IsSpellKnown(manaShieldSpell)
                && !myBuffs.Any(e => e.Equals(manaShieldSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(manaShieldSpell))
            {
                HookManager.CastSpell(manaShieldSpell);
                return;
            }

            if (!myBuffs.Any(e => e.Equals(missileBarrageSpell, StringComparison.OrdinalIgnoreCase)))
            {
                BarrageCounter = 0;
                return;
            }

            LastManashieldCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
                    => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
