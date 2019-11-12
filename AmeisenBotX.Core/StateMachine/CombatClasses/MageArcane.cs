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
            if (IsSpellKnown(arcaneMissilesSpell) && DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleArcaneMissiles())
            {
                return;
            }

            if (IsSpellKnown(manaShieldSpell) && DateTime.Now - LastManashieldCheck > TimeSpan.FromSeconds(manashieldCheckTime)
                && HandleManaShield())
            {
                return;
            }

            if (IsSpellKnown(counterspellSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                && HandleCounterspell())
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 16
                && CastSpellIfPossible(iceBlockSpell))
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 40
                && CastSpellIfPossible(evocationSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(mirrorImageSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(arcaneBarrageSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(arcaneBlastSpell, true))
            {
                return;
            }
        }

        private bool HandleArcaneMissiles()
        {
            if (BarrageCounter > 0
                && CastSpellIfPossible(arcaneMissilesSpell, true))
            {
                return true;
            }

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

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (!myBuffs.Any(e => e.Equals(mageArmorSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(mageArmorSpell, true))
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

        private bool HandleManaShield()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!myBuffs.Any(e => e.Equals(manaShieldSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(manaShieldSpell))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(missileBarrageSpell, StringComparison.OrdinalIgnoreCase)))
            {
                BarrageCounter = 0;
            }
            else
            {
                BarrageCounter++;
            }

            LastManashieldCheck = DateTime.Now;
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
