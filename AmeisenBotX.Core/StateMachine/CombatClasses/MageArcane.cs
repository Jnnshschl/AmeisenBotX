using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.CombatClasses.Utils;
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
        private readonly string spellStealSpell = "Spellsteal";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;
        private readonly int manashieldCheckTime = 16;
        private readonly int missileBarrageCheckTime = 1;

        public MageArcane(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
        {
            ObjectManager = objectManager;
            CharacterManager = characterManager;
            HookManager = hookManager;
            CooldownManager = new CooldownManager(characterManager.SpellBook.Spells);

            Spells = new Dictionary<string, Spell>();
            CharacterManager.SpellBook.OnSpellBookUpdate += () =>
            {
                Spells.Clear();
                foreach (Spell spell in CharacterManager.SpellBook.Spells)
                {
                    Spells.Add(spell.Name, spell);
                }
            };
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => false;

        public IWowItemComparator ItemComparator => null;

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private DateTime LastManashieldCheck { get; set; }

        private DateTime LastMissileBarrageCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        private int BarrageCounter { get; set; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if ((DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && (HandleArcaneMissiles()
                    || HandleSpellSteal()))
                || (DateTime.Now - LastManashieldCheck > TimeSpan.FromSeconds(manashieldCheckTime)
                    && HandleManaShield())
                || (DateTime.Now - LastMissileBarrageCheck > TimeSpan.FromSeconds(missileBarrageCheckTime)
                    && HandleMissileBarrage())) { return; }

            if (ObjectManager.Target != null)
            {
                if (ObjectManager.Target.IsCasting
                    && CastSpellIfPossible(counterspellSpell, true))
                {
                    return;
                }

                if ((ObjectManager.Player.HealthPercentage < 16
                    && CastSpellIfPossible(iceBlockSpell))
                || (ObjectManager.Player.ManaPercentage < 40
                    && CastSpellIfPossible(evocationSpell, true))
                || CastSpellIfPossible(mirrorImageSpell, true)
                || CastSpellIfPossible(arcaneBarrageSpell, true)
                || CastSpellIfPossible(arcaneBlastSpell, true))
                {
                    return;
                }
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

        private bool HandleSpellSteal()
        {
            CastSpellIfPossible(spellStealSpell, true);
            return false;
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffing())
            {
                return;
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((!myBuffs.Any(e => e.Equals(mageArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(mageArmorSpell, true))
                || (!myBuffs.Any(e => e.Equals(arcaneIntellectSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(arcaneIntellectSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleMissileBarrage()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!myBuffs.Any(e => e.Equals(missileBarrageSpell, StringComparison.OrdinalIgnoreCase)))
            {
                BarrageCounter = 0;
            }
            else
            {
                BarrageCounter++;
            }

            LastMissileBarrageCheck = DateTime.Now;
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

            LastManashieldCheck = DateTime.Now;
            return false;
        }

        private bool CastSpellIfPossible(string spellName, bool needsMana = false)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsMana || Spells[spellName].Costs < ObjectManager.Player.Mana))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }
    }
}
