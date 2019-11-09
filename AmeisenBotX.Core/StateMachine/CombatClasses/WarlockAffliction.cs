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
            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleDebuffing();
            }

            if (ObjectManager.Player.ManaPercentage < 90
                && ObjectManager.Player.HealthPercentage > 60
                && IsSpellKnown(lifeTapSpell)
                && !IsOnCooldown(lifeTapSpell))
            {
                HookManager.CastSpell(lifeTapSpell);
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 80
                && IsSpellKnown(deathCoilSpell)
                && !IsOnCooldown(deathCoilSpell))
            {
                HookManager.CastSpell(deathCoilSpell);
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (IsSpellKnown(howlOfTerrorSpell)
                    && target.GetType() == typeof(WowPlayer)
                    && ObjectManager.Player.Position.GetDistance(target.Position) < 6
                    && HasEnoughMana(howlOfTerrorSpell)
                    && !IsOnCooldown(howlOfTerrorSpell))
                {
                    HookManager.CastSpell(howlOfTerrorSpell);
                    return;
                }

                if (IsSpellKnown(fearSpell)
                    && target.GetType() == typeof(WowPlayer)
                    && ObjectManager.Player.Position.GetDistance(target.Position) < 12
                    && HasEnoughMana(fearSpell)
                    && !IsOnCooldown(fearSpell))
                {
                    HookManager.CastSpell(fearSpell);
                    return;
                }

                if (IsSpellKnown(drainSoulSpell)
                    && target.HealthPercentage < 0.25
                    && HasEnoughMana(drainSoulSpell)
                    && !IsOnCooldown(drainSoulSpell))
                {
                    if (ObjectManager.Player.CurrentlyCastingSpellId == 0
                        && ObjectManager.Player.CurrentlyCastingSpellId == 0)
                    {
                        HookManager.CastSpell(drainSoulSpell);
                    }

                    return;
                }
            }

            if (IsSpellKnown(shadowBoltSpell)
                && HasEnoughMana(shadowBoltSpell)
                && !IsOnCooldown(shadowBoltSpell))
            {
                HookManager.CastSpell(shadowBoltSpell);
                return;
            }
        }

        public void OutOfCombatExecute()
        {
            if (DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffing();

                if (IsSpellKnown(sumonImpSpell)
                    && ObjectManager.PetGuid == 0)
                {
                    HookManager.CastSpell(sumonImpSpell);
                    return;
                }
            }
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            HookManager.TargetGuid(ObjectManager.PlayerGuid);

            if (IsSpellKnown(felArmorSpell))
            {
                if (!myBuffs.Any(e => e.Equals(felArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && !IsOnCooldown(felArmorSpell))
                {
                    HookManager.CastSpell(felArmorSpell);
                    return;
                }
            }
            else if (IsSpellKnown(demonArmorSpell))
            {
                if (!myBuffs.Any(e => e.Equals(demonArmorSpell, StringComparison.OrdinalIgnoreCase))
                    && !IsOnCooldown(demonArmorSpell))
                {
                    HookManager.CastSpell(demonArmorSpell);
                    return;
                }
            }
            else if (IsSpellKnown(demonSkinSpell))
            {
                if (!myBuffs.Any(e => e.Equals(demonSkinSpell, StringComparison.OrdinalIgnoreCase))
                    && !IsOnCooldown(demonSkinSpell))
                {
                    HookManager.CastSpell(demonSkinSpell);
                    return;
                }
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (IsSpellKnown(hauntSpell)
                && !targetDebuffs.Any(e => e.Equals(hauntSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(hauntSpell))
            {
                HookManager.CastSpell(hauntSpell);
                return;
            }

            if (IsSpellKnown(unstableAfflictionSpell)
                && !targetDebuffs.Any(e => e.Equals(unstableAfflictionSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(unstableAfflictionSpell))
            {
                HookManager.CastSpell(unstableAfflictionSpell);
                return;
            }

            if (IsSpellKnown(curseOfAgonySpell)
                && !targetDebuffs.Any(e => e.Equals(curseOfAgonySpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(curseOfAgonySpell))
            {
                HookManager.CastSpell(curseOfAgonySpell);
                return;
            }

            if (IsSpellKnown(corruptionSpell)
                && !targetDebuffs.Any(e => e.Equals(corruptionSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(corruptionSpell))
            {
                HookManager.CastSpell(corruptionSpell);
                return;
            }

            LastDebuffCheck = DateTime.Now;
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
