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
    public class PriestShadow : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string flashHealSpell = "Flash Heal";
        private readonly string hymnOfHopeSpell = "Hymn of Hope";
        private readonly string shadowformSpell = "Shadowform";
        private readonly string shadowfiendSpell = "Shadowfiend";
        private readonly string powerWordFortitudeSpell = "Power Word: Fortitude";
        private readonly string resurrectionSpell = "Resurrection";
        private readonly string vampiricTouchSpell = "Vampiric Touch";
        private readonly string devouringPlagueSpell = "Devouring Plague";
        private readonly string shadowWordPainSpell = "Shadow Word: Pain";
        private readonly string mindBlastSpell = "Mind Blast";
        private readonly string mindFlaySpell = "Mind Flay";
        private readonly string vampiricEmbraceSpell = "Vampiric Embrace";

        private readonly int buffCheckTime = 8;
        private readonly int debuffCheckTime = 1;
        private readonly int deadPartymembersCheckTime = 4;

        public PriestShadow(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        private DateTime LastDeadPartymembersCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (DateTime.Now - LastDebuffCheck > TimeSpan.FromSeconds(debuffCheckTime))
            {
                HandleDebuffing();
            }

            if (ObjectManager.Player.ManaPercentage < 30
                && IsSpellKnown(hymnOfHopeSpell)
                && !IsOnCooldown(hymnOfHopeSpell))
            {
                HookManager.CastSpell(hymnOfHopeSpell);
                return;
            }

            if (ObjectManager.Player.ManaPercentage < 90
                && IsSpellKnown(shadowfiendSpell)
                && !IsOnCooldown(shadowfiendSpell))
            {
                HookManager.CastSpell(shadowfiendSpell);
                return;
            }

            //// if (ObjectManager.Player.HealthPercentage < 70
            ////     && IsSpellKnown(flashHealSpell)
            ////     && !IsOnCooldown(flashHealSpell))
            //// {
            ////     HookManager.CastSpell(flashHealSpell);
            ////     return;
            //// }

            if (IsSpellKnown(mindFlaySpell)
                && ObjectManager.Player.CurrentlyCastingSpellId == 0
                && ObjectManager.Player.CurrentlyChannelingSpellId == 0
                && HasEnoughMana(mindFlaySpell)
                && !IsOnCooldown(mindFlaySpell))
            {
                HookManager.CastSpell(mindFlaySpell);
                return;
            }
        }

        private void HandleDebuffing()
        {
            List<string> targetDebuffs = HookManager.GetDebuffs(WowLuaUnit.Target);

            if (IsSpellKnown(vampiricTouchSpell)
                && HasEnoughMana(vampiricTouchSpell)
                && !targetDebuffs.Any(e => e.Equals(vampiricTouchSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(vampiricTouchSpell))
            {
                HookManager.CastSpell(vampiricTouchSpell);
                return;
            }

            if (IsSpellKnown(devouringPlagueSpell)
                && HasEnoughMana(devouringPlagueSpell)
                && !targetDebuffs.Any(e => e.Equals(devouringPlagueSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(devouringPlagueSpell))
            {
                HookManager.CastSpell(devouringPlagueSpell);
                return;
            }

            if (IsSpellKnown(mindBlastSpell)
                && HasEnoughMana(mindBlastSpell)
                && !targetDebuffs.Any(e => e.Equals(mindBlastSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(mindBlastSpell))
            {
                HookManager.CastSpell(mindBlastSpell);
                return;
            }

            if (IsSpellKnown(shadowWordPainSpell)
                && HasEnoughMana(shadowWordPainSpell)
                && !targetDebuffs.Any(e => e.Equals(shadowWordPainSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(shadowWordPainSpell))
            {
                HookManager.CastSpell(shadowWordPainSpell);
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

            if (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime))
            {
                HandleDeadPartymembers();
            }
        }

        private void HandleBuffing()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);
            
            if (IsSpellKnown(shadowformSpell)
                && !myBuffs.Any(e => e.Equals(shadowformSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(shadowformSpell))
            {
                HookManager.CastSpell(shadowformSpell);
                return;
            }

            if (IsSpellKnown(powerWordFortitudeSpell)
                && !myBuffs.Any(e => e.Equals(powerWordFortitudeSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(powerWordFortitudeSpell))
            {
                HookManager.TargetGuid(ObjectManager.PlayerGuid);
                HookManager.CastSpell(powerWordFortitudeSpell);
                return;
            }

            if (IsSpellKnown(vampiricEmbraceSpell)
                && !myBuffs.Any(e => e.Equals(vampiricEmbraceSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(vampiricEmbraceSpell))
            {
                HookManager.CastSpell(vampiricEmbraceSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleDeadPartymembers()
        {
            if (IsSpellKnown(resurrectionSpell)
                && HasEnoughMana(resurrectionSpell)
                && !IsOnCooldown(resurrectionSpell))
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(resurrectionSpell);
                }
            }
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            HookManager.TargetGuid(possibleTargets.OrderBy(e => e.HealthPercentage).First().Guid);
        }

        private bool HasEnoughMana(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Mana;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
