using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Spells.Objects;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using AmeisenBotX.Logging;
using AmeisenBotX.Logging.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class RogueAssassination : BasicCombatClass
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

        public RogueAssassination(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { sliceAndDiceSpell, () => CastSpellIfPossible(sliceAndDiceSpell, true, true, 1) },
                { coldBloodSpell, () => CastSpellIfPossible(coldBloodSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(kickSpell, true) }
            };
        }

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicAgilityComparator();

        public override string Displayname => "[WIP] Rogue Assasination";

        public override string Version => "1.0";

        public override string Author => "Jannis";

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override WowClass Class => WowClass.Rogue;

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (!ObjectManager.Player.IsAutoAttacking)
            {
                HookManager.StartAutoAttack();
            }

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossible(cloakOfShadowsSpell, true)))
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if ((ObjectManager.Target.Position.GetDistance2D(ObjectManager.Player.Position) > 16
                        && CastSpellIfPossible(sprintSpell, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossible(eviscerateSpell, true, true, 5)
                || CastSpellIfPossible(mutilateSpell, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {

        }

        private bool CastSpellIfPossible(string spellName, bool needsEnergy = false, bool needsCombopoints = false, int requiredCombopoints = 1)
        {
            AmeisenLogger.Instance.Log($"[{Displayname}]: Trying to cast \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);

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
                AmeisenLogger.Instance.Log($"[{Displayname}]: Casting Spell \"{spellName}\" on \"{ObjectManager.Target?.Name}\"", LogLevel.Verbose);
                return true;
            }

            return false;
        }
    }
}
