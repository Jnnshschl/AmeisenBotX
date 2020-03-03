using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.Statemachine.Utils.AuraManager;
using static AmeisenBotX.Core.Statemachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.Statemachine.CombatClasses.Jannis
{
    public class RogueAssassination : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string cloakOfShadowsSpell = "Cloak of Shadows";
        private readonly string coldBloodSpell = "Cold Blood";
        private readonly string eviscerateSpell = "Eviscerate";
        private readonly string hungerForBloodSpell = "Hunger for Blood";
        private readonly string kickSpell = "Kick";
        private readonly string mutilateSpell = "Mutilate";
        private readonly string sliceAndDiceSpell = "Slice and Dice";
        private readonly string sprintSpell = "Sprint";
        private readonly string stealthSpell = "Stealth";

        public RogueAssassination(WowInterface wowInterface) : base(wowInterface)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { sliceAndDiceSpell, () => CastSpellIfPossibleRogue(sliceAndDiceSpell, true, true, 1) },
                { coldBloodSpell, () => CastSpellIfPossibleRogue(coldBloodSpell, true) }
            };

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossibleRogue(kickSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Rogue;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Assasination Rogue spec.";

        public override string Displayname => "[WIP] Rogue Assasination";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => true;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicAgilityComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (WowInterface.ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (!WowInterface.ObjectManager.Player.IsAutoAttacking)
            {
                WowInterface.HookManager.StartAutoAttack();
            }

            if (MyAuraManager.Tick()
                || TargetInterruptManager.Tick()
                || (WowInterface.ObjectManager.Player.HealthPercentage < 20
                    && CastSpellIfPossibleRogue(cloakOfShadowsSpell, true)))
            {
                return;
            }

            if (WowInterface.ObjectManager.Target != null)
            {
                if ((WowInterface.ObjectManager.Target.Position.GetDistance(WowInterface.ObjectManager.Player.Position) > 16
                        && CastSpellIfPossibleRogue(sprintSpell, true)))
                {
                    return;
                }
            }

            if (CastSpellIfPossibleRogue(eviscerateSpell, true, true, 5)
                || CastSpellIfPossibleRogue(mutilateSpell, true))
            {
                return;
            }
        }

        public override void OutOfCombatExecute()
        {
        }
    }
}