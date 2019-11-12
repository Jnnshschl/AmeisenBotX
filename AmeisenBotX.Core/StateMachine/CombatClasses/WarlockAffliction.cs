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
    class WarlockAffliction : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string corruptionSpell = "Corruption";
        private readonly string curseOfAgonySpell = "Curse of Agony";
        private readonly string unstableAfflictionSpell = "Unstable Affliction";
        private readonly string hauntSpell = "Haunt";
        private readonly string lifeTapSpell = "Life Tap";
        private readonly string drainSoulSpell = "Drain Soul";
        private readonly string shadowBoltSpell = "Shadow Bolt";
        private readonly string fearSpell = "Fear";
        private readonly string howlOfTerrorSpell = "Howl of Terror";
        private readonly string demonSkinSpell = "Demon Skin";
        private readonly string demonArmorSpell = "Demon Armor";
        private readonly string felArmorSpell = "Fel Armor";
        private readonly string deathCoilSpell = "Death Coil";
        private readonly string sumonImpSpell = "Summon Imp";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;

        public WarlockAffliction(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime)
                && HandleDebuffing())
            {
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 90
                && ObjectManager.Player.HealthPercentage > 60
                && CastSpellIfPossible(lifeTapSpell))
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 80
                && CastSpellIfPossible(deathCoilSpell, true))
            {
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (target.GetType() == typeof(WowPlayer))
                {
                    if (ObjectManager.Player.Position.GetDistance(target.Position) < 6
                        && CastSpellIfPossible(howlOfTerrorSpell, true))
                    {
                        return;
                    }

                    if (ObjectManager.Player.Position.GetDistance(target.Position) < 12
                        && CastSpellIfPossible(fearSpell, true))
                    {
                        return;
                    }
                }

                if (ObjectManager.Player.CurrentlyCastingSpellId == 0
                    && ObjectManager.Player.CurrentlyCastingSpellId == 0
                    && target.HealthPercentage < 0.25
                    && CastSpellIfPossible(drainSoulSpell, true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(shadowBoltSpell, true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();

                if (ObjectManager.PetGuid == 0
                    && CastSpellIfPossible(sumonImpSpell, true))
                {
                    return;
                }
            }
        }

        private bool HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (!ObjectManager.Player.IsInCombat)
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
            }

            if (IsSpellKnown(felArmorSpell))
            {
                if (!myBuffs.Any(e => e.Equals(felArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(felArmorSpell, true))
                {
                    return true;
                }
            }
            else if (IsSpellKnown(demonArmorSpell))
            {
                if (!myBuffs.Any(e => e.Equals(demonArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(demonArmorSpell, true))
                {
                    return true;
                }
            }
            else if (IsSpellKnown(demonSkinSpell))
            {
                if (!myBuffs.Any(e => e.Equals(demonSkinSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(demonSkinSpell, true))
                {
                    return true;
                }
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (!targetDebuffs.Any(e => e.Equals(hauntSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(hauntSpell, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(unstableAfflictionSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(unstableAfflictionSpell, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(curseOfAgonySpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(curseOfAgonySpell, true))
            {
                return true;
            }

            if (!targetDebuffs.Any(e => e.Equals(corruptionSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(corruptionSpell, true))
            {
                return true;
            }

            LastDebuffCheck = DateTime.Now;
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
