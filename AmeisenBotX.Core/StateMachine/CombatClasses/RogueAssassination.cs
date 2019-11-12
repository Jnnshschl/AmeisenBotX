using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
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
        }

        public bool HandlesMovement => false;

        public bool HandlesTargetSelection => false;

        public bool IsMelee => true;

        public bool InOpener { get; set; }

        private CharacterManager CharacterManager { get; }

        private HookManager HookManager { get; }

        private DateTime LastBuffCheck { get; set; }

        private DateTime LastEnemyCastingCheck { get; set; }

        private ObjectManager ObjectManager { get; }

        public void Execute()
        {
            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (!ObjectManager.Player.IsInCombat && DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime)
                && HandleBuffs())
            {
                return;
            }

            if (CastSpellIfPossible(hungerForBloodSpell, true))
            {
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 20
                && CastSpellIfPossible(cloakOfShadowsSpell, true))
            {
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (IsSpellKnown(kickSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime)
                    && HandleKick())
                {
                    return;
                }

                if (target.Position.GetDistance2D(ObjectManager.Player.Position) > 6
                    && CastSpellIfPossible(sprintSpell, true))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(eviscerateSpell, true))
            {
                return;
            }

            if (CastSpellIfPossible(mutilateSpell, true))
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

            if (!myBuffs.Any(e => e.Equals(sliceAndDiceSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(sliceAndDiceSpell))
            {
                return true;
            }

            if (!myBuffs.Any(e => e.Equals(coldBloodSpell, StringComparison.OrdinalIgnoreCase))
                && CastSpellIfPossible(coldBloodSpell))
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

        private bool CastSpellIfPossible(string spellname, bool needsEnergy = false)
        {
            if (IsSpellKnown(spellname)
                && (needsEnergy && HasEnoughEnergy(spellname))
                && !IsOnCooldown(spellname))
            {
                HookManager.CastSpell(spellname);
                return true;
            }

            return false;
        }

        private bool HasEnoughEnergy(string spellName)
            => CharacterManager.SpellBook.Spells
            .OrderByDescending(e => e.Rank)
            .FirstOrDefault(e => e.Name.Equals(spellName))
            ?.Costs <= ObjectManager.Player.Energy;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells
            .Any(e => e.Name.Equals(spellName));
    }
}
