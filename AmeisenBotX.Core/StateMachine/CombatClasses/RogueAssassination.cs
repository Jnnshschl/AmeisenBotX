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
            if (!ObjectManager.Player.IsInCombat && DateTime.Now - LastBuffCheck > TimeSpan.FromSeconds(buffCheckTime))
            {
                HandleBuffs();
            }

            if (IsSpellKnown(hungerForBloodSpell)
                && HasEnoughEnergy(hungerForBloodSpell)
                && !IsOnCooldown(hungerForBloodSpell))
            {
                HookManager.CastSpell(hungerForBloodSpell);
                return;
            }

            if (ObjectManager.Player.HealthPercentage < 20
                && IsSpellKnown(cloakOfShadowsSpell)
                && HasEnoughEnergy(cloakOfShadowsSpell)
                && !IsOnCooldown(cloakOfShadowsSpell))
            {
                HookManager.CastSpell(cloakOfShadowsSpell);
                return;
            }

            WowUnit target = ObjectManager.WowObjects.OfType<WowUnit>().FirstOrDefault(e => e.Guid == ObjectManager.TargetGuid);

            if (target != null)
            {
                if (IsSpellKnown(kickSpell) && DateTime.Now - LastEnemyCastingCheck > TimeSpan.FromSeconds(enemyCastingCheckTime))
                {
                    HandleHammerOfWrath();
                }

                if (IsSpellKnown(sprintSpell)
                    && target.Position.GetDistance2D(ObjectManager.Player.Position) > 6
                    && HasEnoughEnergy(sprintSpell)
                    && !IsOnCooldown(sprintSpell))
                {
                    HookManager.CastSpell(sprintSpell);
                    return;
                }
            }

            if (IsSpellKnown(eviscerateSpell)
                /// && ObjectManager.Player.ComboPoints > 3
                && HasEnoughEnergy(eviscerateSpell)
                && !IsOnCooldown(eviscerateSpell))
            {
                HookManager.CastSpell(eviscerateSpell);
            }

            if (IsSpellKnown(mutilateSpell)
                && HasEnoughEnergy(mutilateSpell)
                && !IsOnCooldown(mutilateSpell))
            {
                HookManager.CastSpell(mutilateSpell);
                return;
            }
        }

        public void OutOfCombatExecute()
        {

        }

        private void HandleBuffs()
        {
            List<string> myBuffs = HookManager.GetBuffs(WowLuaUnit.Player);

            if (IsSpellKnown(sliceAndDiceSpell)
                //// && ObjectManager.Player.ComboPoints > 0
                && !myBuffs.Any(e => e.Equals(sliceAndDiceSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(sliceAndDiceSpell))
            {
                HookManager.CastSpell(sliceAndDiceSpell);
            }

            if (IsSpellKnown(coldBloodSpell)
                && !myBuffs.Any(e => e.Equals(coldBloodSpell, StringComparison.OrdinalIgnoreCase))
                && !IsOnCooldown(coldBloodSpell))
            {
                HookManager.CastSpell(coldBloodSpell);
                return;
            }

            LastBuffCheck = DateTime.Now;
        }

        private void HandleHammerOfWrath()
        {
            (string, int) castinInfo = HookManager.GetUnitCastingInfo(WowLuaUnit.Target);

            if (castinInfo.Item1.Length > 0
                && castinInfo.Item2 > 0
                && IsSpellKnown(kickSpell)
                && !IsOnCooldown(kickSpell))
            {
                HookManager.CastSpell(kickSpell);
                return;
            }

            LastEnemyCastingCheck = DateTime.Now;
        }

        private bool HasEnoughEnergy(string spellName)
            => CharacterManager.SpellBook.Spells.OrderByDescending(e => e.Rank).FirstOrDefault(e => e.Name.Equals(spellName))?.Costs <= ObjectManager.Player.Energy;

        private bool IsOnCooldown(string spellName)
            => HookManager.GetSpellCooldown(spellName) > 0;

        private bool IsSpellKnown(string spellName)
            => CharacterManager.SpellBook.Spells.Any(e => e.Name.Equals(spellName));
    }
}
