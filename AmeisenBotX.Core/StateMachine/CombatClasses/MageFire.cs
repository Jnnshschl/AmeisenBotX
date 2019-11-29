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
        private readonly string spellStealSpell = "Spellsteal";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int enemyCastingCheckTime = 1;
        private readonly int hotstreakCheckTime = 1;
        private readonly int shieldCheckTime = 16;

        public MageFire(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        public IWowItemComparator ItemComparator { get; } = new BasicIntellectComparator();

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastDebuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private DateTime LastHotstreakCheck { get; set; }

        private DateTime LastShieldCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if ((DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                    && (HandleDebuffing()
                    || HandleSpellSteal()))
                || (DateTime.Now - LastHotstreakCheck > TimeSpan.FromSeconds(hotstreakCheckTime)
                    && HandlePyroblast())
                || (DateTime.Now - LastShieldCheck > TimeSpan.FromSeconds(shieldCheckTime)
                    && HandleManaShield()))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if (ObjectManager.Target.IsCasting
                    && CastSpellIfPossible(counterspellSpell))
                {
                    return;
                }

                if (CastSpellIfPossible(mirrorImageSpell, true)
                    || (ObjectManager.Player.HealthPercentage < 16
                        && CastSpellIfPossible(iceBlockSpell, true))
                    || (ObjectManager.Player.ManaPercentage < 40
                        && CastSpellIfPossible(evocationSpell, true))
                    || CastSpellIfPossible(fireballSpell, true))
                {
                    return;
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
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if ((!myBuffs.Any(e => e.Equals(moltenArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(moltenArmorSpell, true))
                || (!myBuffs.Any(e => e.Equals(arcaneIntellectSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(arcaneIntellectSpell, true)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleSpellSteal()
        {
            CastSpellIfPossible(spellStealSpell, true);
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

            if ((!targetDebuffs.Any(e => e.Equals(scorchSpell, StringComparison.OrdinalIgnoreCase))
                && !targetDebuffs.Any(e => e.Equals(improvedScorchSpell, StringComparison.OrdinalIgnoreCase)))
                && CastSpellIfPossible(scorchSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
            return false;
        }

        private bool HandlePyroblast()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if ((myBuffs.Any(e => e.Equals(hotstreakSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(pyroblastSpell)))
            {
                return true;
            }

            LastHotstreakCheck = DateTime.Now;
            return false;
        }

        private bool HandleManaShield()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if ((!myBuffs.Any(e => e.Equals(manaShieldSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(manaShieldSpell)))
            {
                return true;
            }

            LastShieldCheck = DateTime.Now;
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
