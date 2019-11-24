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
    public class RogueAssassination : ICombatClass
    {
        // author: Jannis Höschele

        private readonly string stealthSpell = "Stealth";
        private readonly string hungerForBloodSpell = "Hunger for Blood";
        private readonly string sliceAndDiceSpell = "Slice and Dice";
        private readonly string mutilateSpell = "Mutilate";
        private readonly string coldBloodSpell = "Cold Blood";
        private readonly string eviscerateSpell = "Eviscerate";
        private readonly string cloakOfShadowsSpell = "Cloak of Shadows";
        private readonly string kickSpell = "Kick";
        private readonly string sprintSpell = "Sprint";

        private readonly int buffCheckTime = 8;
        private readonly int enemyCastingCheckTime = 1;

        public RogueAssassination(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager)
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

        public bool IsMelee => true;

        public IWowItemComparator ItemComparator => null;

        public bool InOpener { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        private CooldownManager CooldownManager { get; }

        private Dictionary<string, Spell> Spells { get; }

        public void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.CurrentlyCastingSpellId > 0
                || ObjectManager.Player.CurrentlyChannelingSpellId > 0)
            {
                return;
            }

            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if ((!ObjectManager.Player.IsInCombat && DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                    && HandleBuffs())
                // || CastSpellIfPossible(hungerForBloodSpell, true)
                || (ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossible(cloakOfShadowsSpell, true)))
            {
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if ((CharacterManager.SpellBook.IsSpellKnown(kickSpell) 
                        && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                        && HandleKick())
                    || (target.Position.GetDistance2D(ObjectManager.Player.Position) > 16
                        && CastSpellIfPossible(sprintSpell, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(eviscerateSpell, true, true, 3)
                || CastSpellIfPossible(mutilateSpell, true))
            {
                return;
            }
        }

        public void OutOfCombatExecute()
        {

        }

        private bool HandleBuffs()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if ((!myBuffs.Any(e => e.Equals(sliceAndDiceSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(sliceAndDiceSpell, false, true, 1))
                || (!myBuffs.Any(e => e.Equals(coldBloodSpell, StringComparison.OrdinalIgnoreCase))
                    && CastSpellIfPossible(coldBloodSpell)))
            {
                return true;
            }

            LastBuffCheck = DateTime.Now;
            return false;
        }

        private bool HandleKick()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            bool isCasting = castinInfo.Item1.Length > 0 && castinInfo.Item2 > 0;

            if (isCasting
                && CastSpellIfPossible(kickSpell, true))
            {
                return true;
            }

            LastEnemyCastingCheck = DateTime.Now;
            return false;
        }

        private bool CastSpellIfPossible(string spellName, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1)
        {
            if (!Spells.ContainsKey(spellName))
            {
                Spells.Add(spellName, CharacterManager.SpellBook.GetSpellByName(spellName));
            }

            if (Spells[spellName] != null
                && !CooldownManager.IsSpellOnCooldown(spellName)
                && (!needsEnergy || Spells[spellName].Costs < ObjectManager.Player.Energy)
                && (!needsCombopoints || ObjectManager.Player.ComboPoints >= requiredCombopoints))
            {
                HookManager.CastSpell(spellName);
                CooldownManager.SetSpellCooldown(spellName, (int)HookManager.GetSpellCooldown(spellName));
                return true;
            }

            return false;
        }
    }
}
