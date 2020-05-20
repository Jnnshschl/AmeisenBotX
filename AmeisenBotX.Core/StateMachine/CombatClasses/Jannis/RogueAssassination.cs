using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Character.Inventory.Enums;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class RogueAssassination : BasicCombatClass
    {
        // author: Jannis Höschele

#pragma warning disable IDE0051
        private const string cloakOfShadowsSpell = "Cloak of Shadows";
        private const string coldBloodSpell = "Cold Blood";
        private const string eviscerateSpell = "Eviscerate";
        private const string hungerForBloodSpell = "Hunger for Blood";
        private const string kickSpell = "Kick";
        private const string mutilateSpell = "Mutilate";
        private const string sliceAndDiceSpell = "Slice and Dice";
        private const string sprintSpell = "Sprint";
        private const string stealthSpell = "Stealth";
#pragma warning restore IDE0051

        public RogueAssassination(WowInterface wowInterface, AmeisenBotStateMachine stateMachine) : base(wowInterface, stateMachine)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { sliceAndDiceSpell, () => CastSpellIfPossibleRogue(sliceAndDiceSpell, 0, true, true, 1) },
                { coldBloodSpell, () => CastSpellIfPossibleRogue(coldBloodSpell, 0, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, (x) => CastSpellIfPossibleRogue(kickSpell, x.Guid, true) }
            };

            AutoAttackEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(4000));
        }

        public override string Author => "Jannis";

        public override bool WalkBehindEnemy => true;

        public override WowClass Class => WowClass.Rogue;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string Displayname => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicAgilityComparator(new List<ArmorType>() { ArmorType.SHIEDLS });

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        private TimegatedEvent AutoAttackEvent { get; set; }

        public override void ExecuteCC()
        {
            if (!WowInterface.ObjectManager.Player.IsAutoAttacking && AutoAttackEvent.Run())
            {
                WowInterface.HookManager.StartAutoAttack(WowInterface.ObjectManager.Target);
            }

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossibleRogue(cloakOfShadowsSpell, 0, true)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 16
                        && CastSpellIfPossibleRogue(sprintSpell, 0, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossibleRogue(eviscerateSpell, WowInterface.ObjectManager.TargetGuid, true, true, 5)
                || CastSpellIfPossibleRogue(mutilateSpell, WowInterface.ObjectManager.TargetGuid, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
        }
    }
}